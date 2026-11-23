using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MundoPet.Application.Common;
using MundoPet.Application.Interfaces;
using MundoPet.Application.Services;
using MundoPet.Domain.Interfaces;
using MundoPet.Infrastructure.Auth;
using MundoPet.Infrastructure.Cache;
using MundoPet.Infrastructure.Data;
using MundoPet.Infrastructure.Repositories;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------ Configurações (appsettings.json)
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwt.Key.Length < 32)
{
    throw new InvalidOperationException("Configure 'Jwt:Key' com pelo menos 32 caracteres (veja o appsettings.Development.json).");
}

var connectionString = builder.Configuration.GetConnectionString("MundoPet")
    ?? throw new InvalidOperationException("Connection string 'MundoPet' não encontrada.");

// ------------------------------------------------------------------ Injeção de dependências
// Infrastructure: banco, cache e JWT
builder.Services.AddSingleton(new DbConnectionFactory(connectionString));
builder.Services.AddSingleton(jwt);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Infrastructure: repositórios (a interface é do Domain/Application; a classe é da Infrastructure)
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IVeterinarioRepository, VeterinarioRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<ISalaRepository, SalaRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();
builder.Services.AddScoped<IPrescricaoRepository, PrescricaoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IVacinaRepository, VacinaRepository>();
builder.Services.AddScoped<IRemedioRepository, RemedioRepository>();
builder.Services.AddScoped<IHistoricoVacinacaoRepository, HistoricoVacinacaoRepository>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
builder.Services.AddScoped<IDespesaRepository, DespesaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEstatisticaRepository, EstatisticaRepository>();
builder.Services.AddScoped<IHistoricoPetRepository, HistoricoPetRepository>();

// Application: services (casos de uso)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IVeterinarioService, VeterinarioService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<ISalaService, SalaService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IPrescricaoService, PrescricaoService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IRemedioService, RemedioService>();
builder.Services.AddScoped<IHistoricoVacinacaoService, HistoricoVacinacaoService>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<IFormaPagamentoService, FormaPagamentoService>();
builder.Services.AddScoped<IDespesaService, DespesaService>();
builder.Services.AddScoped<IEstatisticaService, EstatisticaService>();

// ------------------------------------------------------------------ API
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação dos DTOs (campo obrigatório, tamanho...) também saem no formato ResultWrapper.
        options.InvalidModelStateResponseFactory = context =>
        {
            var erros = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(erro => erro.ErrorMessage))
                .ToArray();

            var resultado = Result<object?>.Fail(400, "Dados inválidos.", erros);
            return new ObjectResult(resultado) { StatusCode = 400 };
        };
    });

// ------------------------------------------------------------------ CORS (origens liberadas vêm de "Cors:Origins")
// Só é necessário quando o cliente é servido por OUTRO endereço (Live Server, nginx...). Pelo /app da própria API não precisa.
var origensCors = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(origensCors)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("X-Cache")));   // sem isso o navegador não deixa o cliente ler o header X-Cache

// ------------------------------------------------------------------ Autenticação JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // mantém os nomes curtos das claims ("sub", "name"...)

        // Em toda requisição a um endpoint [Authorize], a API valida o token com estes parâmetros.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            NameClaimType = "name"
        };

        // Token ausente, inválido ou expirado: responde 401 no formato ResultWrapper.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(Result<object?>.Fail(401, "Não autenticado.",
                    "Token JWT ausente, inválido ou expirado. Faça login novamente."));
            }
        };
    });
builder.Services.AddAuthorization();

// ------------------------------------------------------------------ Swagger (botão "Authorize" aceita o token JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MundoPet API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Faça login em POST /api/auth/login e cole aqui apenas o token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors();

// ------------------------------------------------------------------ Tratamento de erros (qualquer exceção vira ResultWrapper)
app.UseExceptionHandler(erroApp => erroApp.Run(async context =>
{
    var erro = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    var resultado = erro switch
    {
        // 1451: tentou excluir algo que outros registros usam (chave estrangeira)
        MySqlException { Number: 1451 } => Result<object?>.Fail(409, "Registro em uso.",
            "Não é possível excluir: existem registros vinculados (ex.: pets, consultas, pagamentos)."),

        // 1452: informou um ID que não existe (cliente, pet, sala, produto...)
        MySqlException { Number: 1452 } => Result<object?>.Fail(400, "Referência inválida.",
            "Um dos IDs informados não existe."),

        // 1062: valor duplicado em coluna única
        MySqlException { Number: 1062 } => Result<object?>.Fail(409, "Registro duplicado.",
            "Já existe um registro com esses dados."),

        MySqlException => Result<object?>.Fail(503, "Banco de dados indisponível.",
            "Não foi possível acessar o banco de dados."),

        _ => Result<object?>.Fail(500, "Erro interno do servidor.", "Ocorreu um erro inesperado.")
    };

    context.Response.StatusCode = resultado.StatusCode;
    await context.Response.WriteAsJsonAsync(resultado);
}));

// ------------------------------------------------------------------ Aplicativo cliente (pasta /client) servido em /app
var pastaClient = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "client"));
if (Directory.Exists(pastaClient))
{
    var arquivos = new PhysicalFileProvider(pastaClient);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = arquivos, RequestPath = "/app" });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = arquivos, RequestPath = "/app" });
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();   // 1º: lê e valida o token JWT
app.UseAuthorization();    // 2º: bloqueia os endpoints [Authorize] se não houver usuário autenticado

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/app/")).ExcludeFromDescription();

app.Run();
