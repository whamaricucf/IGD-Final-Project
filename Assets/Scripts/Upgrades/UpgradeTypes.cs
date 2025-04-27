using UnityEngine;

public class UpgradeTypes : MonoBehaviour
{
    [System.Flags]
    public enum PassiveUpgradeType
    {
        None = 0,
        Health = 1 << 0,
        MoveSpeed = 1 << 1,
        Damage = 1 << 2,
        Regen = 1 << 3,
        Magnet = 1 << 4,
        Luck = 1 << 5,
        Armor = 1 << 6,
        Growth = 1 << 7,
        Revival = 1 << 8,
        Greed = 1 << 9,
        Amount = 1 << 10 
    }

    [System.Flags]
    public enum WeaponUpgradeType
    {
        None = 0,
        Damage = 1 << 0,
        Speed = 1 << 1,
        Area = 1 << 2,
        Cooldown = 1 << 3,
        Duration = 1 << 4,
        Pierce = 1 << 5,
        Amount = 1 << 6 
    }
}
