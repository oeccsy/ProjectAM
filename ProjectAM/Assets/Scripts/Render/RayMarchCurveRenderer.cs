using System.Collections.Generic;
using UnityEngine;

public class RayMarchCurveRenderer : MonoBehaviour
{
    [System.Serializable]
    public struct CurveData
    {
        public Vector3 p1, p2, p3, p4;
    }

    public List<CurveData> curves = new List<CurveData>
    {
        new CurveData { p1 = new Vector3(0, 0,  0), p2 = new Vector3(0, 3,  2), p3 = new Vector3(3, 3, -2), p4 = new Vector3(3, 0, 0) },
        new CurveData { p1 = new Vector3(3, 0,  0), p2 = new Vector3(3, 3, -2), p3 = new Vector3(6, 3,  2), p4 = new Vector3(6, 0, 0) },
    };

    public Color color     = new Color(1f, 0.5f, 0f, 1f);
    public float thickness = 0.1f;

    private static Mesh _sharedUnitCube;

    private void Awake()
    {
        Build();
    }

    private void Build()
    {
        if (curves == null || curves.Count == 0) return;

        // 모든 커브가 공유하는 머티리얼 1개
        var sharedMat = new Material(Shader.Find("Custom/RayMarchCurveShader"));
        sharedMat.enableInstancing = true;
        sharedMat.SetColor("_Color",     color);
        sharedMat.SetFloat("_Thickness", thickness);

        // 모든 커브가 공유하는 단위 정육면체 메시 1개
        if (_sharedUnitCube == null)
            _sharedUnitCube = CreateUnitCube();

        foreach (var curve in curves)
            CreateCurveObject(curve, sharedMat);
    }

    private void CreateCurveObject(CurveData curve, Material sharedMat)
    {
        var go = new GameObject("Curve");
        go.transform.SetParent(transform, false);

        Vector3 pad    = Vector3.one * (thickness + 0.1f);
        Vector3 boxMin = Min4(curve.p1, curve.p2, curve.p3, curve.p4) - pad;
        Vector3 boxMax = Max4(curve.p1, curve.p2, curve.p3, curve.p4) + pad;

        // 단위 정육면체를 position + scale로 AABB에 맞게 배치
        go.transform.position   = boxMin;
        go.transform.localScale = boxMax - boxMin;

        var meshFilter   = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh         = _sharedUnitCube; // 공유 메시
        meshRenderer.sharedMaterial   = sharedMat;       // 공유 머티리얼

        // 인스턴스마다 다른 제어점은 MaterialPropertyBlock으로 전달
        var block = new MaterialPropertyBlock();
        block.SetVector("_P1", curve.p1);
        block.SetVector("_P2", curve.p2);
        block.SetVector("_P3", curve.p3);
        block.SetVector("_P4", curve.p4);
        meshRenderer.SetPropertyBlock(block);
    }

    // 로컬 좌표 (0,0,0) ~ (1,1,1) 단위 정육면체
    private static Mesh CreateUnitCube()
    {
        var v = new Vector3[]
        {
            new Vector3(0, 0, 0), // 0
            new Vector3(1, 0, 0), // 1
            new Vector3(1, 1, 0), // 2
            new Vector3(0, 1, 0), // 3
            new Vector3(0, 0, 1), // 4
            new Vector3(1, 0, 1), // 5
            new Vector3(1, 1, 1), // 6
            new Vector3(0, 1, 1), // 7
        };

        var t = new int[]
        {
            0,2,1, 0,3,2,
            4,5,6, 4,6,7,
            0,1,5, 0,5,4,
            3,7,6, 3,6,2,
            0,4,7, 0,7,3,
            1,2,6, 1,6,5,
        };

        var mesh = new Mesh { vertices = v, triangles = t };
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Vector3 Min4(Vector3 a, Vector3 b, Vector3 c, Vector3 d) =>
        new Vector3(Mathf.Min(a.x, b.x, c.x, d.x),
                    Mathf.Min(a.y, b.y, c.y, d.y),
                    Mathf.Min(a.z, b.z, c.z, d.z));

    private static Vector3 Max4(Vector3 a, Vector3 b, Vector3 c, Vector3 d) =>
        new Vector3(Mathf.Max(a.x, b.x, c.x, d.x),
                    Mathf.Max(a.y, b.y, c.y, d.y),
                    Mathf.Max(a.z, b.z, c.z, d.z));
}
