using System.ComponentModel.DataAnnotations;
using MundoPet.Domain.Common;

namespace MundoPet.Application.Dtos;

public class ConsultaDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o pet.")]
    public int PetId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o veterinário.")]
    public int VeterinarioId { get; set; }

    /// <summary>Número da sala.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Informe a sala.")]
    public int SalaId { get; set; }

    /// <summary>Formato aaaa-mm-dd.</summary>
    [Required(ErrorMessage = "Informe a data da consulta.")]
    public DateTime? DataConsulta { get; set; }

    /// <summary>Formato hh:mm:ss.</summary>
    [Required(ErrorMessage = "Informe o horário da consulta.")]
    public TimeSpan? Horario { get; set; }

    [Required, RegularExpression(StatusConsulta.Padrao, ErrorMessage = "Status deve ser Agendada, Realizada ou Cancelada.")]
    public string Status { get; set; } = StatusConsulta.Agendada;

    /// <summary>Opcional: ao agendar ainda não há nada feito.</summary>
    [StringLength(400)]
    public string? ProcessosFeitos { get; set; }
}

public class AlterarStatusDto
{
    [Required, RegularExpression(StatusConsulta.Padrao, ErrorMessage = "Status deve ser Agendada, Realizada ou Cancelada.")]
    public string Status { get; set; } = string.Empty;
}

public class PrescricaoDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a consulta.")]
    public int ConsultaId { get; set; }

    public int? RemedioId { get; set; }

    public int? VacinaId { get; set; }

    [Required, StringLength(400)]
    public string Dosagem { get; set; } = string.Empty;
}

public class VacinaDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o produto.")]
    public int ProdutoId { get; set; }

    /// <summary>Princípio ativo.</summary>
    [Required, StringLength(45)]
    public string Ativo { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string Lote { get; set; } = string.Empty;
}

public class RemedioDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o produto.")]
    public int ProdutoId { get; set; }

    /// <summary>Princípio ativo.</summary>
    [Required, StringLength(40)]
    public string Ativo { get; set; } = string.Empty;

    [Required, StringLength(45)]
    public string Lote { get; set; } = string.Empty;
}

public class HistoricoVacinacaoDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o pet.")]
    public int PetId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a vacina.")]
    public int VacinaId { get; set; }

    /// <summary>Formato aaaa-mm-dd.</summary>
    [Required(ErrorMessage = "Informe a data de aplicação.")]
    public DateTime? DataAplicacao { get; set; }

    [Required, StringLength(45)]
    public string Dosagem { get; set; } = string.Empty;
}
