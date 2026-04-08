# CFS AI Hackathon 2026

**AI-Powered Quality Engineering for Financial Services**

Three practical demos showing how GitHub Copilot CLI transforms testing, documentation, and legacy code analysis in regulated financial services environments.

> Built at the CFS AI Hackathon 2026 — showcasing AI-assisted engineering workflows that meet enterprise compliance requirements.

---

## 🎯 What's Inside

| # | Demo | Use Case | Key Output |
|---|------|----------|------------|
| 1 | [Test Data Locator](#-demo-1-natural-language-test-data-discovery) | Find test data using plain English instead of SQL | SQL queries generated from natural language |
| 2 | [Test Result Documentation](#-demo-2-automated-test-result-documentation) | Generate audit-ready reports from test evidence | Structured markdown reports from JUnit/coverage XML |
| 3 | [Legacy .NET Analysis](#-demo-3-legacy-net-code-analysis) | Understand legacy code with flow diagrams and rebuild plans | Plain English explanations + Mermaid diagrams |

---

## 🚀 Quick Start

### Prerequisites

- [GitHub Copilot CLI](https://docs.github.com/copilot/concepts/agents/about-copilot-cli) installed and authenticated
- Python 3.10+ (for Demo 1 only)
- No .NET SDK required — Demo 3 uses source files for analysis, not compilation

### Clone and Explore

```bash
git clone https://github.com/xavierxmorris/CFS-AI-Hackathon-26.git
cd CFS-AI-Hackathon-26
```

Then pick a demo below and follow the walkthrough.

---

## 📋 Demo 1: Natural-Language Test Data Discovery

> **Problem:** QA engineers waste hours writing SQL to find the right test data in large financial databases.
>
> **Solution:** Describe what you need in plain English. Copilot CLI generates the SQL.

### Flow

```
Developer asks plain English  →  Copilot CLI + @schema.sql  →  SQL generated
  →  Human reviews (SELECT only!)  →  Run against test DB  →  Use in pytest
```

### Walkthrough

**1. Seed the test database:**
```bash
cd samples/test-data-locator
python seed_test_data.py
```

**2. Launch Copilot CLI and ask for data:**
```bash
copilot

> @schema.sql Find me a customer with a frozen account and a failed transaction
```

**3. Copilot generates the SQL:**
```sql
SELECT t.*, a.status AS account_status, c.name
FROM transactions t
JOIN accounts a ON t.account_id = a.account_id
JOIN customers c ON a.customer_id = c.customer_id
WHERE t.status = 'failed' AND a.status = 'frozen';
```

**4. Review and run** — the query helper enforces read-only access:
```python
from query_helper import run_select
rows = run_select("test_data.db", "<SQL from Copilot>")
```

**5. Run the full test suite (17 tests, 6 scenarios):**
```bash
python -m pytest tests/ -v
```

### What's Included

| File | Purpose |
|------|---------|
| `schema.sql` | 3-table financial schema — reference with `@schema.sql` |
| `seed_test_data.py` | Seeds SQLite with 4 customers, 7 accounts, 9 transactions |
| `query_helper.py` | Read-only query runner (rejects INSERT/DELETE/DROP) |
| `tests/` | 17 passing tests across 6 financial scenarios |

### Scenarios Covered

1. ✅ VIP customer lookup (KYC-verified, multi-account)
2. ✅ Failed transactions (error recovery testing)
3. ✅ High-value transactions (regulatory threshold > $100K)
4. ✅ Cross-border payments (multi-currency / FX)
5. ✅ Dormant accounts (lifecycle / reactivation)
6. ✅ Reversed transactions (chargeback workflows)

📖 [Full documentation →](samples/test-data-locator/README.md)

---

## 📋 Demo 2: Automated Test Result Documentation

> **Problem:** After every test run, regulated teams manually write test reports for compliance — 30-60 minutes each, error-prone, nobody enjoys it.
>
> **Solution:** Feed raw CI evidence to Copilot CLI. It generates the report in 2 minutes.

### Flow

```
CI produces artifacts  →  @evidence files in Copilot CLI  →  Structured report
  →  Human reviews + signs off  →  Archive for compliance
```

### Walkthrough

**1. Evidence files are ready** (simulating what CI produces):
```
samples/test-result-docs/evidence/
├── results.xml      ← JUnit XML (14 tests, 1 failure, 1 skip)
├── coverage.xml     ← Coverage XML (86% overall)
└── console.log      ← Raw pytest output
```

**2. Feed evidence to Copilot CLI:**
```bash
copilot

> @evidence/results.xml @evidence/coverage.xml @evidence/console.log
> @templates/test_report.md
>
> Generate a complete test execution report following the template.
> Include risk assessment and recommended actions for any failures.
```

**3. Copilot generates a structured report with:**
- ✅ Test execution summary (pass/fail/skip/duration)
- ✅ Per-module coverage with threshold warnings
- ✅ Failure analysis with root cause and recommended fix
- ✅ Risk assessment per functional area
- ✅ Sign-off table for QA Lead, Dev Lead, Compliance

**4. Review, sign off, and archive.**

### What's Included

| File | Purpose |
|------|---------|
| `evidence/results.xml` | Sample JUnit XML with realistic results |
| `evidence/coverage.xml` | Sample coverage XML (2 modules below threshold) |
| `evidence/console.log` | Raw pytest console output |
| `templates/test_report.md` | Report template with placeholder fields |
| `skill/SKILL.md` | Copilot skill for consistent report generation |
| `example_output.md` | Complete example of what Copilot generates |

📖 [Full documentation →](samples/test-result-docs/README.md)

---

## 📋 Demo 3: Legacy .NET Code Analysis

> **Problem:** Teams inherit legacy .NET systems where no documentation exists, the original developers are gone, and "it works, don't touch it" is the only knowledge.
>
> **Solution:** Feed the source files to Copilot CLI. Get plain English explanations, Mermaid flow diagrams, dependency maps, and an incremental modernization plan.

### Flow

```
Legacy C# + Web.config  →  @files in Copilot CLI  →
  Step 1: Plain English explanation
  Step 2: Mermaid sequence/flow diagrams
  Step 3: Dependency + coupling map
  Step 4: Security + quality risk assessment
  Step 5: Phased modernization plan
  Step 6: Human validates against reality
```

### Walkthrough

**1. The legacy system** (~400 lines, .NET Framework 4.5, circa 2014):

| File | What It Does | Legacy Problem |
|------|-------------|----------------|
| `OrderProcessor.cs` | Order lifecycle orchestrator | God class — mixed concerns |
| `CustomerRepository.cs` | Customer data access | ADO.NET + one SQL injection |
| `PaymentService.cs` | Payment gateway integration | HttpWebRequest, hardcoded coupling |
| `Web.config` | Settings and secrets | Secrets in config file |

**2. Ask Copilot CLI to explain:**
```bash
copilot

> @legacy-code/OrderProcessor.cs
> Explain this class in plain English step by step.
> Assume I'm a new developer who has never seen this codebase.
```

**3. Ask for flow diagrams:**
```bash
> @legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
> @legacy-code/PaymentService.cs
>
> Generate a Mermaid sequence diagram showing the happy-path order flow.
```

**4. Map dependencies:**
```bash
> Generate a dependency graph. Include external systems.
> Highlight hidden coupling through shared config keys.
```

**5. Get a modernization plan:**
```bash
> Create a phased modernization plan.
> Fix security issues first. Make it testable before changing behavior.
> No big-bang rewrite — incremental phases only.
```

Copilot produces a **5-phase plan:**

| Phase | Action | Why This Order |
|-------|--------|----------------|
| 1 | Fix SQL injection in `FindByEmail()` | Live vulnerability |
| 2 | Extract interfaces (ICustomerRepository, IPaymentService) | Enable unit testing |
| 3 | Add tests for current behavior | Safety net before refactoring |
| 4 | Separate concerns from God class | Smaller, focused classes |
| 5 | Upgrade to .NET 8+ (EF Core, HttpClient, ILogger) | Modern platform |

**No .NET SDK required** — these are source files for analysis, not a runnable project.

### What's Included

| File | Purpose |
|------|---------|
| `legacy-code/` | 3 C# files + Web.config (realistic legacy patterns) |
| `example-outputs/` | 4 pre-generated Copilot outputs (explanation, diagrams, deps, plan) |
| `prompts/analysis-prompts.md` | 15+ proven prompts organized by task |

📖 [Full documentation →](samples/legacy-dotnet-analysis/README.md)

---

## 🏦 Enterprise Compliance Notes

These demos are designed for **regulated financial services** environments:

| Principle | How It's Applied |
|-----------|-----------------|
| **Human-in-the-loop** | Every flow ends with human review and sign-off |
| **Read-only data access** | Test data locator rejects non-SELECT queries |
| **No fabricated results** | Test report skill instructs Copilot to only report evidence content |
| **Audit trail** | Reports include sign-off tables and archive guidance |
| **Incremental modernization** | Legacy analysis recommends phases, not rewrites |
| **Security first** | SQL injection fix is always Phase 1 |

---

## 📁 Repository Structure

```
CFS-AI-Hackathon-26/
├── README.md                              ← You are here
└── samples/
    ├── test-data-locator/                 ← Demo 1: NL → SQL test data discovery
    │   ├── schema.sql
    │   ├── seed_test_data.py
    │   ├── query_helper.py
    │   └── tests/
    ├── test-result-docs/                  ← Demo 2: Evidence → Audit report
    │   ├── evidence/
    │   ├── templates/
    │   ├── skill/
    │   └── example_output.md
    └── legacy-dotnet-analysis/            ← Demo 3: Legacy .NET → Understanding
        ├── legacy-code/
        ├── example-outputs/
        └── prompts/
```

---

## 🛠️ Technology Stack

| Component | Technology |
|-----------|-----------|
| AI Assistant | GitHub Copilot CLI |
| Test Framework | Python / pytest |
| Test Database | SQLite |
| CI Evidence | JUnit XML, Cobertura coverage XML |
| Legacy Code | C# / .NET Framework 4.5 (source only) |
| Diagrams | Mermaid (rendered in GitHub markdown) |

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.

---

## 👥 Authors

Built at the **CFS AI Hackathon 2026** by the team at [xavierxmorris](https://github.com/xavierxmorris).

> *"AI doesn't replace the engineer — it removes the tedious parts so you can focus on what matters."*
