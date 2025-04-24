using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti);
}
