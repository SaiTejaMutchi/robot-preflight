using NUnit.Framework;
using SpatialGrid.WarehouseGeometry;
using UnityEngine;

public class WarehouseHighlightControllerTests
{
    GameObject cube;
    Material material;

    [SetUp]
    public void SetUp()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { color = Color.blue };
        cube.GetComponent<Renderer>().sharedMaterial = material;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cube);
        Object.DestroyImmediate(material);
    }

    [Test]
    public void HighlightAndClear_DoNotMutateSourceMaterial()
    {
        var controller = new WarehouseHighlightController();
        var original = material.color;
        controller.Highlight(cube.transform, Color.red);
        Assert.That(controller.IsHighlighted, Is.True);
        Assert.That(material.color, Is.EqualTo(original));
        controller.Clear();
        Assert.That(controller.IsHighlighted, Is.False);
        Assert.That(material.color, Is.EqualTo(original));
    }

    [Test]
    public void ClearWithoutHighlight_IsSafe()
    {
        var controller = new WarehouseHighlightController();
        Assert.DoesNotThrow(controller.Clear);
        Assert.That(controller.IsHighlighted, Is.False);
    }
}
