import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LancamentoService } from '../../../core/services/lancamento.service';
import { Lancamento, TipoLancamento } from '../../../core/models/lancamento.model';

@Component({
  selector: 'app-lancamentos-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './lancamentos-list.component.html',
  styleUrl: './lancamentos-list.component.scss'
})
export class LancamentosListComponent implements OnInit {
  lancamentos: Lancamento[] = [];
  filteredLancamentos: Lancamento[] = [];
  dataInicio = '';
  dataFim = '';
  isLoading = true;
  TipoLancamento = TipoLancamento;

  constructor(private lancamentoService: LancamentoService) {}

  ngOnInit(): void {
    this.loadLancamentos();
  }

  private loadLancamentos(): void {
    this.lancamentoService.getLancamentos().subscribe({
      next: (lancamentos) => {
        this.lancamentos = lancamentos;
        this.filteredLancamentos = lancamentos;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  filterByDate(): void {
    this.filteredLancamentos = this.lancamentos.filter(l => {
      const date = l.dataLancamento.split('T')[0];
      if (this.dataInicio && date < this.dataInicio) return false;
      if (this.dataFim && date > this.dataFim) return false;
      return true;
    });
  }

  clearFilter(): void {
    this.dataInicio = '';
    this.dataFim = '';
    this.filteredLancamentos = this.lancamentos;
  }
}
