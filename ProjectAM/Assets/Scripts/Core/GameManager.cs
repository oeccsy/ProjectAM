using UnityEngine;

/// <summary>
/// 애플리케이션을 시작하는 Bootstrap 역할의 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;
        
        GameObject titleScene = Resources.Load<GameObject>("Prefabs/TitleScene");
        Instantiate<GameObject>(titleScene);
    }
}
