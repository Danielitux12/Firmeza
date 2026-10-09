import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cliente, SaveCliente } from '../models/cliente.model';
import { Empresa, SaveEmpresa } from '../models/empresa.model';
import { PagedResult, RecordStatusFilter } from '../models/paged-result.model';
import { DashboardMetrics } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5246/api';

  // --- Dashboard ---
  getMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(`${this.baseUrl}/dashboard`);
  }

  // --- Clientes ---
  getClientes(page = 1, pageSize = 10, search = '', role = 'all', status: RecordStatusFilter = 'active'): Observable<PagedResult<Cliente>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('status', status);

    if (role && role !== 'all') {
      params = params.set('role', role);
    }

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResult<Cliente>>(`${this.baseUrl}/clientes`, { params });
  }

  getCliente(id: string): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.baseUrl}/clientes/${id}`);
  }

  createCliente(cliente: SaveCliente): Observable<Cliente> {
    return this.http.post<Cliente>(`${this.baseUrl}/clientes`, cliente);
  }

  updateCliente(id: string, cliente: SaveCliente): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.baseUrl}/clientes/${id}`, cliente);
  }

  suspendCliente(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/clientes/${id}/suspend`, {});
  }

  activateCliente(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/clientes/${id}/activate`, {});
  }

  deleteCliente(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/clientes/${id}`);
  }

  deletePermanentCliente(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/clientes/${id}/permanent`);
  }

  exportClientesExcel(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/clientes/export-excel`, { responseType: 'blob' });
  }

  exportClientesPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/clientes/export-pdf`, { responseType: 'blob' });
  }

  // --- Empresas ---
  getEmpresas(page = 1, pageSize = 10, search = ''): Observable<PagedResult<Empresa>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResult<Empresa>>(`${this.baseUrl}/empresas`, { params });
  }

  getEmpresa(id: string): Observable<Empresa> {
    return this.http.get<Empresa>(`${this.baseUrl}/empresas/${id}`);
  }

  createEmpresa(empresa: SaveEmpresa): Observable<Empresa> {
    return this.http.post<Empresa>(`${this.baseUrl}/empresas`, empresa);
  }

  updateEmpresa(id: string, empresa: SaveEmpresa): Observable<Empresa> {
    return this.http.put<Empresa>(`${this.baseUrl}/empresas/${id}`, empresa);
  }

  suspendEmpresa(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/empresas/${id}/suspend`, {});
  }

  activateEmpresa(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/empresas/${id}/activate`, {});
  }

  deleteEmpresa(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/empresas/${id}`);
  }

  exportEmpresasExcel(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/empresas/export-excel`, { responseType: 'blob' });
  }

  exportEmpresasPdf(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/empresas/export-pdf`, { responseType: 'blob' });
  }
}
