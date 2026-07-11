using DG.Tweening;
using UnityEngine;

// NPC 간 접촉(대화)의 신체 상태. 절차 결정은 두뇌(InteractAction)가 한다.
public class Interaction : MonoBehaviour
{
    private const float FaceTurnDuration = 0.3f;

    private NPC owner;
    [SerializeField]
    private InteractionState state = InteractionState.None;
    private bool brainSuspended;

    // 집 출입 연출처럼 접촉이 끼어들면 안 되는 동안 true
    public bool Busy { get; set; }

    public InteractionState State => state;
    public NPC Partner { get; private set; }
    public bool CanBeEngaged => !Busy && state == InteractionState.None && owner.HouseEntry.CurrentHouse == null;

    private void Awake()
    {
        owner = GetComponent<NPC>();
    }

    // 상대가 나를 발견했다. 하던 일을 멈추고 제자리에 선다.
    public void HoldBy(NPC engager)
    {
        state = InteractionState.Held;
        Partner = engager;
        brainSuspended = true;

        owner.SetBrainActive(false);
        owner.Movement.RequestStop();
    }

    // 내가 대상을 발견해 다가가기 시작한다.
    public void BeginEngage(NPC target)
    {
        state = InteractionState.Engaging;
        Partner = target;
    }

    public void BeginTalkWith(NPC partner)
    {
        state = InteractionState.Talking;
        Partner = partner;
        FaceTo(partner.transform.position);
    }

    // 어떤 상태에서든 접촉을 끝내고 일상으로 복귀한다.
    public void Release()
    {
        state = InteractionState.None;
        Partner = null;

        if (brainSuspended)
        {
            brainSuspended = false;
            owner.SetBrainActive(true);
        }
    }

    private void FaceTo(Vector3 worldPosition)
    {
        Vector3 direction = worldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        transform.DORotateQuaternion(Quaternion.LookRotation(direction), FaceTurnDuration);
    }
}
