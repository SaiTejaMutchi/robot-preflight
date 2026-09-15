using NUnit.Framework;
using SpatialGrid.WarehouseGeometry;
using UnityEngine;

public class WarehouseEntityRegistryTests
{
    GameObject root;

    [SetUp] public void SetUp() => root = new GameObject("Root");
    [TearDown] public void TearDown() => Object.DestroyImmediate(root);

    GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        return cube;
    }

    [Test]
    public void ExactMatch_ReturnsStableTransform()
    {
        var expected = Cube("Rack_Left", root.transform, Vector3.zero, Vector3.one).transform;
        var registry = new WarehouseEntityRegistry(root.transform);
        Assert.That(registry.TryResolveUnique("Rack_Left", out var first), Is.True);
        Assert.That(registry.TryResolveUnique("Rack_Left", out var second), Is.True);
        Assert.That(first, Is.SameAs(expected));
        Assert.That(second, Is.SameAs(expected));
    }

    [Test]
    public void DuplicateName_IsRejected()
    {
        Cube("Rack_Left", root.transform, Vector3.left, Vector3.one);
        Cube("Rack_Left", root.transform, Vector3.right, Vector3.one);
        var registry = new WarehouseEntityRegistry(root.transform);
        Assert.That(registry.ContainsDuplicate("Rack_Left"), Is.True);
        Assert.That(registry.TryResolveUnique("Rack_Left", out _), Is.False);
    }

    [Test] public void MissingNode_IsRejected() => Assert.That(new WarehouseEntityRegistry(root.transform).TryResolveUnique("Missing", out _), Is.False);
    [Test]
    public void ZeroGeometry_ReturnsNoBounds()
    {
        var empty = new GameObject("Empty");
        empty.transform.SetParent(root.transform);
        Assert.That(WarehouseEntityRegistry.TryGetRendererBounds(empty.transform, out _), Is.False);
    }

    [Test]
    public void WorldBounds_PreserveTranslatedRotatedScaledHierarchy()
    {
        root.transform.position = new Vector3(7, 2, -4);
        root.transform.rotation = Quaternion.Euler(0, 90, 0);
        root.transform.localScale = Vector3.one * 2;
        var left = Cube("Rack_Left", root.transform, new Vector3(0, .5f, -.77f), new Vector3(1.6f,1,.6f));
        var right = Cube("Rack_Right", root.transform, new Vector3(0, .5f,.77f), new Vector3(1.6f,1,.6f));
        Assert.That(WarehouseEntityRegistry.TryGetRendererBounds(left.transform, out var a), Is.True);
        Assert.That(WarehouseEntityRegistry.TryGetRendererBounds(right.transform, out var b), Is.True);
        var result = AisleClearanceChecker.Check(a,b,ClearanceAxis.X,3.830f);
        Assert.That(result.availableMeters, Is.EqualTo(1.880f).Within(.005f));
        Assert.That(result.status, Is.EqualTo(ClearanceStatus.Blocked));
    }
}
