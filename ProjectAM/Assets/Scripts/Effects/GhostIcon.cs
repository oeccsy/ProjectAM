using DG.Tweening;
using UnityEngine;

/// <summary>
/// Creates and plays the temporary ghost icon used for the death presentation.
/// The sprite is generated at runtime and the object destroys itself when the tween ends.
/// </summary>
public class GhostIcon : MonoBehaviour
{
    private const int TextureSize = 64;

    [SerializeField]
    private float riseHeight = 3f;
    [SerializeField]
    private float riseDuration = 6f;
    [SerializeField]
    private float iconSize = 1.2f;
    [SerializeField]
    private float peakAlpha = 0.9f;

    private SpriteRenderer spriteRenderer;

    public static GhostIcon Spawn(Vector3 position)
    {
        GameObject instance = new GameObject("GhostIcon");
        instance.transform.position = position;

        GhostIcon ghost = instance.AddComponent<GhostIcon>();
        ghost.Rise();

        return ghost;
    }

    private void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateGhostSprite();
        spriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        transform.localScale = Vector3.one * iconSize;
    }

    private void LateUpdate()
    {
        Camera renderCamera = Camera.main;
        if (renderCamera == null) return;

        transform.forward = renderCamera.transform.forward;
    }

    private void Rise()
    {
        float targetHeight = transform.position.y + riseHeight;

        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0f, spriteRenderer.DOFade(peakAlpha, riseDuration * 0.35f));
        sequence.Insert(0f, transform.DOMoveY(targetHeight, riseDuration).SetEase(Ease.OutSine));
        sequence.Insert(riseDuration * 0.6f, spriteRenderer.DOFade(0f, riseDuration * 0.4f));
        sequence.OnComplete(() => Destroy(gameObject));
    }

    private Sprite CreateGhostSprite()
    {
        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = TextureSize * 0.5f;
        float radius = TextureSize * 0.45f;

        for (int row = 0; row < TextureSize; row++)
        {
            for (int col = 0; col < TextureSize; col++)
            {
                Vector2 point = new Vector2(col + 0.5f, row + 0.5f);
                float distance = Vector2.Distance(point, new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (distance / radius));

                texture.SetPixel(col, row, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        texture.Apply();

        Rect area = new Rect(0f, 0f, TextureSize, TextureSize);
        return Sprite.Create(texture, area, new Vector2(0.5f, 0.5f), TextureSize);
    }
}
