# Script para ejecutar tests de Postman con Newman

Write-Host "================================" -ForegroundColor Cyan
Write-Host "  Newman Tests - Renting API" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar que Newman está instalado
Write-Host "🔍 Verificando Newman..." -ForegroundColor Yellow
if (!(Get-Command newman -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Newman no está instalado" -ForegroundColor Red
    Write-Host "📦 Instalando Newman..." -ForegroundColor Yellow
    npm install -g newman
    npm install -g newman-reporter-htmlextra
}

Write-Host "✅ Newman está instalado" -ForegroundColor Green
Write-Host ""

# 2. Verificar que la API está corriendo
Write-Host "🔍 Verificando que la API está corriendo..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ API está corriendo" -ForegroundColor Green
} catch {
    Write-Host "❌ La API no está corriendo en http://localhost:5000" -ForegroundColor Red
    Write-Host "💡 Ejecuta: dotnet run --project src/Renting.Api/Renting.Api.csproj" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# 3. Ejecutar tests de Newman
Write-Host "🧪 Ejecutando tests de Newman..." -ForegroundColor Green
Write-Host ""

newman run postman/Renting-API.postman_collection.json `
    --environment postman/Renting-API.postman_environment.json `
    --reporters cli,htmlextra `
    --reporter-htmlextra-export newman-reports/report.html `
    --color on `
    --delay-request 500

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "  ✅ TODOS LOS TESTS PASARON" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📊 Reporte HTML generado en: newman-reports/report.html" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "  ❌ ALGUNOS TESTS FALLARON" -ForegroundColor Red
    Write-Host "================================" -ForegroundColor Cyan
    exit 1
}