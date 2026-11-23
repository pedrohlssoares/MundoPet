# 🐾 MundoPet — API REST em ASP.NET Core + Aplicativo Cliente

## 1. Integrantes da equipe

- Luis Claudio Amaral Salvato
- Maria Fernanda Marques Barbosa da Silveira Goulart
- Pedro Henrique Lacerda Silva Soares

> Trabalho prático de Engenharia de Software: evolução do sistema da **Clínica Veterinária Mundo Pet** para uma
> arquitetura **aplicativo cliente + API REST**.

```
Aplicativo cliente (/client)  ──HTTP + JWT──►  API ASP.NET Core (/src)  ──Dapper──►  MySQL
```

## 2. Descrição do sistema

O MundoPet é um sistema de gestão de clínica veterinária. A API expõe operações de cadastro e consulta para:

- **Pessoas:** clientes (tutores), funcionários e veterinários;
- **Atendimento:** pets, salas, agenda de consultas, prescrições, vacinas, remédios e histórico de vacinação;
- **Financeiro e estoque:** produtos, vendas, formas de pagamento, pagamentos e despesas;
- **Visão geral:** estatísticas (faturamento, despesas, balanço) e prontuário completo do pet
  (tutor, consultas, vacinas, vendas e total gasto);
- **Segurança:** cadastro e login de usuários do sistema, com autenticação JWT.

O **aplicativo cliente** (`/client`) é uma aplicação web de página única (SPA) que consome a API: dashboard, agenda,
prontuário, estatísticas e telas de cadastro (CRUD) de cada recurso.

### Regras de negócio implementadas

- **Imutabilidade do prontuário:** consulta não pode ser excluída (`405`); para desistir, altera-se o status para `Cancelada`.
- **Auditoria de prescrições:** prescrição não pode ser excluída (`405`).
- **Despesas:** despesa lançada não pode ser alterada (`405`); exclui-se e lança-se de novo.
- **Exclusividade de agenda:** mesmo veterinário **ou** mesma sala no mesmo dia e horário → `409` (consultas canceladas não contam).
- **Integridade referencial:** excluir registro usado por outros → `409`; informar ID inexistente → `400`; valor duplicado → `409`.

## 3. Tecnologias utilizadas

| Parte | Tecnologia |
|---|---|
| API | C# / ASP.NET Core (.NET 10), controllers REST |
| Acesso a dados | Dapper + MySqlConnector (SQL escrito à mão, sem ORM) |
| Banco de dados | MySQL 8 (script `banco.sql`) |
| Autenticação | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Senhas | BCrypt (`BCrypt.Net-Next`) — apenas o hash é gravado |
| Cache | `IMemoryCache` (cache em memória) |
| Documentação da API | Swagger (Swashbuckle) |
| Cliente | HTML + JavaScript puro + Bootstrap 5 (sem etapa de build) |
| Implantação | Docker e Docker Compose (`Dockerfile`, `docker-compose.yml`) |
| Controle de versão | Git / GitHub |

## 4. Estrutura da arquitetura

O projeto segue **Clean Architecture** em 4 camadas:

```
MundoPet/
├── banco.sql                     Script de criação do banco (schema mundopet)
├── Dockerfile · docker-compose.yml · .env.example     Execução com Docker
├── Directory.Build.props         Versão do .NET usada por todos os projetos
├── MundoPet.sln
├── src/
│   ├── MundoPet.Domain           Entities/ (classes das tabelas) · Interfaces/ (contratos dos repositórios)
│   ├── MundoPet.Application      Services/ (regras de negócio) · Dtos/ · Common/ (ResultWrapper, helper de cache) · Interfaces/
│   ├── MundoPet.Infrastructure   Repositories/ (SQL com Dapper) · Auth/ (JWT, BCrypt) · Cache/ (memória) · Data/ (conexão)
│   └── MundoPet.Api              Controllers/ · Program.cs (DI, JWT, Swagger, erros) · appsettings.json
└── client/                       Aplicativo que consome a API (index.html, css/, js/, config.js)
```

