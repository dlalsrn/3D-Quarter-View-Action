using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private Rigidbody rigid;
    private ParticleSystem[] particles;

    [SerializeField]
    private int damage;
    [SerializeField]
    private GameObject meshObejct;
    [SerializeField]
    private GameObject bombEffect;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void Throw(Vector3 throwVec)
    {
        throwVec.y = 13f;
        rigid.AddForce(throwVec, ForceMode.Impulse);
        rigid.AddTorque((Vector3.forward * Random.Range(0.3f, 0.7f) + Vector3.right  * Random.Range(0.3f, 0.7f)) * 5f, ForceMode.Impulse);
    
        StartCoroutine(BombRoutine());
    }

    IEnumerator BombRoutine()
    {
        yield return new WaitForSeconds(3f); // 3초 후 폭발
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObejct.SetActive(false);
        bombEffect.SetActive(true); // 폭발 효과 On
        AudioManager.Instance.PlaySfxAudio(Sfx.Grenade);

        // 15 유닛 범위 내에 존재하는 모든 Enemy Object에 Damage
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15f, Vector3.up, 0f, LayerMask.GetMask("Enemy"));
        foreach (RaycastHit hitObj in rayHits)
        {
            Enemy enemy = hitObj.collider.GetComponent<Enemy>();
            enemy.OnHit(damage, transform.position, true);
        }

        Destroy(gameObject, 4f);
    }
}
