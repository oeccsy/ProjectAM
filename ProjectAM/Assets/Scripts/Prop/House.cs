using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public NpcColor owner;
    public Vector2Int origin;
    public Vector2Int size;
    public Vector2Int entrance;

    public List<NpcColor> guests;
}
