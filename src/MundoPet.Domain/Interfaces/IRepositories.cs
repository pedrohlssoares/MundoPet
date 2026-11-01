using MundoPet.Domain.Entities;

namespace MundoPet.Domain.Interfaces;

// Contratos de acesso a dados. A implementação (SQL com Dapper) fica na Infrastructure.
// Cada repositório declara explicitamente os seus métodos.

public interface IClienteRepository
{
    Task<IEnumerable<Cliente>> ObterTodosAsync();
    Task<Cliente?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Cliente item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Cliente item);
    Task<bool> RemoverAsync(int id);
}

public interface IFuncionarioRepository
{
    Task<IEnumerable<Funcionario>> ObterTodosAsync();
    Task<Funcionario?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Funcionario item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Funcionario item);
    Task<bool> RemoverAsync(int id);
}

public interface IVeterinarioRepository
{
    Task<IEnumerable<Veterinario>> ObterTodosAsync();
    Task<Veterinario?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Veterinario item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Veterinario item);
    Task<bool> RemoverAsync(int id);
}

public interface ISalaRepository
{
    Task<IEnumerable<Sala>> ObterTodosAsync();
    Task<Sala?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Sala item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Sala item);
    Task<bool> RemoverAsync(int id);
}

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> ObterTodosAsync();
    Task<Produto?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Produto item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Produto item);
    Task<bool> RemoverAsync(int id);
}

public interface IRemedioRepository
{
    Task<IEnumerable<Remedio>> ObterTodosAsync();
    Task<Remedio?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Remedio item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Remedio item);
    Task<bool> RemoverAsync(int id);
}

public interface IVacinaRepository
{
    Task<IEnumerable<Vacina>> ObterTodosAsync();
    Task<Vacina?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Vacina item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Vacina item);
    Task<bool> RemoverAsync(int id);
}

public interface IHistoricoVacinacaoRepository
{
    Task<IEnumerable<HistoricoVacinacao>> ObterTodosAsync();
    Task<HistoricoVacinacao?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(HistoricoVacinacao item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(HistoricoVacinacao item);
    Task<bool> RemoverAsync(int id);
}

public interface IPrescricaoRepository
{
    Task<IEnumerable<Prescricao>> ObterTodosAsync();
    Task<Prescricao?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Prescricao item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Prescricao item);
    Task<bool> RemoverAsync(int id);
}

public interface IVendaRepository
{
    Task<IEnumerable<Venda>> ObterTodosAsync();
    Task<Venda?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Venda item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Venda item);
    Task<bool> RemoverAsync(int id);
}

public interface IPagamentoRepository
{
    Task<IEnumerable<Pagamento>> ObterTodosAsync();
    Task<Pagamento?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Pagamento item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Pagamento item);
    Task<bool> RemoverAsync(int id);
}

public interface IDespesaRepository
{
    Task<IEnumerable<Despesa>> ObterTodosAsync();
    Task<Despesa?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Despesa item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Despesa item);
    Task<bool> RemoverAsync(int id);
}

public interface IFormaPagamentoRepository
{
    Task<IEnumerable<FormaPagamento>> ObterTodosAsync();
    Task<FormaPagamento?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(FormaPagamento item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(FormaPagamento item);
    Task<bool> RemoverAsync(int id);
}

public interface IPetRepository
{
    Task<IEnumerable<Pet>> ObterTodosAsync();
    Task<Pet?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Pet item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Pet item);
    Task<bool> RemoverAsync(int id);
    /// <summary>Número = busca por ID; texto = busca por parte do nome.</summary>
    Task<IEnumerable<Pet>> BuscarAsync(string termo);
}

public interface IConsultaRepository
{
    Task<IEnumerable<Consulta>> ObterTodosAsync();
    Task<Consulta?> ObterPorIdAsync(int id);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Consulta item);
    /// <summary>Atualiza o registro. Devolve false se ele não existe.</summary>
    Task<bool> AtualizarAsync(Consulta item);
    Task<bool> RemoverAsync(int id);
    Task<IEnumerable<Consulta>> ObterPorDataAsync(DateTime data);
    Task<bool> VeterinarioOcupadoAsync(int veterinarioId, DateTime data, TimeSpan horario, int ignorarConsultaId);
    Task<bool> SalaOcupadaAsync(int salaId, DateTime data, TimeSpan horario, int ignorarConsultaId);
}

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    /// <summary>Insere e devolve o ID gerado.</summary>
    Task<int> AdicionarAsync(Usuario item);
}
