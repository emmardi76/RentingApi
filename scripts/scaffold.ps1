# Ejecutar desde la raíz del repositorio (PowerShell)
# Requisitos: dotnet 8 SDK instalado

$solutionName = "Renting"
$src = "src"
$tests = "tests"
$postman = "postman"

# Crear carpetas
mkdir $src -ErrorAction SilentlyContinue
mkdir $tests -ErrorAction SilentlyContinue
mkdir $postman -ErrorAction SilentlyContinue
mkdir scripts -ErrorAction SilentlyContinue
mkdir docker -ErrorAction SilentlyContinue
mkdir docs -ErrorAction SilentlyContinue

# Crear solución
dotnet new sln -n $solutionName

# Crear proyectos
dotnet new webapi -n Renting.Api -o "$src/Renting.Api" --no-https
dotnet new classlib -n Renting.Application -o "$src/Renting.Application"
dotnet new classlib -n Renting.Domain -o "$src/Renting.Domain"
dotnet new classlib -n Renting.Infrastructure -o "$src/Renting.Infrastructure"

# Proyectos de tests (xUnit)
dotnet new xunit -n Renting.Tests.Unit -o "$tests/Renting.Tests.Unit"
dotnet new xunit -n Renting.Tests.Functional -o "$tests/Renting.Tests.Functional"
dotnet new xunit -n Renting.Tests.Host -o "$tests/Renting.Tests.Host"

# Añadir proyectos a la solución
dotnet sln add "$src/Renting.Api/Renting.Api.csproj"
dotnet sln add "$src/Renting.Application/Renting.Application.csproj"
dotnet sln add "$src/Renting.Domain/Renting.Domain.csproj"
dotnet sln add "$src/Renting.Infrastructure/Renting.Infrastructure.csproj"
dotnet sln add "$tests/Renting.Tests.Unit/Renting.Tests.Unit.csproj"
dotnet sln add "$tests/Renting.Tests.Functional/Renting.Tests.Functional.csproj"
dotnet sln add "$tests/Renting.Tests.Host/Renting.Tests.Host.csproj"

# Añadir referencias entre proyectos
dotnet add "$src/Renting.Api/Renting.Api.csproj" reference "$src/Renting.Application/Renting.Application.csproj"
dotnet add "$src/Renting.Api/Renting.Api.csproj" reference "$src/Renting.Infrastructure/Renting.Infrastructure.csproj"
dotnet add "$src/Renting.Application/Renting.Application.csproj" reference "$src/Renting.Domain/Renting.Domain.csproj"
dotnet add "$src/Renting.Infrastructure/Renting.Infrastructure.csproj" reference "$src/Renting.Application/Renting.Application.csproj"
dotnet add "$src/Renting.Infrastructure/Renting.Infrastructure.csproj" reference "$src/Renting.Domain/Renting.Domain.csproj"

# Tests referencias
dotnet add "$tests/Renting.Tests.Unit/Renting.Tests.Unit.csproj" reference "$src/Renting.Application/Renting.Application.csproj"
dotnet add "$tests/Renting.Tests.Unit/Renting.Tests.Unit.csproj" reference "$src/Renting.Domain/Renting.Domain.csproj"
dotnet add "$tests/Renting.Tests.Functional/Renting.Tests.Functional.csproj" reference "$src/Renting.Application/Renting.Application.csproj"
dotnet add "$tests/Renting.Tests.Functional/Renting.Tests.Functional.csproj" reference "$src/Renting.Infrastructure/Renting.Infrastructure.csproj"
dotnet add "$tests/Renting.Tests.Host/Renting.Tests.Host.csproj" reference "$src/Renting.Api/Renting.Api.csproj"

# Paquetes útiles (puedes modificarlos luego)
dotnet add "$tests/Renting.Tests.Host/Renting.Tests.Host.csproj" package Microsoft.AspNetCore.Mvc.Testing --version 8.0.0
dotnet add "$src/Renting.Infrastructure/Renting.Infrastructure.csproj" package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add "$src/Renting.Infrastructure/Renting.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design --version 8.0.0

# Escribir .editorconfig
$editorconfig = @"
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{md,ps1,yml,yaml,json}]
indent_style = space
indent_size = 2

[*.cs]
indent_style = space
indent_size = 4
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = true
csharp_new_line_before_open_brace = all
"@
Set-Content -Path ".editorconfig" -Value $editorconfig -Encoding utf8

# Escribir CONTRIBUTING.md
$contrib = @"
# Contribuir

## Formato de código
- Se usa .editorconfig para formateo automático.
- Indentación: 4 espacios en C#.

## Pruebas
- Aplicar TDD: los cambios deben acompañarse de pruebas unitarias cuando afecten a lógica de negocio.
- Tests de integración y host deben existir para flujos críticos.

## Workflow
- Trabajar en ramas feature/issue-xxx.
- Pull requests revisados y aprobados antes de merge.

## Estándares de arquitectura
- Hexagonal + DDD: separar Domain, Application, Infrastructure, Api.
- Repositorios: interfaces en `Application`, implementaciones en `Infrastructure`.
"@
Set-Content -Path "CONTRIBUTING.md" -Value $contrib -Encoding utf8

# Docker compose y Dockerfile básicos para desarrollo local (Postgres)
$compose = @"
version: '3.8'
services:
  api:
    build:
      context: .
      dockerfile: docker/Dockerfile.api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=renting;Username=postgres;Password=postgres
    depends_on:
      - postgres
  postgres:
    image: postgres:15
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: renting
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
volumes:
  pgdata:
"@
Set-Content -Path "docker/docker-compose.yml" -Value $compose -Encoding utf8

$dockerfile = @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore \"src/Renting.Api/Renting.Api.csproj\"
RUN dotnet publish \"src/Renting.Api/Renting.Api.csproj\" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [\"dotnet\", \"Renting.Api.dll\"]
"@
Set-Content -Path "docker/Dockerfile.api" -Value $dockerfile -Encoding utf8

Write-Host "Scaffold completado. Abre la solución $solutionName.sln"