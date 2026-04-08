# Automated Test Result Documentation

> **Use Case:** Copilot CLI generates structured, audit-friendly test reports from raw execution evidence — no manual writing needed.

---

## The Problem

After every test run, teams in regulated industries must produce documentation:

```
❌  Manual process today:
     CI produces JUnit XML + coverage reports
     → QA opens the XML, counts pass/fail/skip manually
     → copies numbers into a Word doc or wiki page
     → writes impact analysis for failures
     → sends for sign-off
     → 30-60 minutes per report, error-prone, nobody enjoys it
```

## The Solution

Feed the raw evidence files to Copilot CLI and let it generate the report:

```
✅  With Copilot CLI:
     CI produces JUnit XML + coverage reports (same as before)
     → reference evidence files with @ in Copilot CLI
     → Copilot parses, summarizes, assesses risk, drafts the report
     → human reviews and signs off
     → 2 minutes, consistent format every time
```

---

## The Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                        CI PIPELINE                              │
│                                                                 │
│  pytest --junitxml=results.xml --cov --cov-report=xml           │
│                                                                 │
│  Artifacts:                                                     │
│    📄 results.xml    (pass/fail/skip per test)                  │
│    📄 coverage.xml   (line + branch coverage per module)        │
│    📄 console.log    (raw pytest terminal output)               │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                      COPILOT CLI                                │
│                                                                 │
│  > @evidence/results.xml @evidence/coverage.xml                 │
│  > @evidence/console.log @templates/test_report.md              │
│  >                                                              │
│  > Generate a test execution report following the template.     │
│  > Include risk assessment and recommended actions for          │
│  > any failures.                                                │
│                                                                 │
│  Copilot:                                                       │
│    • Parses JUnit XML → test summary table                      │
│    • Parses coverage XML → coverage table with thresholds       │
│    • Analyzes failures → impact + recommended fix               │
│    • Evaluates risk → per-area risk level                       │
│    • Fills the template → complete markdown report              │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    👀 HUMAN REVIEW                              │
│                                                                 │
│  • Are the failure descriptions accurate?                       │
│  • Is the risk assessment reasonable?                           │
│  • Are recommended actions correct?                             │
│  • Fill in sign-off fields                                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                   📁 COMPLIANCE ARCHIVE                         │
│                                                                 │
│  Store alongside build artifacts with retention policy          │
│  (e.g., 7 years for SOX compliance)                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step-by-Step Walkthrough

### Step 1: Produce Evidence (CI does this already)

Your CI pipeline generates these files after every test run:

```bash
# Typical CI step — you likely have this already
python -m pytest tests/ -v \
  --junitxml=evidence/results.xml \
  --cov=src --cov-report=xml:evidence/coverage.xml \
  | tee evidence/console.log
```

This sample ships with pre-generated evidence in `evidence/` so you can
try the workflow immediately without running a real CI pipeline.

### Step 2: Feed Evidence to Copilot CLI

```bash
copilot

> @evidence/results.xml @evidence/coverage.xml @evidence/console.log
> @templates/test_report.md
>
> Generate a complete test execution report following the template.
> Parse the JUnit XML for test results and the coverage XML for
> code coverage. Assess risk for any failures. Fill in every
> section of the template.
```

### Step 3: Review What Copilot Generates

Copilot produces a complete markdown report. See [`example_output.md`](example_output.md)
for what the output looks like.

**Key things Copilot extracts automatically:**
- ✅ Test totals, pass rate, and duration from JUnit XML
- ✅ Per-module coverage with threshold warnings from coverage XML
- ✅ Failure details with traceback, impact analysis, and fix recommendations
- ✅ Skip reasons from the `<skipped>` elements
- ✅ Risk assessment based on which functional areas are affected

### Step 4: Refine with Follow-Up Prompts

Copilot CLI remembers context within a session. Ask follow-up questions:

```bash
# Drill into a specific failure
> Explain the dormant account reactivation failure in more detail.
> What is the most likely root cause based on the traceback?

# Add compliance context
> Add a section mapping each test class to its regulatory requirement:
> TestHighValueTransactions → BSA/AML reporting threshold
> TestFailedTransactions → SOX error-handling controls
> TestCrossBorderScenarios → OFAC compliance checks

# Reformat for a different audience
> Rewrite the executive summary for a non-technical compliance officer
```

### Step 5: Archive as Compliance Evidence

Save the report alongside your build artifacts:

```bash
# Copy Copilot's output to a file
# (use /copy in Copilot CLI to copy to clipboard, then paste)
# Or use /share to export the session

# In CI, store with your test artifacts
# GitHub Actions example:
#   - uses: actions/upload-artifact@v4
#     with:
#       name: test-report-${{ github.sha }}
#       path: reports/test-execution-report.md
#       retention-days: 2555   # 7 years for SOX
```

---

## Using the Skill for Consistency

Copy the `skill/SKILL.md` into your repo's `.github/skills/test-report/`
directory. Copilot CLI automatically applies it when you ask for test reports:

```bash
# After installing the skill:
copilot

> /skills list
# Shows: test-report — Generate structured test execution reports...

> @evidence/results.xml @evidence/coverage.xml
> Generate a test execution report
# Copilot now follows the skill's rules automatically:
# - All 7 sections present
# - Risk assessment included
# - Sign-off table at the bottom
# - Disclaimer about human review
```

See [`skill/SKILL.md`](skill/SKILL.md) for the full skill definition.

---

## Example Prompts for Different Reports

| What You Need | Copilot CLI Prompt |
|---|---|
| Full test report | `@evidence/results.xml @evidence/coverage.xml Generate a test execution report following @templates/test_report.md` |
| Failure analysis only | `@evidence/results.xml List every failed test with root cause analysis and recommended fix` |
| Coverage gaps | `@evidence/coverage.xml Which modules are below 85% coverage? What lines are uncovered?` |
| Executive summary | `@evidence/results.xml @evidence/coverage.xml Write a 3-sentence executive summary of test health` |
| Regression comparison | `@evidence/results.xml @evidence/previous-results.xml Compare these two test runs and highlight regressions` |
| Requirement traceability | `@evidence/results.xml Map each test class to its regulatory requirement (SOX, PCI-DSS, BSA/AML)` |

---

## Project Structure

```
test-result-docs/
├── README.md                        ← This file (the full flow)
├── example_output.md                ← What a generated report looks like
├── evidence/
│   ├── results.xml                  ← Sample JUnit XML (14 tests, 1 fail, 1 skip)
│   ├── coverage.xml                 ← Sample coverage XML (86% overall)
│   └── console.log                  ← Sample pytest console output
├── templates/
│   └── test_report.md               ← Report template with placeholder fields
└── skill/
    └── SKILL.md                     ← Copilot skill for consistent report generation
```

---

## Key Principles

1. **Copilot is the transformer, not the source of truth** — it reads evidence files, it doesn't invent results
2. **Human review is mandatory** — Copilot drafts the report, a human validates and signs off
3. **Never fabricate** — the skill explicitly instructs Copilot to only report what the evidence contains
4. **Template-driven consistency** — every report follows the same structure, regardless of who generates it
5. **Evidence stays in CI** — the XML artifacts are the authoritative record; the report is a summary layer
