#if UNITY_EDITOR

using UnityEngine;

public static class DebugUtils
{
    public static void ShowTexture(Texture2D texture)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = Vector3.zero;

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;

        quad.GetComponent<MeshRenderer>().material = material;

        quad.transform.localScale = new Vector3(5, 5, 1);
    }
}

#endif