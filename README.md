# Asset Management System

Aplicação para **gerenciamento e alocação de ativos de TI** (notebooks, monitores, periféricos), desenvolvida como teste técnico para vaga de desenvolvedor.

O sistema permite:

* cadastro de ativos, usuários e tipos de ativos
* alocação e devolução de ativos com regras de negócio
* histórico de alocações
* feedback de erros para o usuário

---

## Como Executar o Projeto

### Pré-requisitos

* Docker Engine 20.10+ (inclui Docker Compose v2)

> Não é necessário instalar .NET ou PostgreSQL localmente para executar a aplicação  
> Para executar os testes é necessário .NET 8 ou superior

---

### Subindo a aplicação

O banco de dados é automaticamente versionado via EF Core Migrations. Ao iniciar a aplicação, a base é criada e populada com dados iniciais para facilitar a visualização das funcionalidades de listagem e alocação.

Na raiz do projeto, execute:

```bash
docker compose up --build
```

O Docker irá:

1. Criar o banco PostgreSQL
2. Subir a API
3. Subir o Frontend Blazor

### Para rodar os testes via terminal

```bash
dotnet test
```

---

### Evitar conflito de porta (UI)

Se a porta `8080` estiver ocupada, use a variável `WEB_PORT` ao subir o compose:

* Bash
```bash
WEB_PORT=8081 docker compose up --build
```

* PowerShell
```powershell
$env:WEB_PORT=8081
docker compose up --build
```

---

## Acessos

Após subir os containers:

* **Frontend (UI)**: padrão [http://localhost:8080](http://localhost:8080). Se usar `WEB_PORT`, exemplo [http://localhost:8081](http://localhost:8081)
* **Backend (API)**: [http://localhost:7001](http://localhost:7001)
* **Banco de dados (PostgreSQL)**: Host `localhost`, Port `5400`, Database `asset_management`, User `postgres`, Password `postgres`

---

### Documentação da API (Swagger)

[http://localhost:7001/swagger/index.html](http://localhost:7001/swagger/index.html)

O Swagger foi mantido com a documentação padrão, focada na apresentação dos endpoints, corpos de requisições e resposta esperada.

---

## Arquitetura

O projeto segue uma arquitetura em camadas, com separação de responsabilidades:

* **Backend (API)**: .NET 8, API RESTful
* **Frontend**: Blazor
* **Banco de dados**: PostgreSQL
* **Orquestração**: Docker + Docker Compose

Principais conceitos aplicados:

* Clean Architecture
* Repository + Unit of Work
* Entity Framework Core
* Validações no backend
* Exceções tipadas e retorno padronizado com `ProblemDetails`
* Middleware de tratamento de exceções com mensagens centralizadas
* Testes unitários com xUnit

---

## Tecnologias Utilizadas

### Backend

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* FluentValidation
* xUnit

### Frontend

* Blazor Server / Blazor Web App
* Bootstrap (layout básico)

### Infraestrutura

* Docker
* Docker Compose

---

## Author

Luís Eduardo Dias  
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Perfil-blue?logo=linkedin)](https://www.linkedin.com/in/luisvdias94)  
Backend / Full Stack Developer  
Java • Spring Boot • Angular • PostgreSQL