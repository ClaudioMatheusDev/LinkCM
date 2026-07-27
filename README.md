# LinkCM

API para encurtamento de URLs em ASP.NET Core, com SQL Server executado em Docker.

## Executar localmente

1. Copie `.env.example` para `.env` e substitua `SA_PASSWORD` por uma senha forte.
2. Execute `docker compose up --build`.
3. A API estará disponível em `http://localhost:8080`.

Na primeira inicialização, a API aplica automaticamente as migrations do Entity Framework e cria o banco `LinkCM`.

Para parar os containers, use `docker compose down`. Os dados permanecem no volume `sqlserver-data`. Para remover também os dados, use `docker compose down -v`.
