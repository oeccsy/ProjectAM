using UnityEngine;

/// <summary>
/// NPC의 집 출입을 관리하는 클래스.
/// </summary>
public class HouseEntry : MonoBehaviour
{
    private NPC owner;
    private SoftHide softHide;
    private House currentHouse;
    
    public bool IsInsideHouse => currentHouse != null;
    public House CurrentHouse => currentHouse;

    private void Awake()
    {
        owner = GetComponent<NPC>();
        softHide = GetComponent<SoftHide>();
    }

    public void Enter(House house)
    {
        currentHouse = house;
        house.guests.Add(owner.OwnColor);
        World.Instance.MapRuntime.Release(owner, owner.CurrentTile);

        softHide.Hide();
    }

    public void Exit()
    {
        if (currentHouse == null) return;

        softHide.Show();

        World.Instance.MapRuntime.Reserve(owner, owner.CurrentTile);
        currentHouse.guests.Remove(owner.OwnColor);
        currentHouse = null;
    }
}
