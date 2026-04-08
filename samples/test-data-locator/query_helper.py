"""Thin helper for running read-only queries against the test data store.

This module is intentionally minimal — the point is that Copilot CLI
generates the SQL for you. These helpers just execute it safely.
"""

import sqlite3
from decimal import Decimal
from typing import Any, Dict, List, Optional


def run_select(db_path: str, sql: str, params: tuple = ()) -> List[Dict[str, Any]]:
    """Execute a read-only SELECT and return rows as dicts.

    Args:
        db_path: Path to the SQLite database file.
        sql: A SELECT statement (writes are rejected).
        params: Bind parameters for the query.

    Returns:
        A list of dicts, one per row, with column names as keys.

    Raises:
        ValueError: If the SQL is not a SELECT statement.

    Example:
        >>> rows = run_select("test_data.db", "SELECT * FROM customers WHERE tier = ?", ("vip",))
        >>> rows[0]["name"]
        'Alice Johnson'
    """
    normalized = sql.strip().upper()
    if not normalized.startswith("SELECT"):
        raise ValueError("Only SELECT queries are allowed for test data lookup")

    conn = sqlite3.connect(db_path)
    conn.row_factory = sqlite3.Row
    try:
        cursor = conn.execute(sql, params)
        return [dict(row) for row in cursor.fetchall()]
    finally:
        conn.close()


def find_customer(db_path: str, **filters: Any) -> List[Dict[str, Any]]:
    """Find customers matching keyword filters.

    Args:
        db_path: Path to the SQLite database file.
        **filters: Column-value pairs (e.g., tier="vip", country_code="US").

    Returns:
        Matching customer rows as dicts.

    Example:
        >>> find_customer("test_data.db", tier="vip")
        [{'customer_id': 'C001', 'name': 'Alice Johnson', ...}]
    """
    clauses = [f"{col} = ?" for col in filters]
    where = " AND ".join(clauses) if clauses else "1=1"
    sql = f"SELECT * FROM customers WHERE {where}"
    return run_select(db_path, sql, tuple(filters.values()))


def find_transactions(
    db_path: str,
    status: Optional[str] = None,
    min_amount: Optional[str] = None,
) -> List[Dict[str, Any]]:
    """Find transactions with optional status and minimum amount filters.

    Args:
        db_path: Path to the SQLite database file.
        status: Filter by transaction status (e.g., "failed", "reversed").
        min_amount: Minimum amount as a Decimal string (e.g., "10000.00").

    Returns:
        Matching transaction rows as dicts.

    Example:
        >>> failed = find_transactions("test_data.db", status="failed")
        >>> len(failed)
        2
    """
    clauses: list[str] = []
    params: list[Any] = []

    if status:
        clauses.append("status = ?")
        params.append(status)
    if min_amount:
        clauses.append("CAST(amount AS REAL) >= ?")
        params.append(float(Decimal(min_amount)))

    where = " AND ".join(clauses) if clauses else "1=1"
    sql = f"SELECT * FROM transactions WHERE {where} ORDER BY created_at"
    return run_select(db_path, sql, tuple(params))
