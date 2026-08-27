# .NET 10 API Development Instructions

## Project Structure
- Use Clean Architecture / Onion Architecture:
    - `Domain` (Entities, Value Objects, Interfaces)
    - `Application` (Use Cases, DTOs, Interfaces for external dependencies)
    - `Infrastructure` (Persistence, External Services, Repositories)
    - `API` (Controllers, Middleware, Configuration)

## Dependency Injection
- Register dependencies in `Program.cs` using `AddScoped`, `AddTransient`, or `AddSingleton` as appropriate.
- Use `IServiceCollection` extensions for each layer (e.g., `AddApplication()`, `AddInfrastructure()`).

## Controllers
- Use `[ApiController]` attribute.
- Use attribute routing: `[Route("api/[controller]")]`.
- Keep controllers thin; delegate business logic to services.
- Return `IActionResult` or `ActionResult<T>`.
- Use `[FromBody]`, `[FromQuery]`, `[FromRoute]` explicitly.

## Validation
- Use FluentValidation for request DTOs.
- Validate inputs before processing.

## Error Handling
- Use a global exception handling middleware.
- Return consistent error responses (Problem Details RFC 7807).
- Log all exceptions using `ILogger`.

## Logging
- Use `ILogger<T>` where `T` is the class name.
- Log at appropriate levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`.

## Data Access
- Use Entity Framework Core with a repository pattern (or a Unit of Work pattern).
- Use migrations for schema changes.
- Avoid raw SQL unless necessary.

## Security
- Use `[Authorize]` attributes for protected endpoints.
- Use JWT (Bearer) for authentication.
- Store connection strings and secrets in `appsettings.json` + environment variables / Azure Key Vault.
- Implement proper CORS policies.

## API Documentation
- Use Swagger/OpenAPI (via Swashbuckle).
- Include XML comments in Swagger.
- Version your API using URL or header versioning.

## Performance
- Use async/await for I/O-bound operations.
- Implement response caching where appropriate.
- Use pagination for large data sets (e.g., `skip` and `take`).

## Testing
- Write unit tests for business logic.
- Write integration tests for API endpoints.
- Use a test database (InMemory or SQLite) for integration tests.