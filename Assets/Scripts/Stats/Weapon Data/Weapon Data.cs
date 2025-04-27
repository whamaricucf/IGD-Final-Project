using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    public string wepName; // the name of the weapon

    [Header("Base Stats")]
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

    [Header("Runtime Stats (copied from base at runtime)")]
    [HideInInspector] public float runtimeArea;
    [HideInInspector] public float runtimeSpd;
    [HideInInspector] public float runtimeDuration;
    [HideInInspector] public float runtimeCd;
    [HideInInspector] public float runtimeProjInterval;
    [HideInInspector] public float runtimeHitDelay;
    [HideInInspector] public float runtimeKnockback;
    [HideInInspector] public float runtimeCritChance;
    [HideInInspector] public float runtimeCritMulti;
    [HideInInspector] public int runtimeBaseDMG;
    [HideInInspector] public int runtimeAmount;
    [HideInInspector] public int runtimePierce;
    [HideInInspector] public int runtimeLimit;

    private void OnEnable()
    {
        ResetToBase();
    }

    public void ResetToBase()
    {
        runtimeArea = area;
        runtimeSpd = spd;
        runtimeDuration = duration;
        runtimeCd = cd;
        runtimeProjInterval = projInterval;
        runtimeHitDelay = hitDelay;
        runtimeKnockback = knockback;
        runtimeCritChance = critChance;
        runtimeCritMulti = critMulti;
        runtimeBaseDMG = baseDMG;
        runtimeAmount = amount;
        runtimePierce = pierce;
        runtimeLimit = limit;
    }
}
