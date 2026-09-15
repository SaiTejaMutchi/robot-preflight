using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public static class AisleClearanceChecker
    {
        public static ClearanceResult Check(Bounds a, Bounds b, ClearanceAxis axis, float requiredMeters, float reviewToleranceMeters = .005f)
        {
            if (requiredMeters <= 0) throw new System.ArgumentOutOfRangeException(nameof(requiredMeters));
            float aMin = axis == ClearanceAxis.X ? a.min.x : a.min.z;
            float aMax = axis == ClearanceAxis.X ? a.max.x : a.max.z;
            float bMin = axis == ClearanceAxis.X ? b.min.x : b.min.z;
            float bMax = axis == ClearanceAxis.X ? b.max.x : b.max.z;
            float available = a.center[(axis == ClearanceAxis.X) ? 0 : 2] <= b.center[(axis == ClearanceAxis.X) ? 0 : 2]
                ? Mathf.Max(0, bMin - aMax) : Mathf.Max(0, aMin - bMax);
            float difference = available - requiredMeters;
            var status = Mathf.Abs(difference) <= reviewToleranceMeters ? ClearanceStatus.Review
                : difference < 0 ? ClearanceStatus.Blocked : ClearanceStatus.Pass;
            return new ClearanceResult { requiredMeters=requiredMeters, availableMeters=available, differenceMeters=difference, status=status };
        }
    }
}
