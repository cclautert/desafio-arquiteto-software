import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lancamento, CreateLancamentoDto } from '../models/lancamento.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LancamentoService {
  private apiUrl = `${environment.apiLancamentosUrl}/api/v1/lancamentos`;

  constructor(private http: HttpClient) {}

  getLancamentos(): Observable<Lancamento[]> {
    return this.http.get<Lancamento[]>(this.apiUrl);
  }

  getLancamento(id: string): Observable<Lancamento> {
    return this.http.get<Lancamento>(`${this.apiUrl}/${id}`);
  }

  createLancamento(dto: CreateLancamentoDto): Observable<Lancamento> {
    return this.http.post<Lancamento>(this.apiUrl, dto);
  }
}
