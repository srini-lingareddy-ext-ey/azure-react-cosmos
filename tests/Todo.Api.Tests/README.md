# Todo.Api.Tests

Backend unit and integration tests for Todo.Api (AC-FOUNDATION-012).

## Stack

- **xUnit** — test framework
- **Moq** — mocking (e.g. `IRepository<T>`)
- **WebApplicationFactory** — integration tests over the full HTTP pipeline

## Traits

Tests are categorized for filtering:

- **FastLocal** — fast tests for local development
- **FullCI** — all tests, including integration, run in CI

Filter examples:

```bash
dotnet test --filter "Category=FastLocal"
dotnet test --filter "Category=FullCI"
```

## Structure

- **Builders** — test data builders (e.g. `TenantBuilder`) for reusable fixtures
- **Domain** — unit tests for domain entities
- **Application** — unit tests for services (with Moq) and validators
- **Integration** — WebApplicationFactory-based HTTP tests

## Coverage approach

- Target high-signal tests: business logic, validation, error paths, API behavior.
- Use builders and fixtures to keep tests readable and consistent.
- Integration tests cover real HTTP endpoints; unit tests use mocks for dependencies.
