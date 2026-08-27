# GitHub Copilot Instructions for C# / .NET

## General Style
- Follow C# coding conventions (PascalCase for public members, camelCase for private fields and parameters).
- Use `readonly` for fields that are only set in the constructor.
- Keep methods focused and small (single responsibility principle).
- Use `var` only when the type is obvious (e.g., `var list = new List<string>();`).
- Prefer `=>` expression-bodied members for simple getters and methods.

## Error Handling
- Use specific exceptions (e.g., `ArgumentException`, `InvalidOperationException`) rather than general `Exception`.
- Avoid swallowing exceptions unless explicitly required; always log or rethrow.

## Comments
- Use XML comments for public API members (`/// <summary>`).
- Avoid obvious comments like `// increment i`. Prefer code that is self-documenting.
- Use `// TODO:` to mark unfinished work.

## Naming
- Use meaningful, intention-revealing names.
- Prefix interfaces with `I`.
- Name asynchronous methods with the `Async` suffix.
- Use `_` prefix for private fields (e.g., `_logger`).

## Dependency Injection
- Prefer constructor injection for required dependencies.
- Use property injection only for optional dependencies.
- Do not use service locator pattern.

## Unit Testing (xUnit)
- Name test methods clearly: `MethodName_Scenario_ExpectedBehavior`.
- Use `[Theory]` and `[InlineData]` for parameterized tests.
- Avoid multiple assertions in a single test if possible.
- Mock external dependencies using a mock framework (e.g., NSubstitute, Moq).

## Performance
- Prefer `IEnumerable<T>` for read-only collections; use `List<T>` only when modification is needed.
- Avoid `LINQ` in tight loops when performance is critical.
- Use `StringBuilder` for complex string concatenation in loops.