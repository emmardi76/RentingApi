# PRUEBA TÉCNICA .NET

Implementar un microservicio que permita gestionar la siguiente funcionalidad para una empresa de renting de vehículos:

## Funcionalidad Requerida (vía REST)

El microservicio debería permitir realizar las siguientes acciones:

*   **Gestión de flota**: Creación de nuevos vehículos para la flota.
*   **Consulta de disponibilidad**: Listar los vehículos disponibles actualmente.
*   **Alquiler**: Poder alquilar un vehículo para un cliente.
*   **Devolución**: Registrar la devolución de un vehículo alquilado.

## Restricciones de Negocio

*   **Límite de Reservas**: Una misma persona no debería poder reservar más de un vehículo al mismo tiempo.
*   **Antigüedad de la Flota**: La flota no debe contener vehículos cuya fecha de fabricación sea superior a 5 años.

---

## Arquitectura y Patrones

Haciendo uso de un **template estándar de Arquitectura Hexagonal y DDD (Domain-Driven Design)** e implementando los patrones indicados en este tipo de arquitecturas (Entidades, Agregados, Repositorios, Servicios de Aplicación, etc.), se debe implementar dicho microservicio.

## Metodología y Herramientas

*   **Asistencia por IA**: Se requiere que el candidato haga uso de herramientas de **IA (como GitHub Copilot)** para la generación del código, demostrando eficiencia en el uso de estas tecnologías.
*   **TDD (Test-Driven Development)**: La implementación de las pruebas unitarias debe seguir una metodología **TDD**, asegurando que el diseño del código está dirigido por los tests.

## Pruebas Automáticas

La implementación debe facilitar el desarrollo de pruebas automáticas. No es necesaria la implementación de todas las pruebas para el código completo, pero sí **un ejemplo funcional de cada una**:

1.  **Infraestructura**: Probar uno de los métodos REST implementados. Solo a nivel de Host (recepción de la llamada y validación del modelo).
2.  **Unitaria (TDD)**: Validar de forma unitaria uno de los métodos o lógicas de negocio, aplicando la metodología **TDD**.
3.  **Funcional**: Realizar una prueba de integración que valide un flujo de negocio (excluyendo la capa de host/red).
4.  **Newman/Postman**: Implementar una suite de pruebas utilizando **Postman** que pueda ser ejecutada mediante **Newman** para validar los contratos de los endpoints.

## Requisitos de Ejecución

El microservicio debería de poder ejecutarse en local por cualquier persona sin necesidad de instalar ninguna dependencia externa. Se requiere el uso de **Docker** y/o **Docker-Compose** para asegurar este punto (incluyendo base de datos si fuera necesario).

## Entrega

Para enviarnos la solución, por favor genera un archivo **ZIP** de la carpeta con todo el código fuente y las instrucciones necesarias para su ejecución.
