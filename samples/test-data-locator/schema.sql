-- =============================================================
-- Financial Test Data Schema
-- Reference this file with: @schema.sql in Copilot CLI
-- so Copilot understands your database structure.
-- =============================================================

CREATE TABLE IF NOT EXISTS customers (
    customer_id     TEXT PRIMARY KEY,
    name            TEXT NOT NULL,
    email           TEXT NOT NULL UNIQUE,
    tier            TEXT NOT NULL CHECK (tier IN ('standard', 'premium', 'vip')),
    country_code    TEXT NOT NULL,       -- ISO 3166-1 alpha-2
    kyc_verified    INTEGER NOT NULL DEFAULT 0,  -- 0 = false, 1 = true
    created_at      TEXT NOT NULL         -- ISO 8601
);

CREATE TABLE IF NOT EXISTS accounts (
    account_id      TEXT PRIMARY KEY,
    customer_id     TEXT NOT NULL REFERENCES customers(customer_id),
    account_type    TEXT NOT NULL CHECK (account_type IN ('checking', 'savings', 'investment')),
    currency        TEXT NOT NULL DEFAULT 'USD',  -- ISO 4217
    balance         TEXT NOT NULL DEFAULT '0.00',  -- stored as TEXT for Decimal precision
    status          TEXT NOT NULL CHECK (status IN ('active', 'dormant', 'frozen', 'closed')),
    opened_at       TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS transactions (
    transaction_id  TEXT PRIMARY KEY,
    account_id      TEXT NOT NULL REFERENCES accounts(account_id),
    tx_type         TEXT NOT NULL CHECK (tx_type IN ('credit', 'debit', 'transfer', 'fee')),
    amount          TEXT NOT NULL,         -- Decimal as TEXT, always positive
    currency        TEXT NOT NULL DEFAULT 'USD',
    status          TEXT NOT NULL CHECK (status IN ('completed', 'pending', 'failed', 'reversed')),
    description     TEXT,
    counterparty    TEXT,                  -- destination account or external ref
    created_at      TEXT NOT NULL
);
