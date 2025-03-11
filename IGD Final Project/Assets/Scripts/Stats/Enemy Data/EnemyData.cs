using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : ScriptableObject
{
    [SerializeField]
    float movSpd, maxHP, knockback, power;
    // movSpd = Movement Speed = how fast the enemy moves
    // maxHP = Max Health = how much damage is required to kill the enemy
    // knockback = knockback taken multiplier
    int exp;
    // exp = how much experience is dropped by the enemy
}
