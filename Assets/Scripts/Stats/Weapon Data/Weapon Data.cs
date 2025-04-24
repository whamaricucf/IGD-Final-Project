using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    public string wepName; // the name of the weapon
    public float area, spd, duration, cd, projInterval, hitDelay, knockback, critChance, critMulti;
    // area = size of the attack/projectile
    // spd = projectile speed
    // duration = how long an attack lasts
    // cd = cooldown = length of time before the attack is triggered again
    // projInterval = time required for an additional projectile to be fired between each cooldown
    // hitDelay = Hitbox Delay = the time before same enemy can be hit by the same weapon
    // knockback = knockback dealt multiplier
    // critChance = Critical Hit Chance
    // critMulti = Critical Hit Damage Multiplier
    public int maxLvl, rarity, baseDMG, amount, pierce, limit;
    // maxLvl = Max Level = highest # level a weapon can get upgrades for. ex: a weapon with maxLvl of 3 would be able to be upgraded twice (starting at 1)
    // rarity = weighted chance of appearing in upgrade pool
    // baseDMG = Base Damage
    // amount = amount of projectiles fired per use
    // pierce = number of enemies the projectile can hit before being used up
    // limit = maximum amount of projectiles that can be on screen
    public bool wallBlock;
    // wallBlock = if the projectile is blocked by walls
}
