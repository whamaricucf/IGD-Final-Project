using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Enemy/Stats")]
public class EnemyData : ScriptableObject
{
    public float speed;
    public int health;
    public int power;
    public float knockback;
    public int experience;
}
