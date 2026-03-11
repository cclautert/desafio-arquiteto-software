import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConsolidadoService } from '../../core/services/consolidado.service';
import { ConsolidadoDiario } from '../../core/models/consolidado-diario.model';

@Component({
  selector: 'app-consolidado',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './consolidado.component.html',
  styleUrl: './consolidado.component.scss'
})
export class ConsolidadoComponent {
  dataSelecionada = '';
  consolidado: ConsolidadoDiario | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(private consolidadoService: ConsolidadoService) {}

  buscar(): void {
    if (!this.dataSelecionada) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.consolidado = null;

    this.consolidadoService.getConsolidado(this.dataSelecionada).subscribe({
      next: (consolidado) => {
        this.consolidado = consolidado;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Nenhum consolidado encontrado para esta data.';
        this.isLoading = false;
      }
    });
  }
}
