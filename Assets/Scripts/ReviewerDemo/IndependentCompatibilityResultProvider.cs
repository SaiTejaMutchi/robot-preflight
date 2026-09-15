using System;
using SpatialGrid.WarehouseGeometry;

namespace SpatialGrid.ReviewerDemo
{
    // UI-002: one reload-safe source of truth for the reviewer demo.
    // Provenance constants are frozen (OTTO requirement, AWS environment,
    // preregistered boundaries). Measured values are NEVER invented here —
    // Bind() copies them from a real AisleClearanceChecker ClearanceResult
    // and then validates that the frozen fields still match GEO-003.
    public sealed class IndependentCompatibilityResultProvider
    {
        public const string GlbStreamingPath = "WarehouseGeometry/aws_independent_warehouse.glb";
        public const string BoundaryAFullName = "aws_robomaker_warehouse_ShelfF_01_001";
        public const string BoundaryBFullName = "aws_robomaker_warehouse_ShelfD_01_001";
        public const string BoundaryPrefix = "aws_robomaker_warehouse_";
        public const float RequiredMeters = 1.915f;
        public const float FrozenAvailableMeters = 7.509663f;
        public const float FrozenDifferenceMeters = 5.594663f;
        public const float ValueToleranceMeters = 0.0005f;

        public const string RobotManufacturer = "OTTO Motors by Rockwell Automation";
        public const string RobotModel = "OTTO 1500";
        public const string CheckName = "Geometry preflight — one-way aisle-width check";
        public const string ConstraintId = "OTTO-1500-MIN-AISLE-WIDTH-ONE-WAY";
        public const string MachineSourceDocument = "OTTO 1500 Spec Sheet";
        public const int MachineSourcePage = 1;
        public const string MachineSourceText = "Min. Aisle Width 1915 mm (78 in) (One Way)";
        public const string EnvironmentSource = "AWS RoboMaker Small Warehouse World";
        public const string SourceRevision = "3c23a698bf0b4e366ddf8b084af507c519bd3483";
        // ENV-003 deployed GLB (measurement-identical to GEO-003).
        public const string SceneFileHashSha256 = "7a4a6539f7eaadfa8c3a268485c45ed64ebd677c6d5c92d1d26738738e2e27c4";
        public const string MeasurementMethod = "AisleClearanceChecker.Check, axis X, Unity Play Mode glTFast import";

        public static string SourceDisplayLine => $"{MachineSourceDocument} · p.{MachineSourcePage}";

        public CompatibilityResult? Current { get; private set; }
        public bool HasResult => Current.HasValue;

        public static string ShortBoundaryDisplay(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return fullName;
            return fullName.StartsWith(BoundaryPrefix, StringComparison.Ordinal)
                ? fullName.Substring(BoundaryPrefix.Length)
                : fullName;
        }

        public void Clear()
        {
            Current = null;
        }

        public CompatibilityResult Bind(ClearanceResult clearance)
        {
            if (Math.Abs(clearance.requiredMeters - RequiredMeters) > ValueToleranceMeters)
                throw new InvalidOperationException(
                    $"Provider refused to bind: required {clearance.requiredMeters} != frozen {RequiredMeters}");

            string boundaryA = string.IsNullOrEmpty(clearance.leftEntityId) ? BoundaryAFullName : clearance.leftEntityId;
            string boundaryB = string.IsNullOrEmpty(clearance.rightEntityId) ? BoundaryBFullName : clearance.rightEntityId;
            if (boundaryA != BoundaryAFullName || boundaryB != BoundaryBFullName)
                throw new InvalidOperationException(
                    $"Provider refused to bind: boundaries {boundaryA}/{boundaryB} are not the frozen pair");

            clearance.leftEntityId = BoundaryAFullName;
            clearance.rightEntityId = BoundaryBFullName;

            var result = CompatibilityResult.FromClearance(
                clearance,
                runId: $"ui002-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                checkName: CheckName,
                constraintId: ConstraintId,
                machineSourceDocument: MachineSourceDocument,
                machineSourcePage: MachineSourcePage,
                machineSourceText: MachineSourceText,
                environmentSource: EnvironmentSource,
                sourceRevision: SourceRevision,
                sceneFileHash: SceneFileHashSha256,
                measurementMethod: MeasurementMethod);

            Validate(result);
            Current = result;
            return result;
        }

        public static void Validate(CompatibilityResult result)
        {
            if (result.robotModel != RobotModel)
                throw new InvalidOperationException($"robotModel '{result.robotModel}' is not {RobotModel}");
            if (string.IsNullOrEmpty(result.robotManufacturer))
                throw new InvalidOperationException("robotManufacturer missing");
            if (Math.Abs(result.requiredValue - RequiredMeters) > ValueToleranceMeters)
                throw new InvalidOperationException("requiredValue drifted from frozen OTTO 1.915 m");
            if (result.unit != "m")
                throw new InvalidOperationException("unit must be m");
            if (string.IsNullOrEmpty(result.checkName) || string.IsNullOrEmpty(result.constraintId))
                throw new InvalidOperationException("check identity missing");
            if (string.IsNullOrEmpty(result.machineSourceDocument) || result.machineSourcePage < 1)
                throw new InvalidOperationException("machine source missing");
            if (string.IsNullOrEmpty(result.environmentSource) || string.IsNullOrEmpty(result.sourceRevision))
                throw new InvalidOperationException("environment provenance missing");
            if (result.boundaryA != BoundaryAFullName || result.boundaryB != BoundaryBFullName)
                throw new InvalidOperationException("boundary IDs are not the frozen pair");
            if (string.IsNullOrEmpty(result.measurementMethod))
                throw new InvalidOperationException("measurementMethod missing");
            if (string.IsNullOrEmpty(result.schemaVersion) || string.IsNullOrEmpty(result.runId))
                throw new InvalidOperationException("schemaVersion/runId missing");
        }

        public static bool MatchesFrozenIndependentMeasurement(CompatibilityResult result)
        {
            return Math.Abs(result.requiredValue - RequiredMeters) <= ValueToleranceMeters
                && Math.Abs(result.measuredValue - FrozenAvailableMeters) <= ValueToleranceMeters
                && Math.Abs(result.signedDifference - FrozenDifferenceMeters) <= ValueToleranceMeters
                && result.status == ClearanceStatus.Pass
                && result.robotModel == RobotModel
                && result.boundaryA == BoundaryAFullName
                && result.boundaryB == BoundaryBFullName;
        }
    }
}
