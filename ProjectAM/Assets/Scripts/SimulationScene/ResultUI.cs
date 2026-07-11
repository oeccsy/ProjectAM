using UnityEngine;
using UnityEngine.UI;

// 코드로 캔버스를 만들어 승패 문구를 화면 중앙에 보여준다.
public class ResultUI : MonoBehaviour
{
    public static ResultUI Show(string message, Transform parent)
    {
        GameObject root = new GameObject("ResultUI");
        root.transform.SetParent(parent, false);

        ResultUI ui = root.AddComponent<ResultUI>();
        ui.Build(message);

        return ui;
    }

    private void Build(string message)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>();

        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(transform, false);

        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.5f);
        Stretch(dimImage.rectTransform);

        GameObject label = new GameObject("Message");
        label.transform.SetParent(transform, false);

        Text text = label.AddComponent<Text>();
        text.text = message;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        Stretch(text.rectTransform);
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
