using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : MonoBehaviour
{
    //enemy script will be sending data here based on collisions

    //when a player's weapon projectile collides with an enemy this will run
    float WeaponDamageCalc(/*take stats of enemy, player, and weapon here*/)
    {
        float dmg = 0; //damage (can't see enemy HP but it will still be run in the bg)
        // if dmg (after modifiers) > current enemy hp -> kill enemy
        // else deal dmg to enemy
        return dmg;
    }

    //when an enemy collides with the player this will run
    float EnemyDamageCalc(/*take stats of enemy and player here */)
    {
        float dmg = 0; // using floats because hp will be on a UI slider
        // if dmg (after modifiers) > current player hp -> kill player
        // else deal dmg to player
        return dmg;
    }
}
