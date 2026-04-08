# Test Execution Report

> **Status:** ❌ **1 FAILURE** — review required before merge
> Generated from CI evidence by GitHub Copilot CLI

---

## 1. Environment

| Field           | Value                          |
|-----------------|--------------------------------|
| Project         | Payment Processing Service     |
| Date            | 2026-04-07 14:30:12 UTC        |
| Runner          | ci-runner-03                   |
| Python Version  | 3.12.3                         |
| Commit SHA      | *Not available — fill in from CI* |
| Branch          | *Not available — fill in from CI* |

---

## 2. Test Execution Summary

| Metric     | Count   |
|------------|---------|
| Total      | 14      |
| ✅ Passed   | 12      |
| ❌ Failed   | 1       |
| ⏭️ Skipped  | 1       |
| Duration   | 2.85s   |

**Pass Rate:** 85.7%

---

## 3. Coverage Summary

| Module                 | Statements | Missed | Coverage |
|------------------------|-----------|--------|----------|
| query_helper.py        | 18        | 0      | **100%** |
| payment_processor.py   | 34        | 6      | 82% ⚠️   |
| account_manager.py     | 27        | 6      | 78% ⚠️   |
| seed_test_data.py      | 22        | 2      | 91%      |

**Overall Coverage:** 86%
**Coverage Threshold:** 85%
**Threshold Met:** ✅ Yes (overall), but 2 modules are individually below threshold

---

## 4. Failed Tests

### `test_dormant_account_reactivation_sends_email`

- **Class:** `TestDormantAccountScenarios`
- **Error:** `AssertionError: Expected email to be sent on reactivation`
- **Traceback:**
  ```
  File "tests/test_payment_scenarios.py", line 142
      assert mock_email.called, "Expected email to be sent on reactivation"
  ```
- **Impact:** Dormant account reactivation flow does not trigger customer notification. Customers may not be informed when their account is reactivated. Medium business impact — not a financial calculation issue.
- **Recommended Action:** Verify that `reactivate_account()` in `account_manager.py` calls the email notification service. Check if the mock is patching the correct import path.

---

## 5. Skipped Tests

| Test | Reason |
|------|--------|
| `test_chargeback_notifies_merchant` | Merchant notification API not yet implemented |

---

## 6. Risk Assessment

| Area | Risk Level | Notes |
|------|-----------|-------|
| Financial calculations | ✅ LOW | All high-value, precision, and cross-border tests pass |
| Failed transactions handling | ✅ LOW | All failure-mode tests pass including frozen account |
| Account lifecycle | ⚠️ MEDIUM | Reactivation email test failing — notification gap |
| Payment processor coverage | ⚠️ MEDIUM | 82% coverage — uncovered branches at lines 45, 52, 58 |
| Account manager coverage | ⚠️ MEDIUM | 78% coverage — uncovered lines 35, 42, 61 |
| Chargeback workflow | ℹ️ INFO | Merchant notification skipped (API not implemented) |

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
