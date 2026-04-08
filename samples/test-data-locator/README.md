# Natural-Language Test Data Discovery

> **Use Case:** Use Copilot CLI to locate required test data in plain English — no manual SQL needed.

---

## The Problem

In enterprise financial systems, test databases contain thousands of rows across dozens of tables. QA engineers waste hours writing SQL to find the right test data:

```
❌  "I need a customer with a frozen account and a failed transaction..."
     → manually joins 3 tables
     → gets the WHERE clause wrong twice
     → 20 minutes lost before writing the actual test
```

## The Solution

Give Copilot CLI your **schema as context**, then **ask in plain English**:

```
✅  @schema.sql Find me a customer with a frozen account and a failed transaction
     → Copilot generates the SQL instantly
     → you review it (read-only SELECT)
     → paste into your test
     → done in 30 seconds
```

---

## The Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    DEVELOPER / QA ENGINEER                   │
│                                                             │
│  "I need a failed transaction on a frozen account           │
│   to test our error recovery flow"                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                     COPILOT CLI                             │
│                                                             │
│  Input:  @schema.sql + plain-English question               │
│  Output: SELECT query tailored to your schema               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                  👀 HUMAN REVIEW                            │
│                                                             │
│  • Is it a SELECT? (never run writes against test data)     │
│  • Does the JOIN logic look correct?                        │
│  • Are the WHERE filters what I intended?                   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   TEST DATABASE                             │
│                                                             │
│  SQLite / PostgreSQL / SQL Server with seeded test data     │
│  ┌────────────┐ ┌──────────┐ ┌──────────────┐              │
│  │ customers  │ │ accounts │ │ transactions │              │
│  └────────────┘ └──────────┘ └──────────────┘              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   YOUR TEST CODE                            │
│                                                             │
│  def test_frozen_account_rejects_payment(db):               │
│      rows = run_select(db, "<SQL from Copilot>")            │
│      assert rows[0]["account_status"] == "frozen"           │
│      assert rows[0]["tx_status"] == "failed"                │
└─────────────────────────────────────────────────────────────┘
```

---

## 5-Step Walkthrough

### Step 1: Seed Your Test Database

```bash
cd samples/test-data-locator
python seed_test_data.py
# ✅ Seeded test_data.db with 4 customers, 7 accounts, 9 transactions
```

### Step 2: Ask Copilot CLI in Plain English

Launch Copilot CLI and reference the schema so it understands your tables:

```bash
copilot

> @schema.sql I need a failed transaction to test our error recovery.
> Find all failed transactions with the account and customer details.
```

### Step 3: Review the Generated SQL

Copilot responds with something like:

```sql
SELECT t.*, a.status AS account_status, c.name AS customer_name
FROM transactions t
JOIN accounts a ON t.account_id = a.account_id
JOIN customers c ON a.customer_id = c.customer_id
WHERE t.status = 'failed';
```

**Before running — check:**
- ✅ Is it a `SELECT`? (never accept `INSERT`, `UPDATE`, `DELETE`)
- ✅ Do the JOINs make sense?
- ✅ Does the WHERE match what I asked for?

### Step 4: Run It

```bash
> Run this query against test_data.db and show me the results
```

Or use the helper in Python:

```python
from query_helper import run_select

rows = run_select("test_data.db", """
    SELECT t.*, a.status AS account_status
    FROM transactions t
    JOIN accounts a ON t.account_id = a.account_id
    WHERE t.status = 'failed'
""")
print(rows)
```

### Step 5: Use the Data in Your Test

```python
class TestFailedTransactions:
    def test_failed_tx_has_description(self, db_path):
        failed = find_transactions(db_path, status="failed")
        for tx in failed:
            assert tx["description"], "Every failed tx should explain why"
```

---

## Example Prompts → SQL

Here are real prompts you can try with `@schema.sql`:

| Plain English Prompt | What Copilot Generates |
|---|---|
| "Find a VIP customer in the US who passed KYC" | `SELECT * FROM customers WHERE tier='vip' AND country_code='US' AND kyc_verified=1` |
| "I need a dormant account to test reactivation" | `SELECT * FROM accounts WHERE status='dormant'` |
| "Show transactions over $100K for regulatory tests" | `SELECT * FROM transactions WHERE CAST(amount AS REAL) >= 100000` |
| "Find reversed transactions for chargeback testing" | `SELECT * FROM transactions WHERE status='reversed'` |
| "Customer with a non-USD account for FX tests" | `SELECT c.*, a.currency FROM customers c JOIN accounts a ON ... WHERE a.currency != 'USD'` |
| "Was a fee charged on the dormant account?" | `SELECT t.* FROM transactions t JOIN accounts a ON ... WHERE a.status='dormant' AND t.tx_type='fee'` |

---

## Run the Tests

```bash
cd samples/test-data-locator
python -m pytest tests/ -v
```

All tests use `tmp_path` fixtures — each test gets a fresh database, no cleanup needed.

---

## Project Structure

```
test-data-locator/
├── schema.sql              ← Reference with @schema.sql in Copilot CLI
├── seed_test_data.py       ← Creates and populates the test database
├── query_helper.py         ← Thin read-only query runner
├── README.md               ← This file (the full flow)
└── tests/
    ├── conftest.py          ← Fixtures: fresh DB per test via tmp_path
    └── test_payment_scenarios.py  ← 6 scenarios, each discovered via NL
```

---

## Advanced: Direct DB Access with MCP (Optional)

For teams that want Copilot CLI to query the database **directly** (no
copy-paste of SQL), you can configure an MCP server that exposes the test
database. This lets you ask questions and get **results** back, not just SQL.

Add to your `.mcp.json`:

```json
{
  "mcpServers": {
    "test-data": {
      "command": "npx",
      "args": ["-y", "@anthropic/sqlite-mcp", "test_data.db"]
    }
  }
}
```

Then in Copilot CLI:

```bash
> How many failed transactions are in the test database?
# Copilot calls the MCP tool, queries the DB, and returns:
# "There are 2 failed transactions: T003 (wire transfer rejected)
#  and T009 (payment blocked on frozen account)."
```

> **Note:** This requires installing and running an MCP server.
> See [Chapter 06: MCP Servers](../../06-mcp-servers/README.md) for setup details.

---

## Key Principles

1. **Always give Copilot your schema** — use `@schema.sql` so it knows your tables
2. **Review before running** — verify it's a SELECT and the logic matches your intent
3. **Read-only access only** — the `query_helper.py` rejects anything that isn't a SELECT
4. **Isolate tests** — use `tmp_path` fixtures; never point tests at shared databases
5. **Document the prompt** — add the Copilot CLI prompt as a comment above each query so teammates understand where the SQL came from
