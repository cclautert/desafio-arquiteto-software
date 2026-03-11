import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LancamentoService } from '../../core/services/lancamento.service';
import { ConsolidadoService } from '../../core/services/consolidado.service';
import { Lancamento, TipoLancamento } from '../../core/models/lancamento.model';
import { ConsolidadoDiario } from '../../core/models/consolidado-diario.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  lancamentos: Lancamento[] = [];
  consolidado: ConsolidadoDiario | null = null;
  totalTransactions = 0;
  totalCreditos = 0;
  totalDebitos = 0;
  saldo = 0;
  isLoading = true;

  constructor(
    private lancamentoService: LancamentoService,
    private consolidadoService: ConsolidadoService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    const today = new Date().toISOString().split('T')[0];

    this.lancamentoService.getLancamentos().subscribe({
      next: (lancamentos) => {
        this.lancamentos = lancamentos;
        this.totalTransactions = lancamentos.length;
        this.totalCreditos = lancamentos
          .filter(l => l.tipo === TipoLancamento.Credito)
          .reduce((sum, l) => sum + l.valor, 0);
        this.totalDebitos = lancamentos
          .filter(l => l.tipo === TipoLancamento.Debito)
          .reduce((sum, l) => sum + l.valor, 0);
        this.saldo = this.totalCreditos - this.totalDebitos;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });

    this.consolidadoService.getConsolidado(today).subscribe({
      next: (consolidado) => {
        this.consolidado = consolidado;
      },
      error: () => {}
    });
  }
}
