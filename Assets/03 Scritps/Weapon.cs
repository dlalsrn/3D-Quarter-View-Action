using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        Melee,
        Range,
    }

    [SerializeField]
    private Type weaponType;
    public Type WeaponType => weaponType;
    [SerializeField]
    private int damage;
    public int Damage => damage;
    
    [SerializeField]
    private float rate;
    public float Rate => rate;
    [SerializeField]
    private int maxAmmo;
    public int MaxAmmo => maxAmmo;
    [SerializeField]
    private int curAmmo;
    public int CurAmmo => curAmmo;

    [SerializeField]
    private BoxCollider meleeArea;
    [SerializeField]
    private TrailRenderer trailRenderer;

    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform firePos;
    [SerializeField]
    private GameObject bulletCasePrefab;
    [SerializeField]
    private Transform bulletCasePos;

    public void Use()
    {
        switch (weaponType)
        {
            case Type.Melee:
                StartCoroutine(SwingRountine());
                break;
            case Type.Range:
                if (curAmmo > 0)
                {
                    curAmmo--;
                    StartCoroutine(ShotRoutine());
                }
                break;
        }
    }

    IEnumerator SwingRountine()
    {
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = true;
        trailRenderer.enabled = true;
        AudioManager.Instance.PlaySfxAudio(Sfx.Melee);

        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailRenderer.enabled = false;
    }

    IEnumerator ShotRoutine()
    {
        yield return null;
        
        // 1. 총알 발사
        GameObject bullet = Instantiate<GameObject>(bulletPrefab, firePos.position, firePos.rotation);
        bullet.GetComponent<Bullet>().Fire();

        // 2. 탄피 배출
        GameObject bulletCase = Instantiate<GameObject>(bulletCasePrefab, bulletCasePos.position, bulletCasePos.rotation);
        bulletCase.GetComponent<BulletCase>().Init();

        AudioManager.Instance.PlaySfxAudio(Sfx.Range);
    }

    public void Reload(int ammo)
    {
        curAmmo += ammo;
    }
}
