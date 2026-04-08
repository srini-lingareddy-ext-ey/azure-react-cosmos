# Validation layers (AC-FOUNDATION-008.7)

Validation is split across three layers. Each layer owns specific failures and HTTP semantics.

| Layer | Responsibility | Typical HTTP status | Examples |
|-------|----------------|---------------------|----------|
| **Transport / input** | Request shape, required fields, types, string length, format | **400 Bad Request** | Missing body field, empty string where required, value too long |
| **Business invariants** | Domain rules, state transitions | **422 Unprocessable Entity** or **409 Conflict** | Invalid state change, negative price |
| **Persistence** | Uniqueness, referential integrity, optimistic concurrency | **409 Conflict** or **412 Precondition Failed** | Duplicate key, ETag mismatch |

**Execution order:** transport validation (FluentValidation, before the controller action runs) → service → domain methods → repository → database constraints.

**This codebase**

- **FluentValidation (transport only)** — `AbstractValidator` types registered via `ValidationAssemblyMarker` must target **inbound request models** under `Application.Transport` (subclass `TransportValidatorBase<T>`). Failures → `VALIDATION_FAILED` + `ApiValidationErrorEnvelope.errors`.
- **Other application checks** — domain validation in services or manual validators outside the HTTP pipeline; it is **not** part of the FluentValidation MVC pipeline. Never register FluentValidation validators for domain entities on this scan.
- **Domain** — entity methods throw or return errors for business rules; map to 422/409 in services or exception middleware.
- **Infrastructure** — Cosmos / DB errors mapped to 409/412 as appropriate.

See the API Server blueprint *Validation at the Application Layer* for full conventions.
