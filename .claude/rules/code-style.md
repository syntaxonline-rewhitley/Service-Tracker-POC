---
description: Enforce core architectural patterns, naming conventions, and clean code formatting standards across the project.
applyTo: "**/*.ts, **/*.js, **/*.py, **/*.go"
---

# Code Style and Architecture Guardrails

## 1. Naming Conventions
- **Variables & Functions**: Use `camelCase` for variable names, functions, and object properties.
- **Classes & Types**: Use `PascalCase` for classes, interfaces, types, and components.
- **Constants**: Use uppercase `SNAKE_CASE` for global constants and environment variable keys.
- **Files & Directories**: Use `kebab-case` for file and folder names (e.g., `user-profile.ts`).

## 2. Functional & Clean Code Rules
- **Pure Functions**: Favor pure functions with predictable inputs and outputs. Avoid side effects where possible.
- **Single Responsibility**: Keep functions small and focused on a single task. Break down functions exceeding 30 lines.
- **Early Return**: Write functions that handle edge cases and errors first, returning early to avoid deeply nested `if` statements.
- **Explicit Types**: Do not use generic fallback types like `any`. Explicitly type all function signatures, parameters, and return structures.

## 3. Formatting & Readability
- **Comments**: Write self-documenting code. Use comments only to explain *why* code does something complex, not *what* it is doing.
- **Line Length**: Keep lines under 100 characters to maintain screen scannability.
- **Destructuring**: Use object destructuring for function arguments and configuration extractions to improve clarity.

## 4. Error Handling
- **No Silent Failures**: Never write empty `catch` or `except` blocks. Log errors explicitly or rethrow them.
- **Custom Exceptions**: Use or extend domain-specific error classes rather than throwing generic strings or untyped errors.
