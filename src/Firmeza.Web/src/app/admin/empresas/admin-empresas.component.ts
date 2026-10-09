import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
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

  // Modal Crear / Editar
  showModal = false;
  modalTitle = 'Nueva Empresa';
  modalErrorMessage = '';
  isEditing = false;
  currentEmpresa: SaveEmpresa = this.getEmptyEmpresa();

  // Validaciones de formulario
  nameError = '';
  nitError = '';
  emailError = '';
  phoneError = '';
  addressError = '';

  ngOnInit(): void {
    this.loadEmpresas();
  }

  loadEmpresas(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();
    this.adminService.getEmpresas(this.page, this.pageSize, this.search).subscribe({
      next: (result) => {
        this.empresas = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 1;
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
    this.isEditing = false;
    this.modalTitle = 'Registrar Nueva Empresa';
    this.currentEmpresa = this.getEmptyEmpresa();
    this.clearErrors();
    this.modalErrorMessage = '';
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.markForCheck();
  }

  openEditModal(empresa: Empresa): void {
    this.isEditing = true;
    this.modalTitle = 'Editar Empresa';
    this.currentEmpresa = {
      id: empresa.id,
      name: empresa.name,
      nit: empresa.nit,
      email: empresa.email,
      phone: empresa.phone || '',
      address: empresa.address || '',
      isActive: empresa.isActive
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
    this.nitError = '';
    this.emailError = '';
    this.phoneError = '';
    this.addressError = '';
    this.modalErrorMessage = '';
    this.cdr.markForCheck();
  }

  validateEmpresaForm(): boolean {
    let isValid = true;
    this.clearErrors();

    // Razón Social
    const name = (this.currentEmpresa.name || '').trim();
    const nameRegex = /^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,\-&'’()#]+$/;
    if (!name) {
      this.nameError = 'La razón social o nombre de la empresa es obligatorio.';
      isValid = false;
    } else if (name.length < 3 || name.length > 150) {
      this.nameError = 'La razón social debe tener entre 3 y 150 caracteres.';
      isValid = false;
    } else if (!nameRegex.test(name)) {
      this.nameError = 'La razón social contiene caracteres inválidos.';
      isValid = false;
    }

    // NIT / Identificación Fiscal
    const nit = (this.currentEmpresa.nit || '').trim();
    const nitRegex = /^([0-9]{1,3}(\.[0-9]{3}){1,4}|[0-9]{5,15})(-[0-9kK])?$/;
    if (!nit) {
      this.nitError = 'El NIT o identificación fiscal es obligatorio.';
      isValid = false;
    } else if (nit.length < 5 || nit.length > 50) {
      this.nitError = 'El NIT debe tener entre 5 y 50 caracteres.';
      isValid = false;
    } else if (!nitRegex.test(nit)) {
      this.nitError = 'El NIT debe ser numérico o tener formato fiscal válido (ej. 900123456-1 o 900.123.456-1).';
      isValid = false;
    }

    // Correo Electrónico
    const email = (this.currentEmpresa.email || '').trim();
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!email) {
      this.emailError = 'El correo electrónico de contacto es obligatorio.';
      isValid = false;
    } else if (email.length > 150) {
      this.emailError = 'El correo electrónico no puede exceder 150 caracteres.';
      isValid = false;
    } else if (!emailRegex.test(email)) {
      this.emailError = 'Ingresa un correo electrónico corporativo válido (ej. contacto@empresa.com).';
      isValid = false;
    }

    // Teléfono
    const phone = (this.currentEmpresa.phone || '').trim();
    const phoneRegex = /^[0-9+\-\s()]{7,20}$/;
    if (!phone) {
      this.phoneError = 'El teléfono de contacto es obligatorio.';
      isValid = false;
    } else if (!phoneRegex.test(phone)) {
      this.phoneError = 'Ingresa un número de teléfono válido (entre 7 y 20 dígitos y símbolos permitidos +, -, (, )).';
      isValid = false;
    }

    // Dirección
    const address = (this.currentEmpresa.address || '').trim();
    if (!address) {
      this.addressError = 'La dirección principal es obligatoria.';
      isValid = false;
    } else if (address.length < 5 || address.length > 250) {
      this.addressError = 'La dirección debe tener entre 5 y 250 caracteres.';
      isValid = false;
    }

    return isValid;
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

  deleteEmpresa(empresa: Empresa): void {
    if (!empresa.id) return;

    if (!confirm(`¿Estás seguro de que deseas eliminar permanentemente la empresa "${empresa.name}"? Esta acción no se puede deshacer.`)) {
      return;
    }

    this.adminService.deleteEmpresa(empresa.id).subscribe({
      next: () => {
        this.successMessage = `Empresa "${empresa.name}" eliminada correctamente.`;
        this.loadEmpresas();
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.errorMessage = `Error al eliminar la empresa: ${err.error?.detail || err.error?.title || 'Error en servidor'}`;
        this.cdr.markForCheck();
      }
    });
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
