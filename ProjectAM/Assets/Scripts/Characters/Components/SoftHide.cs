using UnityEngine;

// GameObject는 켜둔 채(스크립트 계속 동작) Renderer/Collider/물리만 잠시 끈다.
// SetActive(false)와 달리 스스로 복귀할 수 있어 외부 개입이 필요 없다.
public class SoftHide : MonoBehaviour
{
    private Renderer[] renderers;
    private Collider[] colliders;
    private Rigidbody rigidbody;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Hide()
    {
        foreach (Renderer renderer in renderers) renderer.enabled = false;
        foreach (Collider collider in colliders) collider.enabled = false;

        if (rigidbody != null) rigidbody.isKinematic = true;
    }
    
    public void Show()
    {
        foreach (Renderer renderer in renderers) renderer.enabled = true;
        foreach (Collider collider in colliders) collider.enabled = true;

        if (rigidbody != null) rigidbody.isKinematic = false;
    }
}
