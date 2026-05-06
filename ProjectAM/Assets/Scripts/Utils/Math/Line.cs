using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField]
    private List<Vector3> points = new List<Vector3>();

    public List<Vector3> Points => points;

    [field: SerializeField]
    public Color Color { get; set; }
    [field: SerializeField]
    public float Thickness { get; set; }

    public delegate void LineHandler(Vector3 start, Vector3 end);
    public event LineHandler OnLineAdded;
    
    private void Awake()
    {
        Color = Color.white;
        Thickness = 1f;
    }

    private void Reset()
    {
        Color = Color.white;
        Thickness = 1f;

        AddLine(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        points.Add(start);
        points.Add(end);

        OnLineAdded?.Invoke(start, end);
    }
}
