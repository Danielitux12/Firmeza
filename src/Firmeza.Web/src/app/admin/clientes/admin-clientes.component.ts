import { Component, OnInit, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AdminService } from '../../services/admin.service';
import { Cliente, SaveCliente } from '../../models/cliente.model';

@Component({
  selector: 'app-admin-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  templateUrl: './admin-clientes.component.html'
})
export class AdminClientesComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly cdr = inject(ChangeDetectorRef);

  activeTab: 'activos' | 'suspendidos' = 'activos';
  clientes: Cliente[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 1;
  search = '';
  role = 'all';

  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Control de dropdown de acciones flotante (fixed)
  activeDropdownId: string | null = null;
  activeCliente: Cliente | null = null;
  lastDropdownTrigger: HTMLElement | null = null;
  dropdownPosition = { top: '0px', bottom: 'auto', left: '0px', right: 'auto' };

  // Modal de Confirmación de Acción (Suspender / Eliminar / Reactivar)
  showConfirmModal = false;
  confirmActionType: 'suspend' | 'delete' | 'reactivate' = 'suspend';
  confirmModalTitle = '';
  confirmModalMessage = '';
  targetCliente: Cliente | null = null;

  // Modal de Crear / Editar
  showModal = false;
  modalTitle = 'Nuevo Cliente';
  modalErrorMessage = '';
  isEditing = false;
  currentCliente: SaveCliente = this.getEmptyCliente();
  lastTriggerElement: HTMLElement | null = null;
  initialSnapshot = '';
  touchedFields: Record<string, boolean> = {};
  attemptedSubmit = false;

  // Validaciones sencillas de formulario
  nameError = '';
  documentError = '';
  emailError = '';
  ageError = '';
  phoneError = '';
  addressError = '';

  // Ordenamiento en cliente
  sortColumn: 'name' | 'documentNumber' | 'age' | null = null;
  sortDirection: 'asc' | 'desc' = 'asc';

  get showingStart(): number {
    return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get showingEnd(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  get hasMultipleRoles(): boolean {
    if (!this.clientes || this.clientes.length <= 1) return false;
    const firstRole = this.clientes[0].role || 'Cliente';
    return this.clientes.some(c => (c.role || 'Cliente') !== firstRole);
  }

  get showRoleColumn(): boolean {
    if (this.role !== 'all') return true;
    return this.hasMultipleRoles;
  }

  onSort(column: 'name' | 'documentNumber' | 'age'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.applyClientSort();
  }

  private applyClientSort(): void {
    if (!this.sortColumn || !this.clientes) return;
    const col = this.sortColumn;
    const dir = this.sortDirection === 'asc' ? 1 : -1;

    this.clientes.sort((a, b) => {
      if (col === 'age') {
        const valA = a.age ?? 0;
        const valB = b.age ?? 0;
        return (valA - valB) * dir;
      } else if (col === 'name') {
        const valA = (a.name || '').toLowerCase();
        const valB = (b.name || '').toLowerCase();
        return valA.localeCompare(valB) * dir;
      } else if (col === 'documentNumber') {
        const valA = a.documentNumber || '';
        const valB = b.documentNumber || '';
        return valA.localeCompare(valB, undefined, { numeric: true }) * dir;
      }
      return 0;
    });
  }

  formatPhone(phone: string | null | undefined): string {
    if (!phone) return '—';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 10) {
      return `+57 ${digits.slice(0, 3)} ${digits.slice(3, 6)} ${digits.slice(6)}`;
    }
    if (digits.length === 12 && digits.startsWith('57')) {
      const local = digits.slice(2);
      return `+57 ${local.slice(0, 3)} ${local.slice(3, 6)} ${local.slice(6)}`;
    }
    return phone;
  }

  getInitial(name: string | null | undefined): string {
    if (!name) return 'C';
    const trimmed = name.trim();
    return trimmed.charAt(0).toUpperCase();
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.pageSize = Number(target.value) || 10;
    this.page = 1;
    this.loadClientes();
  }

  toggleDropdown(cliente: Cliente, event: MouseEvent): void {
    event.stopPropagation();
    if (this.activeDropdownId === cliente.id) {
      this.closeDropdown();
      return;
    }
    const buttonEl = event.currentTarget as HTMLElement;
    this.lastDropdownTrigger = buttonEl;
    this.activeCliente = cliente;
    this.activeDropdownId = cliente.id || null;

    const rect = buttonEl.getBoundingClientRect();
    const menuWidth = 160;
    const menuEstimatedHeight = 120;
    const spaceBelow = window.innerHeight - rect.bottom;
    const openUp = spaceBelow < menuEstimatedHeight && rect.top > menuEstimatedHeight;

    this.dropdownPosition = {
      top: openUp ? 'auto' : `${rect.bottom + 4}px`,
      bottom: openUp ? `${window.innerHeight - rect.top + 4}px` : 'auto',
      left: `${Math.max(8, rect.right - menuWidth)}px`,
      right: 'auto'
    };

    setTimeout(() => {
      const firstItem = document.querySelector<HTMLElement>('.table-dropdown-fixed .table-dropdown-item');
      firstItem?.focus();
    }, 0);
  }

  closeDropdown(): void {
    if (this.activeDropdownId) {
      this.activeDropdownId = null;
      this.activeCliente = null;
      setTimeout(() => this.lastDropdownTrigger?.focus(), 0);
    }
  }

  onDropdownKeyDown(event: KeyboardEvent): void {
    const items = Array.from(document.querySelectorAll<HTMLElement>('.table-dropdown-fixed .table-dropdown-item:not(:disabled)'));
    if (!items.length) return;
    const activeIdx = items.indexOf(document.activeElement as HTMLElement);

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      const nextIdx = (activeIdx + 1) % items.length;
      items[nextIdx]?.focus();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      const prevIdx = (activeIdx - 1 + items.length) % items.length;
      items[prevIdx]?.focus();
    } else if (event.key === 'Escape') {
      event.preventDefault();
      this.closeDropdown();
    } else if (event.key === 'Tab') {
      this.closeDropdown();
    }
  }

  @HostListener('window:scroll')
  @HostListener('window:resize')
  onWindowChange(): void {
    if (this.activeDropdownId) {
      this.activeDropdownId = null;
      this.activeCliente = null;
    }
  }

  ngOnInit(): void {
    this.loadClientes();
  }

  setTab(tab: 'activos' | 'suspendidos'): void {
    if (this.activeTab !== tab) {
      this.activeTab = tab;
      this.page = 1;
      this.search = '';
      this.role = 'all';
      this.errorMessage = '';
      this.successMessage = '';
      this.activeDropdownId = null;
      this.loadClientes();
    }
  }

  loadClientes(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.activeDropdownId = null;
    this.cdr.markForCheck();
    const statusFilter = this.activeTab === 'activos' ? 'active' : 'inactive';
    this.adminService.getClientes(this.page, this.pageSize, this.search, this.role, statusFilter).subscribe({
      next: (result) => {
        this.clientes = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 1;
        if (this.sortColumn) {
          this.applyClientSort();
        }
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'No se pudo cargar la lista de clientes. Verifica la conexión a la API.';
        console.error('Error al cargar clientes:', err);
        this.cdr.markForCheck();
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadClientes();
  }

  onRoleChange(): void {
    this.page = 1;
    this.loadClientes();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadClientes();
    }
  }

  openCreateModal(): void {
    this.lastTriggerElement = document.activeElement as HTMLElement | null;
    this.isEditing = false;
    this.modalTitle = 'Registrar nuevo cliente';
    this.currentCliente = this.getEmptyCliente();
    this.initialSnapshot = JSON.stringify(this.currentCliente);
    this.clearErrors();
    this.touchedFields = {};
    this.attemptedSubmit = false;
    this.modalErrorMessage = '';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.activeDropdownId = null;
    this.cdr.markForCheck();
  }

  openEditModal(cliente: Cliente): void {
    this.lastTriggerElement = document.activeElement as HTMLElement | null;
    this.isEditing = true;
    this.modalTitle = 'Editar cliente';
    this.currentCliente = {
      id: cliente.id,
      name: cliente.name,
      documentNumber: cliente.documentNumber,
      email: cliente.email,
      phone: cliente.phone || '',
      age: cliente.age,
      address: cliente.address || '',
      isActive: cliente.isActive
    };
    this.initialSnapshot = JSON.stringify(this.currentCliente);
    this.clearErrors();
    this.touchedFields = {};
    this.attemptedSubmit = false;
    this.modalErrorMessage = '';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.activeDropdownId = null;
    this.cdr.markForCheck();
  }

  hasUnsavedChanges(): boolean {
    return JSON.stringify(this.currentCliente) !== this.initialSnapshot;
  }

  onBackdropClick(event: MouseEvent): void {
    if (!this.hasUnsavedChanges()) {
      this.closeModal();
    }
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event?: Event): void {
    if (this.activeDropdownId) {
      this.closeDropdown();
      return;
    }
    if (this.showModal) {
      if (!this.hasUnsavedChanges()) {
        this.closeModal();
      }
    } else if (this.showConfirmModal) {
      this.cancelConfirm();
    }
  }

  closeModal(): void {
    this.showModal = false;
    this.clearErrors();
    this.touchedFields = {};
    this.attemptedSubmit = false;
    this.modalErrorMessage = '';
    this.cdr.markForCheck();
    setTimeout(() => this.lastTriggerElement?.focus(), 0);
  }

  clearErrors(): void {
    this.nameError = '';
    this.documentError = '';
    this.emailError = '';
    this.ageError = '';
    this.phoneError = '';
    this.addressError = '';
    this.modalErrorMessage = '';
    this.cdr.markForCheck();
  }

  markTouched(field: string): void {
    this.touchedFields[field] = true;
    this.validateField(field);
  }

  isFieldInvalid(field: string): boolean {
    if (!this.touchedFields[field] && !this.attemptedSubmit) return false;
    return !!this.getFieldError(field);
  }

  getFieldError(field: string): string {
    switch (field) {
      case 'name': return this.nameError;
      case 'documentNumber': return this.documentError;
      case 'age': return this.ageError;
      case 'email': return this.emailError;
      case 'phone': return this.phoneError;
      case 'address': return this.addressError;
      default: return '';
    }
  }

  validateField(field: string): void {
    if (field === 'name') {
      const name = (this.currentCliente.name || '').trim();
      const nameRegex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.,\-']+$/;
      if (!name) {
        this.nameError = 'El nombre completo es obligatorio.';
      } else if (name.length < 3 || name.length > 150) {
        this.nameError = 'El nombre debe tener entre 3 y 150 caracteres.';
      } else if (!nameRegex.test(name)) {
        this.nameError = 'El nombre solo debe contener letras y espacios.';
      } else {
        this.nameError = '';
      }
    } else if (field === 'documentNumber') {
      const doc = (this.currentCliente.documentNumber || '').trim();
      const docRegex = /^[0-9]{6,12}$/;
      if (!doc) {
        this.documentError = 'El número de documento es obligatorio.';
      } else if (doc.length < 6 || doc.length > 12) {
        this.documentError = 'El documento debe tener entre 6 y 12 dígitos.';
      } else if (!docRegex.test(doc)) {
        this.documentError = 'El documento debe contener únicamente dígitos numéricos.';
      } else {
        this.documentError = '';
      }
    } else if (field === 'email') {
      const email = (this.currentCliente.email || '').trim();
      const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      if (!email) {
        this.emailError = 'El correo electrónico es obligatorio.';
      } else if (email.length > 150) {
        this.emailError = 'El correo no puede exceder 150 caracteres.';
      } else if (!emailRegex.test(email)) {
        this.emailError = 'Ingresa un correo electrónico válido.';
      } else {
        this.emailError = '';
      }
    } else if (field === 'age') {
      const rawAge = this.currentCliente.age as unknown;
      if (rawAge === undefined || rawAge === null || rawAge === '') {
        this.ageError = 'La edad es obligatoria.';
      } else {
        const age = Number(rawAge);
        if (isNaN(age) || age < 18) {
          this.ageError = 'El cliente debe ser mayor de edad (18 años o más).';
        } else if (age > 120) {
          this.ageError = 'Ingresa una edad válida (máximo 120 años).';
        } else {
          this.ageError = '';
        }
      }
    } else if (field === 'phone') {
      const phone = (this.currentCliente.phone || '').trim();
      const phoneRegex = /^[0-9+\-\s()]{7,20}$/;
      if (!phone) {
        this.phoneError = 'El teléfono de contacto es obligatorio.';
      } else if (!phoneRegex.test(phone)) {
        this.phoneError = 'Ingresa un número de teléfono válido (7 a 20 dígitos).';
      } else {
        this.phoneError = '';
      }
    } else if (field === 'address') {
      const address = (this.currentCliente.address || '').trim();
      if (!address) {
        this.addressError = 'La dirección de domicilio es obligatoria.';
      } else if (address.length < 5 || address.length > 250) {
        this.addressError = 'La dirección debe tener entre 5 y 250 caracteres.';
      } else {
        this.addressError = '';
      }
    }
  }

  validateClienteForm(): boolean {
    this.attemptedSubmit = true;
    this.validateField('name');
    this.validateField('documentNumber');
    this.validateField('email');
    this.validateField('age');
    this.validateField('phone');
    this.validateField('address');

    return !this.nameError && !this.documentError && !this.emailError && !this.ageError && !this.phoneError && !this.addressError;
  }

  saveCliente(): void {
    if (!this.validateClienteForm()) {
      return;
    }

    this.isSaving = true;
    this.modalErrorMessage = '';
    this.errorMessage = '';
    this.cdr.markForCheck();

    if (this.isEditing) {
      if (!this.currentCliente.id) {
        this.isSaving = false;
        this.modalErrorMessage = 'No se encontró el ID del cliente para actualizar.';
        this.cdr.markForCheck();
        return;
      }

      this.adminService.updateCliente(this.currentCliente.id, this.currentCliente).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.successMessage = 'Cliente actualizado correctamente.';
          this.loadClientes();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.detail || err.error?.title || 'Error al actualizar el cliente.';
          this.modalErrorMessage = msg;
          this.errorMessage = msg;
          this.cdr.markForCheck();
        }
      });
    } else {
      this.adminService.createCliente(this.currentCliente).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.successMessage = 'Cliente registrado exitosamente.';
          this.loadClientes();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.detail || err.error?.title || 'Error al registrar el cliente.';
          this.modalErrorMessage = msg;
          this.errorMessage = msg;
          this.cdr.markForCheck();
        }
      });
    }
  }

  openConfirmModal(cliente: Cliente, action: 'suspend' | 'delete' | 'reactivate'): void {
    this.targetCliente = cliente;
    this.confirmActionType = action;
    this.activeDropdownId = null;

    if (action === 'suspend') {
      this.confirmModalTitle = 'Suspender cliente';
      this.confirmModalMessage = `¿Confirmas que deseas suspender al cliente «${cliente.name}»? Pasará al apartado de suspendidos y se deshabilitará su acceso.`;
    } else if (action === 'delete') {
      this.confirmModalTitle = 'Eliminar permanentemente';
      this.confirmModalMessage = `¿Confirmas que deseas eliminar definitivamente al cliente «${cliente.name}»? Esta acción no se puede deshacer.`;
    } else if (action === 'reactivate') {
      this.confirmModalTitle = 'Reactivar cliente';
      this.confirmModalMessage = `¿Confirmas que deseas reactivar al cliente «${cliente.name}»? Volverá a la cartera activa.`;
    }

    this.showConfirmModal = true;
    this.cdr.markForCheck();
  }

  cancelConfirm(): void {
    this.showConfirmModal = false;
    this.targetCliente = null;
    this.cdr.markForCheck();
  }

  executeConfirmedAction(): void {
    const cliente = this.targetCliente;
    if (!cliente || !cliente.id) return;
    const id = cliente.id;
    this.showConfirmModal = false;

    if (this.confirmActionType === 'suspend') {
      this.adminService.deleteCliente(id).subscribe({
        next: () => {
          this.successMessage = `Cliente «${cliente.name}» suspendido correctamente.`;
          this.loadClientes();
        },
        error: (err) => {
          this.errorMessage = `Error al suspender: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    } else if (this.confirmActionType === 'reactivate') {
      this.adminService.activateCliente(id).subscribe({
        next: () => {
          this.successMessage = `Cliente «${cliente.name}» reactivado correctamente.`;
          this.loadClientes();
        },
        error: (err) => {
          this.errorMessage = `Error al reactivar: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    } else if (this.confirmActionType === 'delete') {
      this.adminService.deletePermanentCliente(id).subscribe({
        next: () => {
          this.successMessage = `Cliente «${cliente.name}» eliminado definitivamente.`;
          this.loadClientes();
        },
        error: (err) => {
          this.errorMessage = `Error al eliminar: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    }
  }

  downloadExcel(): void {
    this.adminService.exportClientesExcel().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Clientes_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = 'Error al descargar el archivo Excel.';
      }
    });
  }

  downloadPdf(): void {
    this.adminService.exportClientesPdf().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Clientes_${new Date().toISOString().slice(0, 10)}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = 'Error al descargar el archivo PDF.';
      }
    });
  }

  private getEmptyCliente(): SaveCliente {
    return {
      name: '',
      documentNumber: '',
      email: '',
      phone: '',
      age: undefined as unknown as number,
      address: '',
      isActive: true
    };
  }
}
