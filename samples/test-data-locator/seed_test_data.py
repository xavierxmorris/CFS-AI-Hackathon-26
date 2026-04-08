"""Seed a SQLite database with realistic financial test scenarios.

Usage:
    python seed_test_data.py              # creates test_data.db in current dir
    python seed_test_data.py my_tests.db  # custom path

Each scenario is labelled so Copilot CLI can help you find the right
test data using plain English instead of writing SQL by hand.
"""

import sqlite3
import sys
from pathlib import Path

SCHEMA_FILE = Path(__file__).parent / "schema.sql"

# ---------------------------------------------------------------------------
# Test scenarios — each tuple documents *why* this row exists
# ---------------------------------------------------------------------------

CUSTOMERS = [
    # (customer_id, name, email, tier, country_code, kyc_verified, created_at)
    ("C001", "Alice Johnson", "alice@example.com", "vip", "US", 1, "2023-01-15T10:00:00Z"),
    ("C002", "Bob Smith", "bob@example.com", "standard", "US", 1, "2023-06-20T14:30:00Z"),
    ("C003", "Carlos García", "carlos@example.com", "premium", "MX", 1, "2022-11-01T09:00:00Z"),
    ("C004", "Diana Müller", "diana@example.com", "standard", "DE", 0, "2024-01-10T08:00:00Z"),
]

ACCOUNTS = [
    # (account_id, customer_id, account_type, currency, balance, status, opened_at)
    # Alice — VIP with multiple accounts, one high-value
    ("A001", "C001", "checking", "USD", "150000.00", "active", "2023-01-15T10:00:00Z"),
    ("A002", "C001", "savings", "USD", "500000.00", "active", "2023-01-15T10:05:00Z"),
    ("A003", "C001", "investment", "USD", "1250000.00", "active", "2023-02-01T12:00:00Z"),
    # Bob — standard customer, one dormant account
    ("A004", "C002", "checking", "USD", "2500.75", "active", "2023-06-20T14:30:00Z"),
    ("A005", "C002", "savings", "USD", "100.00", "dormant", "2023-06-20T14:35:00Z"),
    # Carlos — cross-border, MXN currency
    ("A006", "C003", "checking", "MXN", "85000.50", "active", "2022-11-01T09:00:00Z"),
    # Diana — unverified KYC, frozen account
    ("A007", "C004", "checking", "EUR", "3200.00", "frozen", "2024-01-10T08:00:00Z"),
]

TRANSACTIONS = [
    # (transaction_id, account_id, tx_type, amount, currency, status, description, counterparty, created_at)
    # Normal completed transactions
    ("T001", "A001", "credit", "5000.00", "USD", "completed", "Payroll deposit", None, "2024-03-01T09:00:00Z"),
    ("T002", "A001", "debit", "150.50", "USD", "completed", "Utility payment", "UTIL-CO-123", "2024-03-02T11:00:00Z"),
    # Failed transaction — insufficient funds scenario
    ("T003", "A004", "debit", "10000.00", "USD", "failed", "Wire transfer rejected", "EXT-9999", "2024-03-05T14:00:00Z"),
    # High-value transaction — triggers regulatory review
    ("T004", "A002", "transfer", "250000.00", "USD", "completed", "Investment transfer", "A003", "2024-03-10T10:00:00Z"),
    # Cross-border transaction
    ("T005", "A006", "credit", "50000.00", "MXN", "completed", "International wire", "US-BANK-REF", "2024-03-12T16:00:00Z"),
    # Pending transaction
    ("T006", "A001", "debit", "999.99", "USD", "pending", "Online purchase", "MERCHANT-456", "2024-03-15T20:00:00Z"),
    # Reversed transaction — chargeback scenario
    ("T007", "A004", "debit", "75.00", "USD", "reversed", "Chargeback - unauthorized", "MERCHANT-789", "2024-03-18T13:00:00Z"),
    # Fee transaction
    ("T008", "A005", "fee", "25.00", "USD", "completed", "Dormant account fee", None, "2024-03-20T00:00:00Z"),
    # Transaction on frozen account
    ("T009", "A007", "debit", "500.00", "EUR", "failed", "Payment blocked - account frozen", "SHOP-EU-01", "2024-03-22T10:30:00Z"),
]


def create_database(db_path: str) -> None:
    """Create the schema and seed all test data."""
    schema_sql = SCHEMA_FILE.read_text(encoding="utf-8")

    conn = sqlite3.connect(db_path)
    try:
        conn.executescript(schema_sql)

        conn.executemany(
            "INSERT OR REPLACE INTO customers VALUES (?, ?, ?, ?, ?, ?, ?)",
            CUSTOMERS,
        )
        conn.executemany(
            "INSERT OR REPLACE INTO accounts VALUES (?, ?, ?, ?, ?, ?, ?)",
            ACCOUNTS,
        )
        conn.executemany(
            "INSERT OR REPLACE INTO transactions VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)",
            TRANSACTIONS,
        )
        conn.commit()
        print(f"✅ Seeded {db_path} with {len(CUSTOMERS)} customers, "
              f"{len(ACCOUNTS)} accounts, {len(TRANSACTIONS)} transactions")
    finally:
        conn.close()


if __name__ == "__main__":
    target = sys.argv[1] if len(sys.argv) > 1 else "test_data.db"
    create_database(target)
