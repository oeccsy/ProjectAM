using UnityEngine;

/// <summary>
/// GameObject를 비활성화 하는 대신 숨김 상태로 전환하는 클래스
/// GameObject를 비활성화 하는 경우 스스로 활성화 할 수 없기 때문에 스스로 활성화 하기 위해 존재
/// </summary>
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
