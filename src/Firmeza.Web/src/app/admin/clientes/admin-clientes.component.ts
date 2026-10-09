import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
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

  // Modal de Crear / Editar
  showModal = false;
  modalTitle = 'Nuevo Cliente';
  modalErrorMessage = '';
  isEditing = false;
  currentCliente: SaveCliente = this.getEmptyCliente();

  // Validaciones sencillas de formulario
  nameError = '';
  documentError = '';
  emailError = '';
  ageError = '';
  phoneError = '';
  addressError = '';

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
      this.loadClientes();
    }
  }

  loadClientes(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();
    const statusFilter = this.activeTab === 'activos' ? 'active' : 'inactive';
    this.adminService.getClientes(this.page, this.pageSize, this.search, this.role, statusFilter).subscribe({
      next: (result) => {
        this.clientes = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 1;
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
    this.isEditing = false;
    this.modalTitle = 'Registrar Nuevo Cliente';
    this.currentCliente = this.getEmptyCliente();
    this.clearErrors();
    this.modalErrorMessage = '';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  openEditModal(cliente: Cliente): void {
    this.isEditing = true;
    this.modalTitle = 'Editar Cliente';
    this.currentCliente = {
      id: cliente.id,
      name: cliente.name,
      documentNumber: cliente.documentNumber,
      email: cliente.email,
      phone: cliente.phone || '',
      age: cliente.age || 18,
      address: cliente.address || '',
      isActive: cliente.isActive
    };
    this.clearErrors();
    this.modalErrorMessage = '';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  closeModal(): void {
    this.showModal = false;
    this.clearErrors();
    this.modalErrorMessage = '';
    this.cdr.markForCheck();
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

  validateClienteForm(): boolean {
    let isValid = true;
    this.clearErrors();

    // Nombre
    const name = (this.currentCliente.name || '').trim();
    const nameRegex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.,\-']+$/;
    if (!name) {
      this.nameError = 'El nombre completo es obligatorio.';
      isValid = false;
    } else if (name.length < 3 || name.length > 150) {
      this.nameError = 'El nombre debe tener entre 3 y 150 caracteres.';
      isValid = false;
    } else if (!nameRegex.test(name)) {
      this.nameError = 'El nombre solo debe contener letras y espacios.';
      isValid = false;
    }

    // Documento
    const doc = (this.currentCliente.documentNumber || '').trim();
    const docRegex = /^[0-9]{6,12}$/;
    if (!doc) {
      this.documentError = 'El número de documento es obligatorio.';
      isValid = false;
    } else if (doc.length < 6 || doc.length > 12) {
      this.documentError = 'El documento debe tener entre 6 y 12 dígitos.';
      isValid = false;
    } else if (!docRegex.test(doc)) {
      this.documentError = 'El documento de identidad debe contener únicamente dígitos numéricos.';
      isValid = false;
    }

    // Correo
    const email = (this.currentCliente.email || '').trim();
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!email) {
      this.emailError = 'El correo electrónico es obligatorio.';
      isValid = false;
    } else if (email.length > 150) {
      this.emailError = 'El correo no puede exceder 150 caracteres.';
      isValid = false;
    } else if (!emailRegex.test(email)) {
      this.emailError = 'Ingresa un correo electrónico válido (ej: nombre@dominio.com).';
      isValid = false;
    }

    // Edad
    const age = Number(this.currentCliente.age);
    if (isNaN(age) || age < 18) {
      this.ageError = 'El cliente debe ser mayor de edad (18 años o más).';
      isValid = false;
    } else if (age > 120) {
      this.ageError = 'Por favor ingresa una edad válida (máximo 120 años).';
      isValid = false;
    }

    // Teléfono
    const phone = (this.currentCliente.phone || '').trim();
    const phoneRegex = /^[0-9+\-\s()]{7,20}$/;
    if (!phone) {
      this.phoneError = 'El teléfono de contacto es obligatorio.';
      isValid = false;
    } else if (!phoneRegex.test(phone)) {
      this.phoneError = 'Ingresa un número de teléfono válido (entre 7 y 20 dígitos y símbolos permitidos +, -, (, )).';
      isValid = false;
    }

    // Dirección
    const address = (this.currentCliente.address || '').trim();
    if (!address) {
      this.addressError = 'La dirección de domicilio es obligatoria.';
      isValid = false;
    } else if (address.length < 5 || address.length > 250) {
      this.addressError = 'La dirección debe tener entre 5 y 250 caracteres.';
      isValid = false;
    }

    return isValid;
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
          const msg = err.error?.detail || err.error?.title || 'Error al crear el cliente.';
          this.modalErrorMessage = msg;
          this.errorMessage = msg;
          this.cdr.markForCheck();
        }
      });
    }
  }

  deleteCliente(cliente: Cliente): void {
    if (!cliente.id) return;

    if (!confirm(`¿Estás seguro de que deseas suspender al cliente "${cliente.name}"? Pasará al apartado de clientes suspendidos.`)) {
      return;
    }

    this.adminService.deleteCliente(cliente.id).subscribe({
      next: () => {
        this.successMessage = `Cliente "${cliente.name}" suspendido correctamente.`;
        this.loadClientes();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = `Error al suspender el cliente: ${err.error?.detail || err.error?.title || 'Error en servidor'}`;
        this.cdr.markForCheck();
      }
    });
  }

  activateCliente(cliente: Cliente): void {
    if (!cliente.id) return;

    this.adminService.activateCliente(cliente.id).subscribe({
      next: () => {
        this.successMessage = `Cliente "${cliente.name}" reactivado exitosamente. Ya tiene acceso habilitado.`;
        this.loadClientes();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = `Error al reactivar el cliente: ${err.error?.detail || err.error?.title || 'Error en servidor'}`;
        this.cdr.markForCheck();
      }
    });
  }

  hardDeleteCliente(cliente: Cliente): void {
    if (!cliente.id) return;

    if (!confirm(`¿Estás completamente seguro de que deseas eliminar DEFINITIVAMENTE de la base de datos al cliente "${cliente.name}"? Esta acción borrará todo su historial y no se puede deshacer.`)) {
      return;
    }

    this.adminService.deletePermanentCliente(cliente.id).subscribe({
      next: () => {
        this.successMessage = `Cliente "${cliente.name}" eliminado definitivamente de la base de datos.`;
        this.loadClientes();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = `Error al eliminar el cliente: ${err.error?.detail || err.error?.title || 'Error en servidor'}`;
        this.cdr.markForCheck();
      }
    });
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
      age: 18,
      address: '',
      isActive: true
    };
  }
}
