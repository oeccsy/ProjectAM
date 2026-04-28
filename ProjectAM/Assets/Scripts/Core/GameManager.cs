using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;
        
        GameObject titleScene = Resources.Load<GameObject>("Prefabs/TitleScene");
        Instantiate<GameObject>(titleScene);
    }
}