**Dependências entre camadas:** `Api → Application → Domain` e `Infrastructure → Application → Domain`.
O `Domain` não conhece banco nem HTTP, e a `Application` não depende de nenhum pacote de infraestrutura:
detalhes técnicos como cache, token JWT e hash de senha são **interfaces** na Application (`ICacheService`,
`ITokenService`, `IPasswordHasher`) implementadas na Infrastructure.

**Fluxo de uma requisição:**
`Controller` (recebe o HTTP) → `Service` (aplica as regras de negócio) → `Repository` (SQL no MySQL) →
o `Service` devolve um `Result<T>` → o `Controller` responde com `StatusCode(r.StatusCode, r)`.

### Decisões de projeto

- Cada entidade tem seu próprio Controller, Service e Repository, com métodos explícitos (sem CRUD genérico),
  para que o fluxo de cada recurso seja fácil de ler e explicar. A lógica de cache, que seria repetida, fica
  centralizada em um helper (`CacheServiceExtensions`).
- SQL explícito e **parametrizado** com Dapper (sem Entity Framework/Migrations); o banco é criado pelo `banco.sql`.
- Cliente e Funcionário gravam o endereço e o registro dentro de **uma transação**: ou grava os dois ou nenhum.
- Injeção de dependências concentrada no `Program.cs`.
- Segredos (chave JWT, senha do banco) ficam **fora do código-fonte** (seção 8).
- **ResultWrapper:** toda resposta da API (sucesso, validação, 401, 404/405/415 genéricos e exceções) usa o mesmo formato:

```json
{ "success": true,  "message": "Registro criado com sucesso.", "data": { "id": 1 }, "errors": [] }
{ "success": false, "message": "Dados inválidos.",             "data": null,       "errors": [ "A senha deve ter entre 6 e 100 caracteres." ] }
```

## 5. Configuração do banco de dados

- **SGBD:** MySQL 8, schema `mundopet`, 17 tabelas (InnoDB): `endereco`, `cliente`, `pet`, `sala`, `funcionario`,
  `veterinario`, `consulta`, `forma_pagamento`, `produto`, `vacina`, `historico_vacinacao`, `remedio`, `prescricao`,
  `venda`, `pagamento`, `despesa` e `usuario`.
- **Criação:** o arquivo `banco.sql` cria o schema e todas as tabelas.
  - Com **Docker**, isso é automático na primeira subida (seção 6.A).
  - **Sem Docker**, execute-o uma vez no seu MySQL:

    ```bash
    mysql -u root -p < banco.sql
    ```

    (ou abra o arquivo no MySQL Workbench e execute).
- **Conexão:** a connection string **não fica no código**. Ela é lida de `ConnectionStrings:MundoPet` e informada
  por *user-secrets* (desenvolvimento) ou variável de ambiente (Docker/produção) — veja a seção 6.
- A tabela `usuario` guarda `senha_hash` (BCrypt) e tem índice único em `email`.

## 6. Instruções para execução da API

Há dois caminhos. O **A (Docker)** é o mais simples; o **B** roda direto na máquina.

### A) Com Docker (recomendado)

**Pré-requisito:** Docker com Docker Compose.

```bash
cp .env.example .env          # Windows (PowerShell): Copy-Item .env.example .env
# Não precisa editar nada: os valores de exemplo já funcionam. (Se quiser trocar: JWT_KEY precisa ter 32+ caracteres)
docker compose up --build
```

Isso sobe o MySQL (criando as tabelas com o `banco.sql`) e a API. Para parar: `Ctrl+C` e `docker compose down`
(os dados ficam guardados no volume `mundopet-dados`; use `docker compose down -v` para apagá-los).

### B) Sem Docker (.NET + MySQL instalados)

