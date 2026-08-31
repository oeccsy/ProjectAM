using UnityEngine;

public class TitleScene : MonoBehaviour
{
    private TitleSceneUI titleSceneUI;

    private void Awake()
    {
        titleSceneUI = GetComponentInChildren<TitleSceneUI>();
    }

    private void Start()
    {
        titleSceneUI.StartButton.onClick.AddListener(() => LoadSimulationScene());
    }

    private void LoadSimulationScene()
    {
        GameObject simulationScene = Resources.Load<GameObject>("Prefabs/SimulationScene");
        Instantiate<GameObject>(simulationScene);

        Destroy(gameObject);
    }
}
