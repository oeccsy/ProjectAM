using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUI : MonoBehaviour
{
    [field: SerializeField] [Bind("StartButton")]
    public Button StartButton { get; set; }
}