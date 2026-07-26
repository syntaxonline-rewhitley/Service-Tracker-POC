# Security Audit Checklist

## Input Validation
- [ ] All user input sanitized before DB queries
- [ ] File upload MIME types validated
- [ ] Path traversal prevented on file operations

## Authentication
- [ ] JWT tokens expire after 24 hours
- [ ] API keys stored in environment variables
- [ ] Passwords hashed with bcrypt or argon2

## 🛠️ Security Audit Checklist
- [ ] Check for unvalidated inputs leading to SQLi, NoSQLi, or Command Injection.
- [ ] Inspect CORS configurations and route-level authorization middlewares.
- [ ] Identify hardcoded secrets, fallback credentials, or overly permissive IAM permissions.
- [ ] Audit dependencies against known CVEs via local lockfile analysis.

