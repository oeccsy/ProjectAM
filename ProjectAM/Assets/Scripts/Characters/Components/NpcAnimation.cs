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
}
