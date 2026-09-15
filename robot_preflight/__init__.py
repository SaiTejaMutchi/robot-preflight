"""Robot Preflight: verify robot requirements against facility geometry.

Public surface:

    from robot_preflight import Preflight

    result = Preflight.check(
        robot="otto_1500",
        facility="examples/otto1500_warehouse",
        constraint="aisle_clearance",
    )
    print(result)

See README.md for the verified reference result and its evidence chain.
"""
from .core import Preflight, PreflightResult, check

__all__ = ["Preflight", "PreflightResult", "check"]
__version__ = "0.1.0"
