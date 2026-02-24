#!/bin/bash

echo "================================"
echo "  Newman Tests - Renting API"
echo "================================"
echo ""

# 1. Verificar que Newman está instalado
echo "🔍 Verificando Newman..."
if ! command -v newman &> /dev/null; then
    echo "❌ Newman no está instalado"
    echo "📦 Instalando Newman..."
    npm install -g newman
    npm install -g newman-reporter-htmlextra
fi

echo "✅ Newman está instalado"
echo ""

# 2. Verificar que la API está corriendo
echo "🔍 Verificando que la API está corriendo..."
if curl -f -s http://localhost:5000/health > /dev/null; then
    echo "✅ API está corriendo"
else
    echo "❌ La API no está corriendo en http://localhost:5000"
    echo "💡 Ejecuta: dotnet run --project src/Renting.Api/Renting.Api.csproj"
    exit 1
fi
echo ""

# 3. Crear directorio para reportes
mkdir -p newman-reports

# 4. Ejecutar tests de Newman
echo "🧪 Ejecutando tests de Newman..."
echo ""

newman run postman/Renting-API.postman_collection.json \
    --environment postman/Renting-API.postman_environment.json \
    --reporters cli,htmlextra \
    --reporter-htmlextra-export newman-reports/report.html \
    --color on \
    --delay-request 500

if [ $? -eq 0 ]; then
    echo ""
    echo "================================"
    echo "  ✅ TODOS LOS TESTS PASARON"
    echo "================================"
    echo ""
    echo "📊 Reporte HTML generado en: newman-reports/report.html"
else
    echo ""
    echo "================================"
    echo "  ❌ ALGUNOS TESTS FALLARON"
    echo "================================"
    exit 1
fi