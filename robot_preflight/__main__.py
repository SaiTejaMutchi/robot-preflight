"""Robot Preflight CLI.

    python -m robot_preflight check examples/otto1500_warehouse
    robot-preflight check examples/otto1500_warehouse/config.yaml

Prints the structured preflight result. Exits 0 on PASS, 1 on BLOCKED or
REVIEW, 2 on error -- so this is usable as a CI gate, not just a demo.
"""
from __future__ import annotations

import argparse
import json
import pathlib
import sys

import yaml

from .core import REPO_ROOT, Preflight


def _cmd_check(args: argparse.Namespace) -> int:
    facility_path = args.facility
    if facility_path.endswith(("config.yaml", "config.yml")):
        facility_dir = pathlib.Path(facility_path).resolve().parent
    else:
        facility_dir = pathlib.Path(facility_path)
        if not facility_dir.is_absolute():
            facility_dir = (REPO_ROOT / facility_dir).resolve()

    config_path = facility_dir / "config.yaml"
    if not config_path.exists():
        print(f"error: no config.yaml found at {config_path}", file=sys.stderr)
        return 2
    config = yaml.safe_load(config_path.read_text())

    try:
        result = Preflight.check(
            robot=config["robot"]["id"],
            facility=str(facility_dir),
            constraint=config["constraint"]["type"],
        )
    except Exception as exc:  # noqa: BLE001 -- CLI boundary, report and exit
        print(f"error: {exc}", file=sys.stderr)
        return 2

    if args.json:
        print(json.dumps(result.to_dict(), indent=2))
    else:
        print(result)

    return {"PASS": 0, "BLOCKED": 1, "REVIEW": 1}.get(result.decision, 2)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="robot-preflight")
    sub = parser.add_subparsers(dest="command", required=True)

    check = sub.add_parser("check", help="run a robot/facility compatibility check")
    check.add_argument("facility", help="path to an example directory or its config.yaml")
    check.add_argument("--json", action="store_true", help="print the structured JSON result")
    check.set_defaults(func=_cmd_check)

    args = parser.parse_args(argv)
    return args.func(args)


if __name__ == "__main__":
    raise SystemExit(main())
