# Postman Collection - Renting API

Esta colección contiene todos los endpoints de la API de Renting con tests automatizados.

## 📋 Contenido

- **Vehicles**: Endpoints para gestión de vehículos
  - Create Vehicle
  - Get Available Vehicles
  - Validaciones

- **Rentals**: Endpoints para gestión de alquileres
  - Create Rental (alquilar vehículo)
  - Return Vehicle (devolver vehículo)
  - Validaciones y casos de error

- **Health**: Health check endpoint

## 🚀 Uso con Postman UI

### 1. Importar la colección

1. Abre Postman
2. Click en "Import"
3. Selecciona `Renting-API.postman_collection.json`
4. Selecciona `Renting-API.postman_environment.json`

### 2. Configurar el environment

1. Selecciona "Renting API - Local" en el selector de environments
2. Verifica que `baseUrl` apunta a `http://localhost:5000`

### 3. Ejecutar la colección

1. Asegúrate de que la API está corriendo: