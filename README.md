# 🐾 MundoPet — API em ASP.NET Core + Aplicativo Cliente

**Integrantes da equipe:** _(preencher: nome 1, nome 2)_

Trabalho prático de Engenharia de Software: evolução do sistema da Clínica Veterinária Mundo Pet para uma
arquitetura **aplicativo cliente + API REST**.

```
Aplicativo cliente (/client)  ──HTTP + JWT──►  API ASP.NET Core (/src)  ──Dapper──►  MySQL
```

## 1. Descrição do sistema
Gestão de clínica veterinária: clientes (tutores), pets, funcionários, veterinários, salas, agenda de consultas,
prescrições, produtos, remédios, vacinas, vendas, pagamentos, despesas, estatísticas e prontuário do pet.

## 2. Tecnologias
| Parte | Tecnologia |
|---|---|
| API | C# / ASP.NET Core (.NET 10), controllers REST |
| Acesso a dados | Dapper + MySqlConnector (SQL escrito à mão) |
| Banco | MySQL 8 (script `banco.sql`) |
| Autenticação | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Senha | BCrypt (`BCrypt.Net-Next`) — só o hash é gravado |
| Cache | `IMemoryCache` |
| Documentação | Swagger |
| Cliente | HTML + JavaScript puro + Bootstrap (sem build) |

## 3. Arquitetura (Clean Architecture, 4 camadas)
```
src/
├── MundoPet.Domain          Entities/ (classes das tabelas) · Interfaces/ (contratos dos repositórios)
├── MundoPet.Application     Services/ (regras de negócio) · Dtos/ · Common/Result.cs (ResultWrapper) · Interfaces/
├── MundoPet.Infrastructure  Repositories/ (SQL com Dapper) · Auth/ (JWT) · Cache/ (memória) · Data/ (conexão)
└── MundoPet.Api             Controllers/ · Program.cs (DI, JWT, Swagger) · appsettings.json
client/                      Aplicativo que consome a API
```
Dependências: **Api → Application → Domain** e **Infrastructure → Application → Domain**. O Domain não conhece banco nem HTTP.

**Fluxo de uma requisição:** Controller (recebe HTTP) → Service (valida regras de negócio) → Repository (SQL no banco)
→ o Service devolve um `Result<T>` → o Controller devolve `StatusCode(r.StatusCode, r)`.

### Decisões de projeto (simplicidade)
- Cada entidade tem seu **próprio** Controller, Service e Repository, com os métodos escritos explicitamente
  (sem CRUD genérico, sem `IRepository<T>`).
- SQL explícito com Dapper (sem EF/Migrations). O banco é criado pelo `banco.sql`.
- Datas com `DateTime` e horas com `TimeSpan` (tipos nativos do Dapper/MySQL).
- Sem transações manuais: Cliente/Funcionário gravam o endereço e depois o registro (dois INSERTs).
- ResultWrapper **explícito**: o service cria o `Result` e o controller o devolve; não há middleware que reformate respostas.
- Injeção de dependências toda no `Program.cs`.

## 4. Como executar
**Pré-requisitos:** .NET SDK 10 (se tiver 8 ou 9, troque `TargetFramework` e `MsVersion` em `Directory.Build.props`) e MySQL.

1. **Banco:** execute o arquivo `banco.sql` no MySQL (cria o schema `mundopet` com todas as tabelas).
2. **Conexão:** ajuste usuário/senha em `src/MundoPet.Api/appsettings.json` → `ConnectionStrings:MundoPet`.
3. **API:**
   ```bash
   dotnet run --project src/MundoPet.Api
   ```
   Swagger: http://localhost:5080/swagger
4. **Aplicativo:** a própria API serve a pasta `client/`; abra http://localhost:5080/app/ e use **Entrar → Cadastre-se**
   para criar o primeiro usuário.

### Configuração sensível fora do código
A chave do JWT (`Jwt:Key`, mín. 32 caracteres) vem do `appsettings`. Em desenvolvimento ela está em
`appsettings.Development.json`; em outro ambiente defina a variável `Jwt__Key`. A API não inicia sem ela.

