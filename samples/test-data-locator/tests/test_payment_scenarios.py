"""Tests demonstrating Copilot CLI's natural-language test data discovery.

Each test below was written using a simple workflow:

    1. Ask Copilot CLI in plain English what data you need
    2. Give it @schema.sql so it knows the DB structure
    3. Review the SQL Copilot generates
    4. Use the query in your test

The comments above each test show the EXACT Copilot CLI prompt that
produced the query. Try them yourself!
"""

import sys
from decimal import Decimal
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from query_helper import find_customer, find_transactions, run_select  # noqa: E402


# =====================================================================
# Scenario 1 — Find a VIP customer for premium-tier testing
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql Find me a VIP customer in the US who has passed KYC
#
# Copilot generated:
#   SELECT * FROM customers
#   WHERE tier = 'vip' AND country_code = 'US' AND kyc_verified = 1;

class TestVipCustomerLookup:
    """Tests that use a VIP customer discovered via natural language."""

    def test_find_vip_customer_exists(self, db_path: str) -> None:
        results = find_customer(db_path, tier="vip", country_code="US")
        assert len(results) >= 1
        assert results[0]["kyc_verified"] == 1

    def test_vip_customer_has_multiple_accounts(self, db_path: str) -> None:
        # Follow-up prompt:
        #   How many accounts does the VIP customer have?
        #
        # Copilot generated:
        #   SELECT COUNT(*) as cnt FROM accounts WHERE customer_id = 'C001';
        rows = run_select(
            db_path,
            "SELECT COUNT(*) as cnt FROM accounts WHERE customer_id = ?",
            ("C001",),
        )
        assert rows[0]["cnt"] == 3


# =====================================================================
# Scenario 2 — Find a failed transaction for error-handling tests
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql I need a failed transaction to test our error recovery.
#   Find all failed transactions with the account and customer details.
#
# Copilot generated:
#   SELECT t.*, a.status AS account_status, c.name AS customer_name
#   FROM transactions t
#   JOIN accounts a ON t.account_id = a.account_id
#   JOIN customers c ON a.customer_id = c.customer_id
#   WHERE t.status = 'failed';

class TestFailedTransactions:
    """Tests using failed transactions found via natural language."""

    def test_failed_transactions_exist(self, db_path: str) -> None:
        failed = find_transactions(db_path, status="failed")
        assert len(failed) >= 1

    def test_failed_tx_has_description(self, db_path: str) -> None:
        failed = find_transactions(db_path, status="failed")
        for tx in failed:
            assert tx["description"], "Every failed tx should explain why"

    def test_frozen_account_tx_fails(self, db_path: str) -> None:
        # Prompt: Find me a failed transaction on a frozen account
        rows = run_select(
            db_path,
            """
            SELECT t.*, a.status AS account_status
            FROM transactions t
            JOIN accounts a ON t.account_id = a.account_id
            WHERE t.status = 'failed' AND a.status = 'frozen'
            """,
        )
        assert len(rows) >= 1
        assert rows[0]["account_status"] == "frozen"


# =====================================================================
# Scenario 3 — High-value transaction for regulatory threshold tests
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql Find transactions over $100,000 that would trigger
#   regulatory reporting requirements.
#
# Copilot generated:
#   SELECT * FROM transactions
#   WHERE CAST(amount AS REAL) >= 100000.00
#   ORDER BY amount DESC;

class TestHighValueTransactions:
    """Tests for regulatory-threshold transaction detection."""

    def test_high_value_tx_exists(self, db_path: str) -> None:
        high_value = find_transactions(db_path, min_amount="100000.00")
        assert len(high_value) >= 1

    def test_high_value_tx_amount_is_precise(self, db_path: str) -> None:
        high_value = find_transactions(db_path, min_amount="100000.00")
        for tx in high_value:
            amount = Decimal(tx["amount"])
            assert amount == amount.quantize(Decimal("0.01")), \
                "Financial amounts must have exactly 2 decimal places"

    def test_high_value_tx_is_completed(self, db_path: str) -> None:
        high_value = find_transactions(db_path, min_amount="100000.00")
        completed = [tx for tx in high_value if tx["status"] == "completed"]
        assert len(completed) >= 1


