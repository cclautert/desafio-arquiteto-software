export enum TipoLancamento {
  Credito = 1,
  Debito = 2
}

export interface Lancamento {
  id: string;
  descricao: string;
  valor: number;
  tipo: TipoLancamento;
  dataLancamento: string;
  createdAt: string;
}

export interface CreateLancamentoDto {
  descricao: string;
  valor: number;
  tipo: TipoLancamento;
  dataLancamento: string;
}
