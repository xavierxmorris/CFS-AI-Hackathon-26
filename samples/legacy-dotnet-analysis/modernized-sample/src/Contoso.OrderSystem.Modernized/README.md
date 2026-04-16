# Contoso.OrderSystem.Modernized

This .NET 8 sample implements the phased modernization plan next to the legacy source.

## Highlights
- Secrets are no longer stored in `appsettings.json`; set `PaymentGateway__MerchantSecret` via environment variables or a secret provider.
- Payment calls require HTTPS and an allowlisted host.
- Order processing is built around interfaces and constructor injection for testability.
- The ADO.NET sample uses parameterized SQL and async APIs.

## Run tests
```bash
dotnet test /home/runner/work/CFS-AI-Hackathon-26/CFS-AI-Hackathon-26/samples/legacy-dotnet-analysis/modernized-sample/Contoso.OrderSystem.Modernized.slnx
```