# =====================================================================
# Scenario 4 — Cross-border / multi-currency for FX tests
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql Find me a customer with a non-USD account so I can
#   test cross-border payment logic.
#
# Copilot generated:
#   SELECT c.*, a.currency, a.account_id
#   FROM customers c
#   JOIN accounts a ON c.customer_id = a.customer_id
#   WHERE a.currency != 'USD';

class TestCrossBorderScenarios:
    """Tests using cross-border data found via natural language."""

    def test_non_usd_account_exists(self, db_path: str) -> None:
        rows = run_select(
            db_path,
            "SELECT * FROM accounts WHERE currency != 'USD'",
        )
        assert len(rows) >= 1

    def test_cross_border_tx_has_counterparty(self, db_path: str) -> None:
        # Prompt: Show me international wire transactions
        rows = run_select(
            db_path,
            """
            SELECT * FROM transactions
            WHERE description LIKE '%international%'
              OR description LIKE '%wire%'
            """,
        )
        assert len(rows) >= 1
        for row in rows:
            assert row["counterparty"], "Cross-border txs must have a counterparty"


# =====================================================================
# Scenario 5 — Dormant account for lifecycle tests
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql I need a dormant account to test our reactivation flow.
#
# Copilot generated:
#   SELECT a.*, c.name FROM accounts a
#   JOIN customers c ON a.customer_id = c.customer_id
#   WHERE a.status = 'dormant';

class TestDormantAccountScenarios:
    """Tests using dormant accounts located via Copilot CLI."""

    def test_dormant_account_exists(self, db_path: str) -> None:
        rows = run_select(
            db_path,
            "SELECT * FROM accounts WHERE status = 'dormant'",
        )
        assert len(rows) >= 1

    def test_dormant_account_has_fee(self, db_path: str) -> None:
        # Prompt: Was there a fee charged on the dormant account?
        rows = run_select(
            db_path,
            """
            SELECT t.* FROM transactions t
            JOIN accounts a ON t.account_id = a.account_id
            WHERE a.status = 'dormant' AND t.tx_type = 'fee'
            """,
        )
        assert len(rows) >= 1
        assert Decimal(rows[0]["amount"]) > 0


# =====================================================================
# Scenario 6 — Reversed transaction for chargeback testing
# =====================================================================
# Copilot CLI prompt:
#   @schema.sql Find reversed transactions for chargeback test cases
#
# Copilot generated:
#   SELECT * FROM transactions WHERE status = 'reversed';

class TestChargebackScenarios:
    """Tests using reversed transactions for chargeback workflows."""

    def test_reversed_tx_exists(self, db_path: str) -> None:
        rows = run_select(
            db_path,
            "SELECT * FROM transactions WHERE status = 'reversed'",
        )
        assert len(rows) >= 1

    def test_reversed_tx_description_mentions_reason(self, db_path: str) -> None:
        rows = run_select(
            db_path,
            "SELECT * FROM transactions WHERE status = 'reversed'",
        )
        for row in rows:
            desc = row["description"].lower()
            assert any(word in desc for word in ["chargeback", "unauthorized", "dispute"]), \
                "Reversed transactions should document the reason"


# =====================================================================
# Safety: query_helper rejects non-SELECT queries
# =====================================================================
class TestQuerySafety:
    """Ensure the query helper only allows read-only access."""

    def test_rejects_insert(self, db_path: str) -> None:
        with pytest.raises(ValueError, match="Only SELECT"):
            run_select(db_path, "INSERT INTO customers VALUES ('X','X','X','vip','US',1,'now')")

    def test_rejects_delete(self, db_path: str) -> None:
        with pytest.raises(ValueError, match="Only SELECT"):
            run_select(db_path, "DELETE FROM customers")

    def test_rejects_drop(self, db_path: str) -> None:
        with pytest.raises(ValueError, match="Only SELECT"):
            run_select(db_path, "DROP TABLE customers")
