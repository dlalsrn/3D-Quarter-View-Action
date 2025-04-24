using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRelay : MonoBehaviour
{
    private Boss boss;

    private void Awake()
    {
        boss = GetComponentInParent<Boss>();
    }

    public void EndPattern()
    {
        boss.EndPattern();
    }

    public void EnableAttackArea()
    {
        boss.EnableAttackArea();
    }

    public void DisableAttackArea()
    {
        boss.DisableAttackArea();
    }
}
