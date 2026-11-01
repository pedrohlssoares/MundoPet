namespace MundoPet.Application.Dtos;

/// <summary>Painel do pet: dados do tutor, consultas, vacinas, vendas e total gasto.</summary>
public class HistoricoPetDto
{
    public PetTutorDto Pet { get; set; } = new();
    public IEnumerable<ConsultaHistoricoDto> Consultas { get; set; } = [];
    public IEnumerable<VacinaHistoricoDto> Vacinas { get; set; } = [];
    public IEnumerable<VendaHistoricoDto> Vendas { get; set; } = [];
    public decimal TotalGasto { get; set; }
}

public class PetTutorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public string Idade { get; set; } = string.Empty;
    public string TutorNome { get; set; } = string.Empty;
    public string TutorCpf { get; set; } = string.Empty;
    public string TutorCelular { get; set; } = string.Empty;
}

public class ConsultaHistoricoDto
{
    public int Id { get; set; }
    public DateTime DataConsulta { get; set; }
    public TimeSpan Horario { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProcessosFeitos { get; set; } = string.Empty;
    public int? SalaNumero { get; set; }
    public string? SalaTipo { get; set; }
}

public class VacinaHistoricoDto
{
    public int Id { get; set; }
    public DateTime DataAplicacao { get; set; }
    public string Dosagem { get; set; } = string.Empty;
    public string Ativo { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
}

public class VendaHistoricoDto
{
    public int VendaId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public DateTime DataPagamento { get; set; }
    public decimal Subtotal => Quantidade * ValorUnitario;
}
