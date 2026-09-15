using UnityEngine;

public class GetMainParentOnRaycast : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject childObject = hit.collider.gameObject;
                GameObject mainParent = GetTopMostParent(childObject);

                if (mainParent != null)
                {
                    Debug.Log("Main Parent: " + mainParent.name);
                }
                else
                {
                    Debug.Log("No Main Parent found.");
                }
            }
        }
    }

    GameObject GetTopMostParent(GameObject childObject)
    {
        Transform parent = childObject.transform.parent;

        while (parent != null)
        {
            childObject = parent.gameObject;
            parent = childObject.transform.parent;
        }

        return childObject;
    }
}
