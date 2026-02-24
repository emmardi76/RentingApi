# 🚗 Renting API

API RESTful para gestión de alquiler de vehículos, implementada con **.NET 8**, **PostgreSQL** y **Docker**.

---

## 📋 Tabla de Contenidos

- [Requisitos Previos](#-requisitos-previos)
- [Ejecución Rápida](#-ejecución-rápida)
- [Arquitectura](#️-arquitectura)
- [Endpoints de la API](#-endpoints-de-la-api)
- [Testing](#-testing)
- [Docker](#-docker)
- [Base de Datos](#️-base-de-datos)
- [Postman](#-postman)
- [Troubleshooting](#-troubleshooting)
- [Reglas de Negocio](#-reglas-de-negocio)
- [Contribuir](#-contribuir)

---

## 🎯 Requisitos Previos

Para ejecutar este proyecto **solo necesitas**:

### **Obligatorio:**
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop/) 20.x o superior
- ✅ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### **Opcional:**
- 📝 [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- 🧪 [Postman](https://www.postman.com/downloads/) para probar la API

### **Verificar instalación:**
```bash
docker --version
dotnet --version
```

---

## 🚀 Ejecución Rápida (Docker Compose)

### **Opción 1: Ejecutar todo con Docker Compose**
# 1. Clonar y navegar al proyecto
git clone https://github.com/tu-usuario/RentingApi.git
cd RentingApi

# 2. Iniciar PostgreSQL
docker-compose up -d postgres

# 3. Verificar que está corriendo
docker ps | grep renting-postgres

# 4. Aplicar migraciones
dotnet ef database update \
  --project src/Renting.Infrastructure/Renting.Infrastructure.csproj \
  --startup-project src/Renting.Api/Renting.Api.csproj

# 5. Ejecutar la API
dotnet run --project src/Renting.Api/Renting.Api.csproj

**✅ La API estará disponible en:**

- 🌐 **API Base:** http://localhost:5111
- 📚 **Swagger UI:** http://localhost:5111/swagger
- ❤️ **Health Check:** http://localhost:5111/health

---

# 6. En otra terminal, probar la API
curl -X POST http://localhost:5111/api/vehicles \
  -H "Content-Type: application/json" \
  -d '{"make":"Toyota","model":"Corolla","year":2023}'

# 7. Ver vehículos disponibles
curl http://localhost:5111/api/vehicles/available

# 8. Ejecutar tests
dotnet test


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

## 📮 Postman

### **Importar la colección:**

1. Abrir **Postman**
2. Click en **Import**
3. Seleccionar `postman/Renting-API.postman_collection.json`
---
### **Flujo de trabajo:**
1️. Create Vehicle ↓ Guarda vehicleId automáticamente
2.Create Rental ↓ Usa vehicleId, guarda rentalId
3. Return Vehicle ↓ Usa rentalId
4️. Get Available Vehicles ✅ Verifica disponibilidad
---

### **Características:**

- ✅ **Tests automatizados** - Valida status codes
- ✅ **Variables auto-guardadas** - vehicleId, rentalId
- ✅ **Fechas UTC automáticas** - Genera startDate correcto
- ✅ **Scripts pre/post-request** - Automatización completa

---

## 🐛 Troubleshooting

### **❌ "The ConnectionString property has not been initialized"**

**Solución:**
