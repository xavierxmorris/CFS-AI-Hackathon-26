# Legacy .NET Code Analysis with Copilot CLI

> **Use Case:** Copilot CLI explains legacy .NET code in plain English, generates visual flow diagrams, maps dependencies, and provides incremental modernization guidance.

---

## The Problem

Enterprise teams inherit legacy .NET codebases where:

```
❌  The original developers left years ago
     → No one knows what OrderProcessor actually does
     → "It works, don't touch it" is the only documentation
     → New developers take weeks to understand the flow
     → Modernization is postponed because the risk is unknown
```

## The Solution

Feed the legacy source files to Copilot CLI and get instant understanding:

```
✅  With Copilot CLI:
     → Plain-English explanation of every class and method
     → Mermaid flow diagrams you can paste into docs or PRs
     → Dependency maps showing hidden coupling
     → Phased modernization plan (incremental, not rewrite)
     → All in minutes, not weeks
```

---

## The Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                     LEGACY .NET CODEBASE                        │
│                                                                 │
│  OrderProcessor.cs — God class, mixed concerns                  │
│  CustomerRepository.cs — ADO.NET, inline SQL                    │
│  PaymentService.cs — Tightly coupled to gateway                 │
│  Web.config — Connection strings, secrets, settings             │
└────────────────────────┬────────────────────────────────────────┘
                         │  Reference with @file
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                       COPILOT CLI                               │
│                                                                 │
│  Step 1: "Explain this class in plain English"                  │
│  Step 2: "Generate a Mermaid sequence diagram"                  │
│  Step 3: "Map dependencies between all classes"                 │
│  Step 4: "Assess security risks and code quality"               │
│  Step 5: "Create a phased modernization plan"                   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    👀 HUMAN REVIEW                              │
│                                                                 │
│  • Does the explanation match what the system actually does?    │
│  • Are the diagrams accurate to real production behavior?       │
│  • Does the team agree with the risk assessment?                │
│  • Is the modernization order right for our priorities?         │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                   📁 TEAM DOCUMENTATION                         │
│                                                                 │
│  • Onboarding wiki for new developers                           │
│  • Architecture Decision Records (ADRs)                         │
│  • Modernization roadmap in project tracker                     │
│  • PR descriptions referencing the analysis                     │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step-by-Step Walkthrough

### The Legacy System

This sample includes a realistic legacy ASP.NET order-processing system
(~400 lines across 3 classes + config), targeting .NET Framework 4.5:

| File | Lines | What It Does | Legacy Pattern |
|------|-------|-------------|----------------|
| `OrderProcessor.cs` | ~190 | Orchestrates the full order lifecycle | God class — mixed concerns |
| `CustomerRepository.cs` | ~120 | Data access for customer records | ADO.NET, one SQL injection risk |
| `PaymentService.cs` | ~130 | External payment gateway integration | HttpWebRequest, hardcoded coupling |
| `Web.config` | ~30 | Connection strings and app settings | Secrets in config file |

**You don't need .NET installed.** These are source files for Copilot to read — no compilation required.

---

### Step 1: Explain in Plain English

```bash
copilot

> @legacy-code/OrderProcessor.cs
> Explain this class in plain English. What does it do step by step?
> Assume I'm a new developer who has never seen this codebase.
```

**What you get:** A walkthrough like [order-processor-explained.md](example-outputs/order-processor-explained.md) — every step of `ProcessOrder` described in plain language, with notes on what an experienced developer should watch out for.

---

### Step 2: Generate Visual Flow Diagrams

```bash
> @legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
> @legacy-code/PaymentService.cs
>
> Generate a Mermaid sequence diagram showing the happy-path flow
> when a customer places an order. Show every class and external
> system involved.
```

**What you get:** Mermaid diagrams you can paste directly into GitHub markdown, PRs, or wiki pages. See [order-processor-flow.md](example-outputs/order-processor-flow.md) for:
- **Sequence diagram** — who calls whom in what order
- **Failure flowchart** — every point where the order can fail
- **Data flow diagram** — inputs, processing, external systems

---

### Step 3: Map Dependencies

