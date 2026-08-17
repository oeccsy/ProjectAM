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
        house.TurnOnLight();
    }

    public void Exit()
    {
        if (currentHouse == null) return;

        currentHouse.TurnOffLight();
        softHide.Show();

        World.Instance.MapRuntime.Reserve(owner, owner.CurrentTile);
        currentHouse.guests.Remove(owner.OwnColor);
        currentHouse = null;
    }

    // 사망으로 자리를 비운다. Exit과 달리 다시 나타나지 않는다
    public void Vacate()
    {
        if (currentHouse == null)
        {
            World.Instance.MapRuntime.Release(owner, owner.CurrentTile);
            return;
        }

        currentHouse.guests.Remove(owner.OwnColor);
        currentHouse = null;
    }
}
