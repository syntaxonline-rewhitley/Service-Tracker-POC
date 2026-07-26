---
description: Enforce critical security boundaries, injection prevention, and credential protection across all source files.
globs:
  - "src/**/*"
  - "api/**/*"
  - "server/**/*"
  - "config/**/*"
  - "*.tf"
---

# Global Security Guardrails

## 1. Credentials & Secrets Management
- **Zero Hardcoding**: Never hardcode API keys, passwords, tokens, or private keys.
- **Environment Variables**: Always load secrets from environment variables or a verified secrets manager.
- **Git Safety**: Verify that configuration files containing local secrets are explicitly listed in `.gitignore`.

## 2. Injection & Input Defenses
- **Database Queries**: Use parameterized queries, prepared statements, or ORM methods. Never use string interpolation or concatenation for database inputs.
- **Input Validation**: Enforce strict schema validation and sanitization at all API boundaries before processing data.
- **Command Execution**: Avoid passing raw user input directly to system shell execution utilities.

## 3. Data Protection & Privacy
- **Logging Guardrails**: Never write Personally Identifiable Information (PII), session tokens, passwords, or medical data to standard output or error logs.
- **Error Messages**: Mask internal system errors and stack traces. Return generic error messages to the client.
- **Output Encoding**: Contextually encode all user-controlled data before rendering it in web views to prevent cross-site scripting (XSS).

## 4. Dependencies & Supply Chain
- **Lockfiles**: Ensure lockfiles (e.g., `package-lock.json`, `poetry.lock`, `Gemfile.lock`) are updated whenever dependencies change.
- **Version Pinning**: Avoid using wildcards (`*`) for critical security dependencies. Use specific, pinned versions.
