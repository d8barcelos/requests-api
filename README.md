# Requests API

Uma API REST construída com .NET 8 para gerenciamento de requisições, utilizando MongoDB como banco de dados principal e Redis para cache.

## 🚀 Tecnologias

- .NET 8
- MongoDB
- Redis
- JWT para autenticação

## 📋 Pré-requisitos

- .NET 8 SDK
- MongoDB
- Redis
- Uma IDE compatível com C# (Visual Studio, VS Code, Rider)

## ⚙️ Configuração

1. Clone o repositório
2. Configure as strings de conexão no `appsettings.json`:

```json
{
  "MongoDB": {
    "ConnectionString": "sua_string_conexao_mongodb",
    "DatabaseName": "seu_banco_de_dados"
  },
  "Redis": {
    "ConnectionString": "sua_string_conexao_redis"
  },
  "Jwt": {
    "Key": "sua_chave_secreta_jwt"
  }
}
```

## 🛠️ Endpoints

### Autenticação

```
POST /api/auth/register
POST /api/auth/login
```

### Requisições (Requer Autenticação)

```
GET    /api/requests        - Lista todas as requisições
GET    /api/requests/{id}   - Obtém uma requisição específica
POST   /api/requests        - Cria uma nova requisição
PUT    /api/requests/{id}   - Atualiza uma requisição
DELETE /api/requests/{id}   - Remove uma requisição
PATCH  /api/requests/{id}/update-status - Atualiza o status
```

## 🔒 Autenticação

A API utiliza autenticação JWT. Para acessar endpoints protegidos:

1. Faça login para obter o token
2. Inclua o token no header das requisições:
```
Authorization: Bearer seu_token_jwt
```

## 📦 Cache

- Redis é utilizado para cache de requisições
- Cache é invalidado automaticamente em operações de escrita
- Tempo de expiração configurado para 30 minutos

## 🏃‍♂️ Executando o Projeto

1. Restaure os pacotes:
```bash
dotnet restore
```

2. Execute o projeto:
```bash
dotnet run
```

A API estará disponível em `https://localhost:5001` ou `http://localhost:5000`

## 📝 Licença

Este projeto está sob a licença MIT.
