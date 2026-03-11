import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LancamentoService } from '../../../core/services/lancamento.service';
import { TipoLancamento } from '../../../core/models/lancamento.model';

@Component({
  selector: 'app-lancamento-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './lancamento-form.component.html',
  styleUrl: './lancamento-form.component.scss'
})
export class LancamentoFormComponent {
  lancamentoForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private lancamentoService: LancamentoService,
    private router: Router
  ) {
    this.lancamentoForm = this.fb.group({
      descricao: ['', [Validators.required, Validators.minLength(3)]],
      valor: [null, [Validators.required, Validators.min(0.01)]],
      tipo: [TipoLancamento.Credito, Validators.required],
      dataLancamento: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.lancamentoForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const formValue = this.lancamentoForm.value;
      const dto = {
        ...formValue,
        tipo: Number(formValue.tipo)
      };

      this.lancamentoService.createLancamento(dto).subscribe({
        next: () => {
          this.successMessage = 'Lancamento criado com sucesso!';
          this.isLoading = false;
          setTimeout(() => this.router.navigate(['/lancamentos']), 1500);
        },
        error: () => {
          this.errorMessage = 'Erro ao criar lancamento. Tente novamente.';
          this.isLoading = false;
        }
      });
    }
  }
}
