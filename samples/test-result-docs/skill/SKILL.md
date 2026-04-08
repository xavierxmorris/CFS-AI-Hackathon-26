---
name: test-report
description: Generate structured test execution reports from JUnit XML, coverage XML, and console output — use when creating test documentation, compliance reports, or audit evidence
---

# Test Execution Report Skill

When generating a test execution report from evidence files, follow this structure.

## Required Input

The user provides one or more of these evidence files via `@` references:
- **JUnit XML** (`results.xml`) — test names, pass/fail/skip, duration, error messages
- **Coverage XML** (`coverage.xml`) — line and branch coverage per module
- **Console log** (`console.log`) — raw pytest output with summary

## Report Sections (all required)

### 1. Environment
Extract from evidence: timestamp, hostname/runner, Python version, platform.
If commit SHA or branch aren't in evidence, leave as "Not available — fill in from CI."

### 2. Test Execution Summary
Parse totals from JUnit XML `<testsuite>` attributes: tests, failures, errors, skipped, time.
Calculate pass rate: `(total - failures - errors) / total * 100`.

### 3. Coverage Summary
Parse from coverage XML `<class>` elements: filename, line-rate (convert to percentage).
Overall coverage from root `<coverage>` element `line-rate` attribute.
Flag modules below 85% coverage threshold.

### 4. Failed Tests (detail each failure)
For every `<testcase>` with a `<failure>` child:
- Test name and class
- Error message (from the `message` attribute)
- Traceback summary (first 3 lines)
- Impact assessment: what area of the system is affected?
- Recommended action: what should the developer do next?

### 5. Skipped Tests
For every `<testcase>` with a `<skipped>` child:
- Test name and skip reason (from `message` attribute)

### 6. Risk Assessment
Evaluate risk based on:
- Any failures in critical financial paths → HIGH risk
- Coverage below 85% on payment/transaction modules → MEDIUM risk
- Only cosmetic or notification failures → LOW risk
- All green → MINIMAL risk

### 7. Sign-Off
Always include a blank sign-off table for QA Lead, Dev Lead, and Compliance.
Always include the disclaimer: "This report was drafted by Copilot from execution evidence. Human review required."

## Formatting Rules

- Use markdown tables (not bullet lists) for summary data
- Use ✅ ❌ ⏭️ emoji for pass/fail/skip
- Bold the overall status in the header
- Keep the report under 200 lines
- Never fabricate test results — only report what the evidence contains