```bash
> @legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
> @legacy-code/PaymentService.cs @legacy-code/Web.config
>
> Generate a dependency map showing which classes depend on which.
> Include shared dependencies and highlight hidden coupling.
```

**What you get:** A dependency graph and coupling analysis like [dependency-map.md](example-outputs/dependency-map.md) — showing that `OrderProcessor` has 6 direct dependencies and zero interface abstractions.

---

### Step 4: Assess Risks

```bash
> @legacy-code/CustomerRepository.cs @legacy-code/PaymentService.cs
> @legacy-code/Web.config
>
> Review for security vulnerabilities. Check for SQL injection,
> hardcoded secrets, and insecure communication.
> Rate each finding as CRITICAL, HIGH, MEDIUM, or LOW.
```

**What Copilot finds in this sample:**
- 🔴 **CRITICAL** — SQL injection in `FindByEmail()` (string concatenation)
- 🟠 **HIGH** — Merchant secret in `Web.config` (should be in Key Vault)
- 🟡 **MEDIUM** — SMTP on port 25 without TLS
- 🟡 **MEDIUM** — No retry logic on payment gateway calls

---

### Step 5: Plan Modernization (Incremental)

```bash
> @legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
> @legacy-code/PaymentService.cs @legacy-code/Web.config
>
> Create a phased modernization plan. Rules:
> - No big-bang rewrite, incremental phases only
> - Fix security issues first
> - Make it testable before changing behavior
> - Suggest modern .NET 8+ replacements for each legacy pattern
```

**What you get:** A 5-phase plan like [modernization-plan.md](example-outputs/modernization-plan.md):

| Phase | What | Why This Order |
|-------|------|----------------|
| 1 | Fix SQL injection | Live vulnerability — do it now |
| 2 | Extract interfaces | Enables unit testing without behavior changes |
| 3 | Add tests for current behavior | Safety net before any refactoring |
| 4 | Separate concerns from God class | Smaller, focused classes |
| 5 | Modernize infrastructure | .NET 8+, EF Core, HttpClient, ILogger |

---

### Step 6: Human Validates

Copilot's analysis is a **starting point, not the final word**. The team must verify:

- ✅ Does the explanation match what the system *actually* does in production?
- ✅ Are there undocumented behaviors Copilot missed (e.g., stored procedures, triggers)?
- ✅ Does the modernization order make sense for the team's priorities and capacity?
- ✅ Are there downstream systems that depend on current behavior?

---

## More Prompts

See [`prompts/analysis-prompts.md`](prompts/analysis-prompts.md) for a full catalog of
proven prompts organized by task:

1. **Understand** — plain English explanations, config mapping, non-technical summaries
2. **Visualize** — sequence diagrams, flowcharts, data flows, dependency graphs
3. **Assess** — security review, code quality, technical debt inventory
4. **Plan** — modernization phases, interface extraction, test plans, migration checklists
5. **Deep Dives** — multi-turn conversations that drill into specific concerns

---

## Project Structure

```
legacy-dotnet-analysis/
├── README.md                           ← This file (the full flow)
├── legacy-code/
│   ├── OrderProcessor.cs               ← God class — order lifecycle orchestrator
│   ├── CustomerRepository.cs           ← ADO.NET data access + one SQL injection
│   ├── PaymentService.cs               ← External gateway integration
│   └── Web.config                      ← Legacy XML config with secrets
├── example-outputs/
│   ├── order-processor-explained.md    ← Plain English walkthrough
│   ├── order-processor-flow.md         ← Mermaid sequence + flow diagrams
│   ├── dependency-map.md               ← Class dependency graph + coupling table
│   └── modernization-plan.md           ← 5-phase incremental modernization
└── prompts/
    └── analysis-prompts.md             ← Full catalog of proven analysis prompts
```

---

## Key Principles

1. **Copilot reads, humans validate** — AI explains the code; the team confirms against reality
2. **Visualize before refactoring** — Mermaid diagrams make invisible dependencies visible
3. **Incremental, not rewrite** — modernize in phases; each phase is independently shippable
4. **Test before changing** — extract interfaces and add tests *before* modifying behavior
5. **Security first** — fix vulnerabilities before any feature work
6. **No .NET required** — these are source files for analysis, not a runnable project
