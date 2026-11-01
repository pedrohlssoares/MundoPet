using System.Text.Json.Serialization;

namespace MundoPet.Application.Common;

/// <summary>
/// ResultWrapper: formato único de resposta de TODA a API.
/// <code>{ "success": true, "message": "...", "data": { ... }, "errors": [] }</code>
/// Os services devolvem um Result e o controller o devolve ao cliente: return StatusCode(r.StatusCode, r);
/// </summary>
public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>Não vai no JSON: é o código HTTP que o controller deve usar (200, 201, 400, 404...).</summary>
    [JsonIgnore]
    public int StatusCode { get; set; }

    /// <summary>Não vai no JSON: true = veio do cache, false = veio do banco, null = sem cache (vira o header X-Cache).</summary>
    [JsonIgnore]
    public bool? FromCache { get; set; }

    public static Result<T> Ok(T? data, string message = "Operação realizada com sucesso.")
        => new() { Success = true, Message = message, Data = data, StatusCode = 200 };

    public static Result<T> Created(T? data, string message = "Registro criado com sucesso.")
        => new() { Success = true, Message = message, Data = data, StatusCode = 201 };

    public static Result<T> Fail(int statusCode, string message, params string[] errors)
        => new() { Success = false, Message = message, Errors = errors.ToList(), StatusCode = statusCode };

    public static Result<T> NotFound(int id)
        => Fail(404, "Registro não encontrado.", $"Não existe registro com o id {id}.");
}
