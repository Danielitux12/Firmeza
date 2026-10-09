import { Component, OnInit, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../../shared/navbar/navbar.component';
import { AdminService } from '../../services/admin.service';
import { Empresa, SaveEmpresa } from '../../models/empresa.model';

@Component({
  selector: 'app-admin-empresas',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  templateUrl: './admin-empresas.component.html'
})
export class AdminEmpresasComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly cdr = inject(ChangeDetectorRef);

  activeTab: 'activas' | 'inactivas' = 'activas';
  empresas: Empresa[] = [];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 1;
  search = '';

  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  // Control de dropdown de acciones flotante (fixed)
  activeDropdownId: string | null = null;
  activeEmpresa: Empresa | null = null;
  lastDropdownTrigger: HTMLElement | null = null;
  dropdownPosition = { top: '0px', bottom: 'auto', left: '0px', right: 'auto' };

  // Modal de Confirmación de Acción (Inactivar / Reactivar / Eliminar)
  showConfirmModal = false;
  confirmActionType: 'suspend' | 'activate' | 'delete' = 'suspend';
  confirmModalTitle = '';
  confirmModalMessage = '';
  targetEmpresa: Empresa | null = null;

  // Modal Crear / Editar
  showModal = false;
  modalTitle = 'Nueva Empresa';
  modalErrorMessage = '';
  isEditing = false;
  currentEmpresa: SaveEmpresa = this.getEmptyEmpresa();
  lastTriggerElement: HTMLElement | null = null;
  initialSnapshot = '';
  touchedFields: Record<string, boolean> = {};
  attemptedSubmit = false;

  // Validaciones de formulario
  nameError = '';
  nitError = '';
  emailError = '';
  phoneError = '';
  addressError = '';

  // Ordenamiento en cliente
  sortColumn: 'name' | 'nit' | null = null;
  sortDirection: 'asc' | 'desc' = 'asc';

  get showingStart(): number {
    return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get showingEnd(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  onSort(column: 'name' | 'nit'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.applyClientSort();
  }

  private applyClientSort(): void {
    if (!this.sortColumn || !this.empresas) return;
    const col = this.sortColumn;
    const dir = this.sortDirection === 'asc' ? 1 : -1;

    this.empresas.sort((a, b) => {
      if (col === 'name') {
        const valA = (a.name || '').toLowerCase();
        const valB = (b.name || '').toLowerCase();
        return valA.localeCompare(valB) * dir;
      } else if (col === 'nit') {
        const valA = a.nit || '';
        const valB = b.nit || '';
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
    if (!name) return 'E';
    const trimmed = name.trim();
    return trimmed.charAt(0).toUpperCase();
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.pageSize = Number(target.value) || 10;
    this.page = 1;
    this.loadEmpresas();
  }

  toggleDropdown(empresa: Empresa, event: MouseEvent): void {
    event.stopPropagation();
    if (this.activeDropdownId === empresa.id) {
      this.closeDropdown();
      return;
    }
    const buttonEl = event.currentTarget as HTMLElement;
    this.lastDropdownTrigger = buttonEl;
    this.activeEmpresa = empresa;
    this.activeDropdownId = empresa.id || null;

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
      this.activeEmpresa = null;
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
      this.activeEmpresa = null;
    }
  }

  ngOnInit(): void {
    this.loadEmpresas();
  }

  setTab(tab: 'activas' | 'inactivas'): void {
    if (this.activeTab !== tab) {
      this.activeTab = tab;
      this.page = 1;
      this.search = '';
      this.errorMessage = '';
      this.successMessage = '';
      this.activeDropdownId = null;
      this.loadEmpresas();
    }
  }

  loadEmpresas(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.activeDropdownId = null;
    this.cdr.markForCheck();
    const statusFilter = this.activeTab === 'activas' ? 'active' : 'inactive';
    this.adminService.getEmpresas(this.page, this.pageSize, this.search, statusFilter).subscribe({
      next: (result) => {
        this.empresas = result.items || [];
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
        this.errorMessage = 'No se pudo cargar la lista de empresas. Verifica la conexión con la API.';
        console.error('Error al cargar empresas:', err);
        this.cdr.markForCheck();
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadEmpresas();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadEmpresas();
    }
  }

  openCreateModal(): void {
    this.lastTriggerElement = document.activeElement as HTMLElement | null;
    this.isEditing = false;
    this.modalTitle = 'Registrar nueva empresa';
    this.currentEmpresa = this.getEmptyEmpresa();
    this.initialSnapshot = JSON.stringify(this.currentEmpresa);
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

  openEditModal(empresa: Empresa): void {
    this.lastTriggerElement = document.activeElement as HTMLElement | null;
    this.isEditing = true;
    this.modalTitle = 'Editar empresa';
    this.currentEmpresa = {
      id: empresa.id,
      name: empresa.name,
      nit: empresa.nit,
      email: empresa.email,
      phone: empresa.phone || '',
      address: empresa.address || '',
      isActive: empresa.isActive
    };
    this.initialSnapshot = JSON.stringify(this.currentEmpresa);
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
    return JSON.stringify(this.currentEmpresa) !== this.initialSnapshot;
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
    this.nitError = '';
    this.emailError = '';
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
      case 'nit': return this.nitError;
      case 'email': return this.emailError;
      case 'phone': return this.phoneError;
      case 'address': return this.addressError;
      default: return '';
    }
  }

  validateField(field: string): void {
    if (field === 'name') {
      const name = (this.currentEmpresa.name || '').trim();
      const nameRegex = /^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,\-&'’()#]+$/;
      if (!name) {
        this.nameError = 'La razón social o nombre de la empresa es obligatorio.';
      } else if (name.length < 3 || name.length > 150) {
        this.nameError = 'La razón social debe tener entre 3 y 150 caracteres.';
      } else if (!nameRegex.test(name)) {
        this.nameError = 'La razón social contiene caracteres inválidos.';
      } else {
        this.nameError = '';
      }
    } else if (field === 'nit') {
      const nit = (this.currentEmpresa.nit || '').trim();
      const nitRegex = /^([0-9]{1,3}(\.[0-9]{3}){1,4}|[0-9]{5,15})(-[0-9kK])?$/;
      if (!nit) {
        this.nitError = 'El NIT o identificación fiscal es obligatorio.';
      } else if (nit.length < 5 || nit.length > 50) {
        this.nitError = 'El NIT debe tener entre 5 y 50 caracteres.';
      } else if (!nitRegex.test(nit)) {
        this.nitError = 'El NIT debe ser numérico o tener formato fiscal válido (ej. 900123456-1).';
      } else {
        this.nitError = '';
      }
    } else if (field === 'email') {
      const email = (this.currentEmpresa.email || '').trim();
      const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      if (!email) {
        this.emailError = 'El correo electrónico de contacto es obligatorio.';
      } else if (email.length > 150) {
        this.emailError = 'El correo electrónico no puede exceder 150 caracteres.';
      } else if (!emailRegex.test(email)) {
        this.emailError = 'Ingresa un correo electrónico corporativo válido.';
      } else {
        this.emailError = '';
      }
    } else if (field === 'phone') {
      const phone = (this.currentEmpresa.phone || '').trim();
      const phoneRegex = /^[0-9+\-\s()]{7,20}$/;
      if (!phone) {
        this.phoneError = 'El teléfono de contacto es obligatorio.';
      } else if (!phoneRegex.test(phone)) {
        this.phoneError = 'Ingresa un número de teléfono válido (7 a 20 dígitos).';
      } else {
        this.phoneError = '';
      }
    } else if (field === 'address') {
      const address = (this.currentEmpresa.address || '').trim();
      if (!address) {
        this.addressError = 'La dirección principal es obligatoria.';
      } else if (address.length < 5 || address.length > 250) {
        this.addressError = 'La dirección debe tener entre 5 y 250 caracteres.';
      } else {
        this.addressError = '';
      }
    }
  }

  validateEmpresaForm(): boolean {
    this.attemptedSubmit = true;
    this.validateField('name');
    this.validateField('nit');
    this.validateField('email');
    this.validateField('phone');
    this.validateField('address');

    return !this.nameError && !this.nitError && !this.emailError && !this.phoneError && !this.addressError;
  }

  saveEmpresa(): void {
    if (!this.validateEmpresaForm()) {
      return;
    }

    this.isSaving = true;
    this.modalErrorMessage = '';
    this.errorMessage = '';
    this.cdr.markForCheck();

    if (this.isEditing) {
      if (!this.currentEmpresa.id) {
        this.isSaving = false;
        this.modalErrorMessage = 'No se encontró el ID de la empresa para actualizar.';
        this.cdr.markForCheck();
        return;
      }

      this.adminService.updateEmpresa(this.currentEmpresa.id, this.currentEmpresa).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.successMessage = 'Empresa actualizada correctamente.';
          this.loadEmpresas();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.detail || err.error?.title || 'Error al actualizar la empresa.';
          this.modalErrorMessage = msg;
          this.errorMessage = msg;
          this.cdr.markForCheck();
        }
      });
    } else {
      this.adminService.createEmpresa(this.currentEmpresa).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.successMessage = 'Empresa registrada correctamente.';
          this.loadEmpresas();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.detail || err.error?.title || 'Error al registrar la empresa.';
          this.modalErrorMessage = msg;
          this.errorMessage = msg;
          this.cdr.markForCheck();
        }
      });
    }
  }

  openConfirmModal(empresa: Empresa, action: 'suspend' | 'activate' | 'delete'): void {
    this.targetEmpresa = empresa;
    this.confirmActionType = action;
    this.activeDropdownId = null;

    if (action === 'suspend') {
      this.confirmModalTitle = 'Inactivar empresa';
      this.confirmModalMessage = `¿Confirmas que deseas marcar como inactiva la empresa «${empresa.name}»? Podrás reactivarla en cualquier momento desde la pestaña de inactivas.`;
    } else if (action === 'activate') {
      this.confirmModalTitle = 'Reactivar empresa';
      this.confirmModalMessage = `¿Deseas reactivar la empresa «${empresa.name}» para que vuelva a figurar en el directorio activo?`;
    } else {
      this.confirmModalTitle = 'Eliminar empresa';
      this.confirmModalMessage = `¿Confirmas que deseas eliminar definitivamente la empresa «${empresa.name}»? Esta acción no se puede deshacer.`;
    }

    this.showConfirmModal = true;
    this.cdr.markForCheck();
  }

  cancelConfirm(): void {
    this.showConfirmModal = false;
    this.targetEmpresa = null;
    this.cdr.markForCheck();
  }

  executeConfirmedAction(): void {
    const empresa = this.targetEmpresa;
    if (!empresa || !empresa.id) return;
    const id = empresa.id;
    const action = this.confirmActionType;
    this.showConfirmModal = false;

    if (action === 'suspend') {
      this.adminService.suspendEmpresa(id).subscribe({
        next: () => {
          this.successMessage = `Empresa «${empresa.name}» inactivada correctamente.`;
          this.loadEmpresas();
        },
        error: (err) => {
          this.errorMessage = `Error al inactivar la empresa: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    } else if (action === 'activate') {
      this.adminService.activateEmpresa(id).subscribe({
        next: () => {
          this.successMessage = `Empresa «${empresa.name}» reactivada correctamente.`;
          this.loadEmpresas();
        },
        error: (err) => {
          this.errorMessage = `Error al reactivar la empresa: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    } else if (action === 'delete') {
      this.adminService.deleteEmpresa(id).subscribe({
        next: () => {
          this.successMessage = `Empresa «${empresa.name}» eliminada correctamente.`;
          this.loadEmpresas();
        },
        error: (err) => {
          this.errorMessage = `Error al eliminar la empresa: ${err.error?.detail || 'Error en servidor'}`;
        }
      });
    }
  }

  downloadExcel(): void {
    this.adminService.exportEmpresasExcel().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Empresas_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = 'Error al descargar el archivo Excel.';
      }
    });
  }

  downloadPdf(): void {
    this.adminService.exportEmpresasPdf().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Empresas_${new Date().toISOString().slice(0, 10)}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage = 'Error al descargar el archivo PDF.';
      }
    });
  }

  private getEmptyEmpresa(): SaveEmpresa {
    return {
      name: '',
      nit: '',
      email: '',
      phone: '',
      address: '',
      isActive: true
    };
  }
}
