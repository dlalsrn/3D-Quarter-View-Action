using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRelay : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public void EndDodge()
    {
        player.EndDodge();
    }

    public void EndWeaponSwap()
    {
        player.EndWeaponSwap();
    }

    public void EndAttack()
    {
        player.EndAttack();
    }

    public void EndReload()
    {
        player.EndReload();
    }
}
