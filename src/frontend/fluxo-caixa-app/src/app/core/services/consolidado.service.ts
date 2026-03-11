import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConsolidadoDiario } from '../models/consolidado-diario.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConsolidadoService {
  private apiUrl = `${environment.apiConsolidadoUrl}/api/v1/consolidado`;

  constructor(private http: HttpClient) {}

  getConsolidado(data?: string): Observable<ConsolidadoDiario> {
    if (data) {
      return this.http.get<ConsolidadoDiario>(`${this.apiUrl}/${data}`);
    }
    return this.http.get<ConsolidadoDiario>(this.apiUrl);
  }
}
