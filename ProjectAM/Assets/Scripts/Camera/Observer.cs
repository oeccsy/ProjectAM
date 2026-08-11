using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

// Cinemachine을 이용해 NPC를 관찰하는 독립 클래스.
public class Observer : MonoBehaviour
{
    private const float ChangeInterval = 15f;

    [SerializeField]
    private CinemachineCamera observerCamera;

    [SerializeField]
    private Vector3 viewOffset;
    [SerializeField]
    private float followDamping;
    [SerializeField]
    private Vector2 aimDamping;
    [SerializeField]
    private Vector2 screenPosition = new Vector2(0f, 0.2f);

    [Header("연출 포커스")]
    [SerializeField]
    private Vector3 focusViewOffset = new Vector3(-6f, 11f, -13f);
    // 집 피벗이 바닥에 있어 몸통은 이 지점보다 위로 보인다. 그만큼 더 내려 잡는다
    [SerializeField]
    private Vector2 focusScreenPosition = new Vector2(0f, -0.28f);

    private CinemachineFollow follow;
    private CinemachineRotationComposer composer;
    private NPC currentTarget;
    private float timer;
    private float focusRemainTime;

    private void Start()
    {
        EnsureCamera();
        EnsureBrain();
        ChangeTarget();
    }

    private void Update()
    {
        ApplyParameters();

        // 연출이 지정한 대상을 보는 동안에는 무작위 순환을 멈춘다
        if (focusRemainTime > 0f)
        {
            focusRemainTime -= Time.deltaTime;
            if (focusRemainTime <= 0f) ChangeTarget();

            return;
        }

        timer += Time.deltaTime;
        if (timer >= ChangeInterval)
        {
            timer = 0f;
            ChangeTarget();
        }
    }

    // 연출용으로 특정 대상을 일정 시간 강제로 비춘다
    public void FocusOn(Transform target, float duration)
    {
        if (target == null) return;

        focusRemainTime = duration;
        timer = 0f;
        currentTarget = null;

        observerCamera.Follow = target;
        observerCamera.LookAt = target;
    }

    // CinemachineCamera와 위치 추적 컴포넌트를 보장한다.
    private void EnsureCamera()
    {
        if (observerCamera == null)
        {
            GameObject cameraObject = new GameObject("ObserverCamera");
            cameraObject.transform.SetParent(transform, false);
            observerCamera = cameraObject.AddComponent<CinemachineCamera>();
        }

        follow = observerCamera.GetComponent<CinemachineFollow>();
        if (follow == null) follow = observerCamera.gameObject.AddComponent<CinemachineFollow>();
        follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;

        composer = observerCamera.GetComponent<CinemachineRotationComposer>();
        if (composer == null) composer = observerCamera.gameObject.AddComponent<CinemachineRotationComposer>();
    }

    private void EnsureBrain()
    {
        Camera renderCamera = Camera.main;
        if (renderCamera == null) renderCamera = FindFirstObjectByType<Camera>();
        if (renderCamera == null) return;

        if (renderCamera.GetComponent<CinemachineBrain>() == null)
        {
            renderCamera.gameObject.AddComponent<CinemachineBrain>();
        }
    }

    // 인스펙터에서 실시간으로 조정할 수 있도록 매 프레임 파라미터 적용
    private void ApplyParameters()
    {
        // 연출 중에는 대상을 화면 아래쪽에 두고 멀리서 잡아 위쪽 여백을 확보한다
        bool isFocusing = focusRemainTime > 0f;

        follow.FollowOffset = isFocusing ? focusViewOffset : viewOffset;
        follow.TrackerSettings.PositionDamping = Vector3.one * followDamping;
        composer.Damping = aimDamping;
        composer.Composition.ScreenPosition = isFocusing ? focusScreenPosition : screenPosition;
    }

    // 무작위로 관찰 대상 지정
    private void ChangeTarget()
    {
        NPC[] npcs = FindObjectsByType<NPC>(FindObjectsSortMode.None);
        if (npcs.Length == 0) return;

        NPC next = npcs[Random.Range(0, npcs.Length)];
        if (npcs.Length > 1)
        {
            while (next == currentTarget)
            {
                next = npcs[Random.Range(0, npcs.Length)];
            }
        }

        currentTarget = next;
        observerCamera.Follow = currentTarget.transform;
        observerCamera.LookAt = currentTarget.transform;
    }
}
