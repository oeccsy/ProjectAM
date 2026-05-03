using System.Collections.Generic;
using UnityEngine;

public class CustomCurveRenderer : MonoBehaviour
{
    private BezierCurve _curve;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _curve = GetComponent<BezierCurve>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        _curve.OnCurveAdded += InitCurveMesh;
    }

    private void InitCurveMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        int verticesSize = 4;
        Vector3[] newVertices = new Vector3[verticesSize];

        float thickness = _curve.Thickness;
        float maxX = Mathf.Max(p1.x, p2.x, p3.x, p4.x) + thickness;
        float maxY = Mathf.Max(p1.y, p2.y, p3.y, p4.y) + thickness;
        float minX = Mathf.Min(p1.x, p2.x, p3.x, p4.x) - thickness;
        float minY = Mathf.Min(p1.y, p2.y, p3.y, p4.y) - thickness;
        
        // _meshFilter.mesh.vertices.CopyTo(newVertices, 0);
        newVertices[verticesSize - 4] = new Vector3(minX, minY, 0);
        newVertices[verticesSize - 3] = new Vector3(minX, maxY, 0);
        newVertices[verticesSize - 2] = new Vector3(maxX, maxY, 0);
        newVertices[verticesSize - 1] = new Vector3(maxX, minY, 0);
        
        int trianglesSize = 6;
        int[] newTriangles = new int[trianglesSize];

        // _meshFilter.mesh.triangles.CopyTo(newTriangles, 0);
        newTriangles[trianglesSize - 6] = verticesSize - 4;
        newTriangles[trianglesSize - 5] = verticesSize - 3;
        newTriangles[trianglesSize - 4] = verticesSize - 2;
        newTriangles[trianglesSize - 3] = verticesSize - 4;
        newTriangles[trianglesSize - 2] = verticesSize - 2;
        newTriangles[trianglesSize - 1] = verticesSize - 1;

        List<Vector3> p1List = new List<Vector3>();
        List<Vector3> p2List = new List<Vector3>();
        List<Vector3> p3List = new List<Vector3>();
        List<Vector3> p4List = new List<Vector3>();

        for(int i=0; i<4; i++)
        {
            p1List.Add(p1);
            p2List.Add(p2);
            p3List.Add(p3);
            p4List.Add(p4);
        }


        _meshFilter.mesh.vertices = newVertices;
        _meshFilter.mesh.triangles = newTriangles;
        _meshFilter.mesh.SetUVs(0, p1List);
        _meshFilter.mesh.SetUVs(1, p2List);
        _meshFilter.mesh.SetUVs(2, p3List);
        _meshFilter.mesh.SetUVs(3, p4List);
    }

    //private void AddCurveMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    //{
    //    int verticesSize = _meshFilter.mesh.vertexCount + 4;
    //    Vector3[] newVertices = new Vector3[verticesSize];

    //    float thickness = _curve.Thickness;
    //    float maxX = Mathf.Max(p1.x, p2.x, p3.x, p4.x) + thickness;
    //    float maxY = Mathf.Max(p1.y, p2.y, p3.y, p4.y) + thickness;
    //    float minX = Mathf.Min(p1.x, p2.x, p3.x, p4.x) - thickness;
    //    float minY = Mathf.Min(p1.y, p2.y, p3.y, p4.y) - thickness;

    //    _meshFilter.mesh.vertices.CopyTo(newVertices, 0);
    //    newVertices[verticesSize - 4] = new Vector3(minX, minY, 0);
    //    newVertices[verticesSize - 3] = new Vector3(minX, maxY, 0);
    //    newVertices[verticesSize - 2] = new Vector3(maxX, maxY, 0);
    //    newVertices[verticesSize - 1] = new Vector3(maxX, minY, 0);


    //    int trianglesSize = _meshFilter.mesh.triangles.Length + 6;
    //    int[] newTriangles = new int[trianglesSize];

    //    _meshFilter.mesh.triangles.CopyTo(newTriangles, 0);
    //    newTriangles[trianglesSize - 6] = verticesSize - 4;
    //    newTriangles[trianglesSize - 5] = verticesSize - 3;
    //    newTriangles[trianglesSize - 4] = verticesSize - 2;
    //    newTriangles[trianglesSize - 3] = verticesSize - 4;
    //    newTriangles[trianglesSize - 2] = verticesSize - 2;
    //    newTriangles[trianglesSize - 1] = verticesSize - 1;


    //    List<Vector3> starts = new List<Vector3>();
    //    List<Vector3> ends = new List<Vector3>();

    //    _meshFilter.mesh.GetUVs(0, starts);
    //    _meshFilter.mesh.GetUVs(1, ends);

    //    for (int i=0; i<4; i++)
    //    {

    //    }


    //    _meshFilter.mesh.vertices = newVertices;
    //    _meshFilter.mesh.triangles = newTriangles;
    //    _meshFilter.mesh.SetUVs(0, starts);
    //    _meshFilter.mesh.SetUVs(1, ends);
    //}
}
