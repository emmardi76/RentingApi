# 🚗 Renting API

API RESTful para gestión de alquiler de vehículos, implementada con **.NET 8**, **PostgreSQL** y **Docker**.

---

## 🏗️ Arquitectura

El proyecto sigue los principios de **Arquitectura Hexagonal** y **Domain-Driven Design (DDD)**:

---

## 🎯 Requisitos Previos

Para ejecutar este proyecto **solo necesitas**:

### **Obligatorio:**
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows, Mac, Linux)
- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### **Opcional (para desarrollo):**
- 📝 [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- 🧪 [Postman](https://www.postman.com/downloads/) (para probar la API)

---

## 🚀 Ejecución Rápida (Docker Compose)

### **Opción 1: Ejecutar todo con Docker Compose**

```bash
git clone https://github.com/tu-usuario/renting-api.git
cd renting-api
docker-compose up --build
```

**La API estará disponible en:** http://localhost:5111

**Swagger UI:** http://localhost:5111/swagger

---

### **Opción 2: Ejecutar con InMemory Database (Sin Docker)**

```bash
git clone https://github.com/tu-usuario/renting-api.git
cd renting-api
dotnet run
```

**La API estará disponible en:** http://localhost:5111

**Swagger UI:** http://localhost:5111/swagger

---

## 🧪 Testing

### **Tests Unitarios**

```bash
cd renting-api/tests/Renting.Api.Tests
dotnet test
```

### **Tests de Integración**

```bash
cd renting-api/tests/Renting.Api.IntegrationTests
dotnet test
```

### **Ejecutar TODOS los tests**

```bash
cd renting-api
dotnet test --no-restore --verbosity normal
```

**Cobertura esperada:** 40+ tests ✅

---

## 📡 Endpoints de la API

### **Vehicles**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/vehicles` | Crear un vehículo |
| `GET` | `/api/vehicles/available` | Listar vehículos disponibles |

### **Rentals**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/rentals` | Alquilar un vehículo |
| `POST` | `/api/rentals/{id}/return` | Devolver un vehículo |

### **Health**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/health` | Health check |

---

## 📮 Uso con Postman

### **Importar la colección:**

1. Abrir Postman
2. Click en **Import**
3. Seleccionar `postman/Renting-API.postman_collection.json`
4. La colección incluye:
   - ✅ Tests automatizados
   - ✅ Variables de entorno auto-guardadas
   - ✅ Generación automática de fechas UTC

### **Ejecutar flujo completo:**

---

## 🔧 Configuración

### **appsettings.Development.json** (Desarrollo Local)

### **appsettings.Production.json** (Docker/Producción)

---

## 🛠️ Comandos de Desarrollo

### **Compilar el proyecto:**

```bash
cd renting-api
dotnet build
```

### **Ejecutar la API:**

```bash
cd renting-api
dotnet run
```

### **Limpiar build artifacts:**

### **Ver información del proyecto:**

---

## 📦 Dependencias Principales

- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL** - Provider de PostgreSQL
- **MediatR** - Patrón Mediator para CQRS
- **AutoMapper** - Mapeo de objetos
- **Swashbuckle (Swagger)** - Documentación de API
- **xUnit** - Framework de testing
- **Moq** - Mocking para tests

---

## 🐛 Troubleshooting

### **Error: "The ConnectionString property has not been initialized"**

**Solución:** Asegúrate de que PostgreSQL está corriendo:

O cambia a InMemory Database en `appsettings.Development.json`:

---

### **Error: "permission denied for schema public"**

**Solución:** Dar permisos al usuario:

---

### **Error: "Return date cannot be before start date"**

**Solución:** Asegúrate de usar fechas en formato UTC (con `Z` al final):

---

### **Error: "No se pudo copiar el archivo (bloqueado)"**

**Solución:** Detener la API antes de compilar:

taskkill /F /IM dotnet.exe
