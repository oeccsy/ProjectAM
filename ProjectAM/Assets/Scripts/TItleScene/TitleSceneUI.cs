using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUI : MonoBehaviour
{
    [field: SerializeField] public Button StartButton { get; set; }

    private void Awake()
    {
        StartButton = transform.Find("StartButton").GetComponent<Button>();
    }
}
