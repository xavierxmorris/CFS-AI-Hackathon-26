"""Pytest fixtures that seed a temporary test database for every test.

Uses tmp_path so each test gets its own isolated DB — no cross-test
contamination, no cleanup needed.
"""

import sys
from pathlib import Path

import pytest

# Ensure the sample directory is importable
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from seed_test_data import create_database  # noqa: E402


@pytest.fixture()
def db_path(tmp_path: Path) -> str:
    """Create a fresh seeded test database and return its path."""
    db_file = tmp_path / "test_data.db"
    create_database(str(db_file))
    return str(db_file)