## 5. Autenticação JWT
1. `POST /api/auth/registrar` cria o usuário (a senha vira hash BCrypt).
2. `POST /api/auth/login` confere e-mail/senha e devolve um **token JWT** (válido por 60 min).
3. O token é assinado com HMAC-SHA256 e leva as claims `sub` (id), `email` e `name`.
4. O cliente guarda o token e envia `Authorization: Bearer <token>` em toda requisição.
5. Os controllers têm `[Authorize]`: sem token válido a API responde **401** (em ResultWrapper).
   Públicos: `POST /api/auth/login` e `POST /api/auth/registrar`.

## 6. Estratégia de cache
Cache em memória (`IMemoryCache`) na **listagem de Produtos e de Salas** (dados lidos o tempo todo e alterados raramente),
padrão *cache-aside*, TTL de 5 minutos. Código em `ProdutoService.cs` e `SalaService.cs`.

| Situação | O que acontece | Log da API | Header `X-Cache` |
|---|---|---|---|
| Lista está no cache | devolve do cache, **não vai ao banco** | `[CACHE HIT]` | `HIT` |
| Lista não está no cache | busca no **banco** e guarda no cache | `[CACHE MISS]` | `MISS` |
| POST / PUT / DELETE | remove a lista do cache | `[CACHE INVALIDATED]` | — |

No aplicativo, as telas de Produtos e Salas mostram o selo **cache HIT/MISS**.

## 7. ResultWrapper
Toda resposta da API usa `Result<T>` (`Application/Common/Result.cs`):
```json
{ "success": true,  "message": "Registro criado com sucesso.", "data": { "id": 1 }, "errors": [] }
{ "success": false, "message": "Dados inválidos.",             "data": null,       "errors": [ "The Nome field is required." ] }
```
Erros de validação, 401 e exceções também saem nesse formato (configurados no `Program.cs`).

## 8. Principais endpoints (prefixo `/api`; todos exigem token, exceto `auth/login` e `auth/registrar`)
Cada recurso tem `GET /`, `GET /{id}`, `POST /`, `PUT /{id}` e `DELETE /{id}`:
`clientes`, `funcionarios`, `veterinarios`, `pets`, `salas`*, `produtos`*, `remedios`, `vacinas`, `consultas`,
`prescricoes`, `historico-vacinacao`, `vendas`, `pagamentos`, `formas-pagamento`, `despesas` (*com cache).

Extras: `GET /pets/buscar?termo=`, `GET /pets/{id}/historico`, `GET /consultas/por-data?data=aaaa-mm-dd`,
`PATCH /consultas/{id}/status`, `GET /estatisticas`, `GET /auth/me`.

Regras de negócio: consulta não pode ser excluída (405); prescrição não pode ser excluída (405); despesa não pode ser
alterada (405); mesmo veterinário/sala no mesmo dia e horário → 409 (conflito de agenda).

## 9. Exemplos
**Login** — `POST /api/auth/login`
```json
{ "email": "ana@email.com", "senha": "123456" }
```
```json
{ "success": true, "message": "Autenticação realizada com sucesso.",
  "data": { "token": "eyJhbGciOi...", "expiraEm": "2026-10-24T15:00:00Z",
            "usuario": { "id": 1, "nome": "Ana", "email": "ana@email.com", "criadoEm": "2026-10-24T14:00:00" } },
  "errors": [] }
```
**Sem token** — `GET /api/pets` → `401`
```json
{ "success": false, "message": "Não autenticado.", "data": null,
  "errors": [ "Token JWT ausente, inválido ou expirado. Faça login novamente." ] }
```
**Criar consulta** — `POST /api/consultas` (com token)
```json
{ "petId": 1, "veterinarioId": 1, "salaId": 1, "dataConsulta": "2026-06-04", "horario": "09:00:00", "status": "Agendada" }
```
Repetir a mesma requisição → `409` `"O veterinário já possui uma consulta nesse dia e horário."`

## 10. Roteiro da apresentação
1. Subir a API e abrir o Swagger. 2. Registrar e fazer login; mostrar o token (jwt.io). 3. Chamar `/api/pets` sem token (401) e com token (200).
4. Abrir Produtos duas vezes (MISS → HIT), criar um produto e abrir de novo (INVALIDATED → MISS); mostrar os logs.
5. Mostrar respostas do ResultWrapper (sucesso, 400, 404, 409). 6. Usar o aplicativo. 7. Mostrar as 4 camadas e o fluxo Controller → Service → Repository.
