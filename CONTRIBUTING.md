# Contribuir

## Formato de cÃ³digo
- Se usa .editorconfig para formateo automÃ¡tico.
- IndentaciÃ³n: 4 espacios en C#.

## Pruebas
- Aplicar TDD: los cambios deben acompaÃ±arse de pruebas unitarias cuando afecten a lÃ³gica de negocio.
- Tests de integraciÃ³n y host deben existir para flujos crÃ­ticos.

## Workflow
- Trabajar en ramas feature/issue-xxx.
- Pull requests revisados y aprobados antes de merge.

## EstÃ¡ndares de arquitectura
- Hexagonal + DDD: separar Domain, Application, Infrastructure, Api.
- Repositorios: interfaces en Application, implementaciones en Infrastructure.
