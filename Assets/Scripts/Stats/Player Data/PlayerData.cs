using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : ScriptableObject
{
    [SerializeField]
    float movSpd, regen, maxHP, str, area, projSpd, duration, cd, luck, growth;
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
    int armor, magnet, revival, amount;
    // armor = damage reduction
    // magnet = increases pickup range
    // revival = how many times player can die and come back to life
    // amount = The number of attacks per weapon
}
