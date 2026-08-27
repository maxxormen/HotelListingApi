# 🔐 API Authentication Hardening – AI Implementation Guide

**Skill Name:** `api-auth-hardening`  
**Target Framework:** .NET 10 / ASP.NET Core  
**Purpose:** This document provides precise, step-by-step instructions for an AI assistant to harden the authentication and authorization layers of a .NET API. The AI should follow these rules strictly, applying best practices and avoiding common pitfalls.

---

## 📋 Prerequisites

Before applying this hardening, ensure:
- The project targets **.NET 10** (or later).
- Authentication is configured (e.g., JWT, Identity, or custom token-based).
- `Microsoft.AspNetCore.Authentication.JwtBearer` and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` are installed (if using Identity).
- Environment variables or Azure Key Vault are available for secrets.

---

## 🧠 General AI Guidelines

When asked to implement authentication hardening, the AI MUST:

- Apply all steps in order, unless the user explicitly skips a step.
- Use `appsettings.json` + environment overrides for all secrets.
- Prefer `SymmetricSecurityKey` with a base64-encoded secret for simplicity, but recommend `RS256` for production.
- Never hardcode secrets — use `IConfiguration` or secure storage.
- Write clean, maintainable, and testable code.
- Add XML comments for public methods.

---