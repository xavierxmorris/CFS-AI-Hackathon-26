# Test Execution Report

> **Status:** {{OVERALL_STATUS}}
> Generated from CI evidence by GitHub Copilot CLI

---

## 1. Environment

| Field           | Value               |
|-----------------|---------------------|
| Project         | {{PROJECT_NAME}}    |
| Date            | {{EXECUTION_DATE}}  |
| Runner          | {{RUNNER_ID}}       |
| Python Version  | {{PYTHON_VERSION}}  |
| Commit SHA      | {{COMMIT_SHA}}      |
| Branch          | {{BRANCH}}          |

---

## 2. Test Execution Summary

| Metric   | Count |
|----------|-------|
| Total    | {{TOTAL}} |
| ✅ Passed | {{PASSED}} |
| ❌ Failed | {{FAILED}} |
| ⏭️ Skipped | {{SKIPPED}} |
| Duration | {{DURATION}} |

**Pass Rate:** {{PASS_RATE}}%

---

## 3. Coverage Summary

| Module | Statements | Missed | Coverage |
|--------|-----------|--------|----------|
| {{MODULE_ROWS}} |

**Overall Coverage:** {{OVERALL_COVERAGE}}%
**Coverage Threshold:** 85%
**Threshold Met:** {{THRESHOLD_MET}}

---

## 4. Failed Tests

> If no failures, write: "✅ All tests passed — no failures to report."

### {{FAILED_TEST_NAME}}

- **Class:** {{TEST_CLASS}}
- **Error:** {{ERROR_MESSAGE}}
- **Impact:** {{IMPACT_ASSESSMENT}}
- **Recommended Action:** {{RECOMMENDED_ACTION}}

---

## 5. Skipped Tests

> List each skipped test with the reason.

| Test | Reason |
|------|--------|
| {{SKIPPED_ROWS}} |

---

## 6. Risk Assessment

| Area | Risk Level | Notes |
|------|-----------|-------|
| {{RISK_ROWS}} |

---

## 7. Sign-Off

| Role | Name | Date | Status |
|------|------|------|--------|
| QA Lead | _________________ | __________ | ☐ Approved |
| Dev Lead | _________________ | __________ | ☐ Approved |
| Compliance | _________________ | __________ | ☐ Approved |

---

*This report was drafted by GitHub Copilot CLI from test execution evidence.*
*Human review and sign-off are required before this document is considered final.*
