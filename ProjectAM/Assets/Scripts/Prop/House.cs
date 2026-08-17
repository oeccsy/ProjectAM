using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private const string RoofObjectName = "Cabin_Roof";

    public NpcColor owner;
    public Vector2Int anchor;
    public Vector2 origin;
    public Vector2Int size;
    public List<Vector2Int> entrances;

    public List<NpcColor> guests;

    [Header("창문 불빛")]
    private WindowRenderer windowRenderer;
    private LightDecal lightDecal;

    [SerializeField]
    private float decalSize = 5f;
    [SerializeField]
    private Vector3 decalLocalOffset = new Vector3(-1.5f, 0f, 0f);

    private void Awake()
    {
        windowRenderer = GetComponent<WindowRenderer>();
    }

    public List<Vector2Int> GetApproachTiles()
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        foreach (Vector2Int entrance in entrances)
        {
            tiles.Add(anchor + entrance);
        }

        return tiles;
    }

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

    public void TurnOnLight()
    {
        if (windowRenderer == null) return;
        if (lightDecal == null)
        {
            Vector3 decalCenter = transform.TransformPoint(decalLocalOffset);
            lightDecal = LightDecal.Create(transform, decalCenter, decalSize);
        }

        windowRenderer.StartGlow();
        lightDecal.StartGlow();
    }

    public void TurnOffLight()
    {
        if (windowRenderer == null) return;
        if (lightDecal == null)
        {
            Vector3 decalCenter = transform.TransformPoint(decalLocalOffset);
            lightDecal = LightDecal.Create(transform, decalCenter, decalSize);
        }

        windowRenderer.StopGlow();
        lightDecal.StopGlow();
    }
}
