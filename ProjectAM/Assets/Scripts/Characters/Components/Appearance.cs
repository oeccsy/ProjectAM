using System.Collections.Generic;
using UnityEngine;

public class Appearance : MonoBehaviour
{
    private static readonly HashSet<string> ColorableSlots = new HashSet<string> { "Hat", "Shirt", "Dress" };

    private SkinnedMeshRenderer[] skinnedRenderers;

    private void Awake()
    {
        skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void ApplyColor(NpcColor color)
    {
        Material colorMaterial = Resources.Load<Material>($"Materials/{color}");
        if (colorMaterial == null)
        {
            Debug.LogWarning($"Color material not found : {color}");
            return;
        }

        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
        {
            Material[] materials = skinnedRenderer.sharedMaterials;
            bool replaced = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;
                if (!ColorableSlots.Contains(materials[i].name)) continue;

                materials[i] = colorMaterial;
                replaced = true;
            }

            if (replaced)
            {
                skinnedRenderer.sharedMaterials = materials;
            }
        }
    }
}
