using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// NPC의 애니메이션을 관리하는 클래스
/// </summary>
public class NpcAnimation : MonoBehaviour
{
    private Animator animator;
    private Movement movement;
    private Conversation conversation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        conversation = GetComponent<Conversation>();
    }

    private void Update()
    {
        animator.SetInteger("MovementState", (int)movement.State);
        animator.SetInteger("ConversationState", (int)conversation.ConversationState);
    }

    // 회오리 치듯 Y축 회전 + 스케일 축소 후 콜백
    public void SpinWithHideAnim(Action onComplete)
    {
        float duration = 1.0f;
        int spins = 3;

        Sequence sequence = DOTween.Sequence();
        
        sequence.Join(transform
            .DORotate(new Vector3(0f, 360f * spins, 0f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad));
        
        sequence.Join(transform
            .DOScale(Vector3.zero, duration)
            .SetEase(Ease.InBack));

        sequence.OnComplete(() => onComplete?.Invoke());
    }

    // 회오리 치듯 스케일 복원 후 콜백 (Hide의 역재생)
    public void SpinWithShowAnim(Action onComplete)
    {
        float duration = 1.0f;
        int spins = 3;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(transform
            .DORotate(new Vector3(0f, 360f * spins, 0f), duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad));

        sequence.Join(transform
            .DOScale(Vector3.one, duration)
            .SetEase(Ease.OutBack));

        sequence.OnComplete(() => onComplete?.Invoke());
    }
}
