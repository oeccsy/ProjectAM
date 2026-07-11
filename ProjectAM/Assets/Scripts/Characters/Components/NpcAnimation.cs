using System;
using DG.Tweening;
using UnityEngine;

public class NpcAnimation : MonoBehaviour
{
    [SerializeField]
    private float enterDuration = 1.0f;
    [SerializeField]
    private int enterSpins = 3;

    // 회오리 치듯 Y축 회전 + 스케일 축소 후 콜백
    public void PlayEnterHouse(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Join(transform
            .DORotate(new Vector3(0f, 360f * enterSpins, 0f), enterDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad));
        
        sequence.Join(transform
            .DOScale(Vector3.zero, enterDuration)
            .SetEase(Ease.InBack));

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    // 밤 사건/추방 퇴장: 회오리 치듯 사라진 뒤 콜백
    public void PlayVanish(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Join(transform
            .DORotate(new Vector3(0f, 360f * enterSpins, 0f), enterDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InQuad));

        sequence.Join(transform
            .DOScale(Vector3.zero, enterDuration)
            .SetEase(Ease.InBack));

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    // 회오리 치듯 스케일 복원 후 콜백 (입장의 역재생)
    public void PlayExitHouse(Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Join(transform
            .DORotate(new Vector3(0f, 360f * enterSpins, 0f), enterDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad));

        sequence.Join(transform
            .DOScale(Vector3.one, enterDuration)
            .SetEase(Ease.OutBack));

        sequence.OnComplete(() => onComplete?.Invoke());
    }
}
