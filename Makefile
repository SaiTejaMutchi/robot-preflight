.PHONY: install verify test

install:
	pip install -e .

verify:
	python3 scripts/verify_reference_result.py

test:
	python3 -m pytest tests/ -v
