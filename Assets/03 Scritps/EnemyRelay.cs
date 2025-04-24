using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRelay : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public void EnableAttackArea()
    {
        enemy.EnableAttackArea();
    }

    public void DisableAttackArea()
    {
        enemy.DisableAttackArea();
    }

    public void EndAttack()
    {
        enemy.EndAttack();
    }
}
