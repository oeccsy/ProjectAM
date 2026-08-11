using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 하단에 상황을 한 줄로 알리는 UI.
/// 지금은 사망 연출이 쓰지만 추방·승패에도 같은 자리를 쓴다.
/// </summary>
public class NoticeUI : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 1.5f;
    [SerializeField]
    private float fontSize = 42f;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI noticeText;

    public static NoticeUI Create()
    {
        GameObject root = new GameObject("NoticeUI");

        return root.AddComponent<NoticeUI>();
    }

    private void Awake()
    {
        BuildCanvas();
        BuildText();
    }

    public void Show(string message, float duration)
    {
        noticeText.text = message;

        canvasGroup.DOKill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1f, fadeDuration));
        sequence.AppendInterval(duration);
        sequence.Append(canvasGroup.DOFade(0f, fadeDuration));
    }

    private void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void BuildText()
    {
        GameObject textObject = new GameObject("NoticeText");
        textObject.transform.SetParent(transform, false);

        noticeText = textObject.AddComponent<TextMeshProUGUI>();
        noticeText.alignment = TextAlignmentOptions.Center;
        noticeText.fontSize = fontSize;
        noticeText.color = Color.white;
        noticeText.raycastTarget = false;

        RectTransform rect = noticeText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 140f);
        rect.sizeDelta = new Vector2(1400f, 120f);
    }
}