**Pré-requisitos:** [.NET SDK 10](https://dotnet.microsoft.com/download) e MySQL 8 em execução, com o `banco.sql` já executado.
Se tiver apenas o .NET 8 ou 9, troque `TargetFramework` (`net8.0`/`net9.0`) e `MsVersion` (`8.*`/`9.*`) em `Directory.Build.props`.

```bash
# 1. Segredos (guardados só na sua máquina, fora do projeto; fazer uma única vez)
dotnet user-secrets set "Jwt:Key" "uma-chave-secreta-com-no-minimo-32-caracteres" --project src/MundoPet.Api
dotnet user-secrets set "ConnectionStrings:MundoPet" "Server=localhost;Port=3306;Database=mundopet;User ID=root;Password=SUA_SENHA;" --project src/MundoPet.Api

# 2. Restaurar e executar
dotnet restore
dotnet run --project src/MundoPet.Api
```

### Endereços

- API e aplicativo: **http://localhost:5080**
- Swagger (documentação interativa): **http://localhost:5080/swagger**

A API **não inicia** sem `Jwt:Key` (mín. 32 caracteres) e sem a connection string; a mensagem de erro indica o que configurar.

## 7. Instruções para execução do aplicativo

O aplicativo cliente **é servido pela própria API** (pasta `client/` publicada em `/app`). Não há instalação nem build:

1. Suba a API (seção 6).
2. Abra **http://localhost:5080/app/** no navegador (a raiz `/` redireciona para lá).
3. Clique em **Entrar → Cadastre-se**, crie o primeiro usuário e faça login.
4. Use o menu: Dashboard, Agenda, Prontuário, Estatísticas e as telas de cadastro de cada recurso.
   As telas de **Produtos** e **Salas** exibem o selo `cache HIT/MISS`.

> O aplicativo carrega Bootstrap e fontes por CDN, então o navegador precisa de acesso à internet.
> O endereço da API usado pelo cliente está em `client/config.js` (`apiUrl: "http://localhost:5080"`).
>
> **Cliente em outro endereço** (ex.: Live Server em `http://localhost:5500`): libere a origem no CORS da API,
> que fica desligado por padrão. Em `appsettings.json` use `"Cors": { "Origins": [ "http://localhost:5500" ] }`
> ou a variável de ambiente `Cors__Origins__0=http://localhost:5500`.

## 8. Autenticação JWT

1. `POST /api/auth/registrar` cria o usuário (senha de 6 a 100 caracteres, gravada como hash BCrypt).
2. `POST /api/auth/login` confere e-mail e senha e devolve um **token JWT** (válido por **60 minutos**).
3. O token é assinado com **HMAC-SHA256** e leva as claims `sub` (id), `email` e `name`.
4. O cliente guarda o token no `localStorage` e envia `Authorization: Bearer <token>` em toda requisição.
5. Todos os controllers usam `[Authorize]`. Sem token válido (ausente, inválido ou expirado) a API responde **401** no
   formato ResultWrapper. Endpoints públicos: `POST /api/auth/login` e `POST /api/auth/registrar`.
6. A API valida assinatura, emissor (`MundoPet.Api`), audiência (`MundoPet.Client`) e expiração.
7. Login com e-mail inexistente ou senha errada devolve a **mesma mensagem** (`401`), sem revelar quais e-mails existem.

### Configuração e segredos (nada sensível no código-fonte)

| Configuração | Desenvolvimento (sem Docker) | Docker |
|---|---|---|
| Chave JWT (`Jwt:Key`, mín. 32 caracteres) | `dotnet user-secrets set "Jwt:Key" "..." --project src/MundoPet.Api` | `JWT_KEY` no arquivo `.env` |
| Connection string do MySQL | `dotnet user-secrets set "ConnectionStrings:MundoPet" "..." --project src/MundoPet.Api` | `MYSQL_ROOT_PASSWORD` no arquivo `.env` |
| Validade do token, emissor, audiência | `appsettings.json` (`Jwt:ExpiresMinutes`, `Jwt:Issuer`, `Jwt:Audience`) | variáveis `Jwt__ExpiresMinutes` etc. no `docker-compose.yml` |

- O `appsettings.json` guarda apenas configurações **não sensíveis**; as chaves ficam vazias.
- O arquivo `.env` está no `.gitignore` (não vai para o repositório); o `.env.example` mostra o formato.
- Em outros ambientes, qualquer configuração pode ser dada por variável de ambiente trocando `:` por `__`
  (ex.: `Jwt__Key`, `ConnectionStrings__MundoPet`).

Para testar no Swagger: faça login em `POST /api/auth/login`, clique em **Authorize** e cole **apenas o token**.

## 9. Estratégia de cache

Cache em memória (`IMemoryCache`) na **listagem de Produtos e de Salas**, que são lidos o tempo todo (vendas, agenda)
e alterados raramente. Padrão **cache-aside**, com **TTL de 5 minutos**. A lógica comum está em
`Application/Common/CacheServiceExtensions.cs`; `ProdutoService.cs` e `SalaService.cs` a utilizam.

| Situação | O que acontece | Log da API | Header `X-Cache` |
|---|---|---|---|
| Lista está no cache | Devolve do cache, **sem ir ao banco** | `[CACHE HIT]` | `HIT` |
| Lista não está no cache | Busca no banco e guarda no cache | `[CACHE MISS]` | `MISS` |
| `POST` / `PUT` / `DELETE` | Remove a lista do cache (invalidação) | `[CACHE INVALIDATED]` | — |

Chaves de cache: `produtos:lista` e `salas:lista`. Limitação: o cache vive na memória do processo; some quando a API
reinicia e não é compartilhado entre várias instâncias.

## 10. Principais endpoints

Prefixo `/api`. Todos exigem token, exceto `auth/login` e `auth/registrar`.

**Autenticação**

| Método | Rota | Descrição |
|---|---|---|
| POST | `/auth/registrar` | Cadastra usuário (público) |
| POST | `/auth/login` | Autentica e devolve o JWT (público) |
| GET | `/auth/me` | Dados do usuário dono do token |

**CRUD** — cada recurso tem `GET /`, `GET /{id}`, `POST /`, `PUT /{id}` e `DELETE /{id}`:
`clientes`, `funcionarios`, `veterinarios`, `pets`, `salas` (com cache), `produtos` (com cache), `remedios`, `vacinas`,
`consultas`, `prescricoes`, `historico-vacinacao`, `vendas`, `pagamentos`, `formas-pagamento`, `despesas`.

**Endpoints extras**

| Método | Rota | Descrição |
|---|---|---|
| GET | `/pets/buscar?termo=` | Busca pet por ID (se numérico) ou parte do nome |
| GET | `/pets/{id}/historico` | Prontuário: tutor, consultas, vacinas, vendas e total gasto |
| GET | `/consultas/por-data?data=aaaa-mm-dd` | Agenda de um dia |
| PATCH | `/consultas/{id}/status` | Altera o status (`Agendada`, `Realizada` ou `Cancelada`) |
| GET | `/estatisticas` | Indicadores gerais (faturamento, despesas, balanço, totais) |

**Operações bloqueadas por regra de negócio:** `DELETE /consultas/{id}` (405), `DELETE /prescricoes/{id}` (405),
`PUT /despesas/{id}` (405).

## 11. Exemplos de requisições e respostas

**Registrar usuário** — `POST /api/auth/registrar` → `201`
```json
{ "nome": "Ana", "email": "ana@email.com", "senha": "123456" }
```
```json
{ "success": true, "message": "Usuário cadastrado com sucesso.",
  "data": { "id": 1, "nome": "Ana", "email": "ana@email.com", "criadoEm": "2026-10-24T14:00:00" },
  "errors": [] }
```

**Login** — `POST /api/auth/login` → `200`
```json
{ "email": "ana@email.com", "senha": "123456" }
```
```json
{ "success": true, "message": "Autenticação realizada com sucesso.",
  "data": { "token": "eyJhbGciOi...", "expiraEm": "2026-10-24T15:00:00Z",
            "usuario": { "id": 1, "nome": "Ana", "email": "ana@email.com", "criadoEm": "2026-10-24T14:00:00" } },
  "errors": [] }
```

**Credenciais inválidas** — `POST /api/auth/login` → `401`
```json
{ "success": false, "message": "Credenciais inválidas.", "data": null, "errors": [ "E-mail ou senha incorretos." ] }
```

**Sem token** — `GET /api/pets` → `401`
```json
{ "success": false, "message": "Não autenticado.", "data": null,
  "errors": [ "Token JWT ausente, inválido ou expirado. Faça login novamente." ] }
```

**Com token** — `GET /api/produtos` (cabeçalho `Authorization: Bearer <token>`)
```bash
curl -i http://localhost:5080/api/produtos -H "Authorization: Bearer <token>"
```
Primeira chamada → `200`, header `X-Cache: MISS`. Segunda chamada (em até 5 min) → `200`, header `X-Cache: HIT`:
```json
{ "success": true, "message": "Registros listados com sucesso (cache).",
  "data": [ { "id": 1, "nome": "Ração Premium 10kg", "quantidade": 20, "descricao": "Ração para cães adultos", "preco": 189.90 } ],
  "errors": [] }
```

**Criar produto** — `POST /api/produtos` → `201` (invalida o cache; a próxima listagem será `MISS`)
```json
{ "nome": "Shampoo Neutro", "quantidade": 15, "descricao": "Shampoo para pets 500ml", "preco": 29.90 }
```

**Criar consulta** — `POST /api/consultas` → `201`
```json
{ "petId": 1, "veterinarioId": 1, "salaId": 1, "dataConsulta": "2026-06-04", "horario": "09:00:00", "status": "Agendada" }
```
Repetir a mesma requisição → `409`
```json
{ "success": false, "message": "Conflito de agenda.", "data": null,
  "errors": [ "O veterinário já possui uma consulta nesse dia e horário." ] }
```

**Alterar status** — `PATCH /api/consultas/1/status` → `200`
```json
{ "status": "Realizada" }
```

**Excluir consulta** — `DELETE /api/consultas/1` → `405`
```json
{ "success": false, "message": "Operação não permitida.", "data": null,
  "errors": [ "Não é permitida a exclusão de prontuários e consultas por motivos de auditoria clínica. Altere o status para Cancelada." ] }
```

**Registro inexistente** — `GET /api/pets/999` → `404`
```json
{ "success": false, "message": "Registro não encontrado.", "data": null, "errors": [ "Não existe registro com o id 999." ] }
```

**Rota inexistente** — `GET /api/xyz` → `404` (também no formato padrão)
```json
{ "success": false, "message": "Recurso não encontrado.", "data": null, "errors": [ "A rota ou o arquivo solicitado não existe." ] }
```

## 12. Instruções para reprodução do projeto (passo a passo)

**Com Docker**

1. Instale o Docker (com Docker Compose) e obtenha o código (`git clone` do repositório ou extraia o `.zip`).
2. Na raiz do projeto: `cp .env.example .env` (Windows: `copy .env.example .env`). Os valores de exemplo já funcionam; trocar a senha e a chave é opcional (a chave JWT precisa ter 32+ caracteres).
3. Execute `docker compose up --build`.
4. Acesse o aplicativo em `http://localhost:5080/app/` e o Swagger em `http://localhost:5080/swagger`.
5. Crie o primeiro usuário (tela *Cadastre-se* ou `POST /api/auth/registrar`) e faça login.

**Sem Docker**

1. Instale o .NET SDK 10 e o MySQL 8; obtenha o código.
2. Crie o banco: `mysql -u root -p < banco.sql`.
3. Configure os segredos com `dotnet user-secrets` (seção 6.B).
4. Execute `dotnet restore` e `dotnet run --project src/MundoPet.Api`.
5. Acesse `http://localhost:5080/app/` e `http://localhost:5080/swagger`; crie o primeiro usuário e faça login.

**Verificando o cache:** abra Produtos duas vezes (MISS → HIT), crie um produto e abra de novo (invalidação → MISS),
acompanhando os logs no terminal (`docker compose logs -f api` no caso do Docker).

**Problemas comuns**

| Sintoma | Causa provável |
|---|---|
| API não inicia pedindo `Jwt:Key` ou a connection string | Segredos não configurados (seção 6/8) |
| `docker compose` reclama de `JWT_KEY`/`MYSQL_ROOT_PASSWORD` | Arquivo `.env` não criado ou incompleto |
| Tabelas não existem no Docker depois de trocar a senha do MySQL | O banco só é criado na 1ª subida do volume: `docker compose down -v` e suba de novo |
| `503 Banco de dados indisponível` | MySQL parado ou connection string incorreta |
| Erro de versão do SDK | Ajuste `TargetFramework` e `MsVersion` em `Directory.Build.props` |
| Aplicativo diz "Não foi possível conectar à API" | API fora do ar ou `apiUrl` incorreto em `client/config.js` |
