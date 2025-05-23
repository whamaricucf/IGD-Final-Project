using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Player/Player Data")]
public class PlayerData : ScriptableObject
{
    [SerializeField]
    public float movSpd, regen, maxHP, str, area, projSpd, duration, cd, luck, growth, magnet;
    // movSpd = Move Speed (how fast the player moves)
    // regen = Health Recovery (how fast/much the player recovers over x time)
    // maxHP = Max Health (cap of player hp)
    // str = Strength (Might in Vampire Survivors, it's a damage multiplier)
    // area = Area/Range/Size of attacks
    // projSpd = Projectile Speed (how fast attacks can move)
    // duration = How long attacks stay active
    // cd = Cooldown (the time between each attack per weapon)
    // luck = Chance of pickups being dropped
    // growth = exp gain multiplier
    public int armor,  revival, amount;
    // armor = damage reduction
    // magnet = increases pickup range
    // revival = how many times player can die and come back to life
    // amount = The number of attacks per weapon
    public string startingWeaponTag; // e.g., "MagicWand", "KingBible", "Garlic"
}
