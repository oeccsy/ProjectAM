using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private const string RoofObjectName = "Cabin_Roof";

    public NpcColor owner;
    public Vector2Int anchor;
    public Vector2Int origin;
    public Vector2Int size;
    public Vector2Int entrance;

    public List<NpcColor> guests;

    public void ApplyRoofColor(NpcColor color)
    {
        Material colorMaterial = Resources.Load<Material>($"Materials/{color}");
        if (colorMaterial == null)
        {
            Debug.LogWarning($"Roof color material not found : {color}");
            return;
        }

        Renderer roofRenderer = null;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name != RoofObjectName) continue;

            roofRenderer = child.GetComponent<Renderer>();
            break;
        }

        if (roofRenderer == null)
        {
            Debug.LogWarning($"Roof object not found : {RoofObjectName}");
            return;
        }

        roofRenderer.sharedMaterial = colorMaterial;
    }
}
