using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestEnemy : MonoBehaviour
{
    private Rigidbody rigid;
    private MeshRenderer meshRenderer;
    private const int EnemyDeadLayer = 13;

    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int curHealth;

    [SerializeField]
    private float knockBackForce;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void OnHit(int damage, Vector3 firePos, bool isGrenade)
    {
        curHealth -= damage;
        Vector3 reactVec = transform.position - firePos;
        StartCoroutine(OnHitRoutine(reactVec, isGrenade));
    }

    IEnumerator OnHitRoutine(Vector3 reactVec, bool isGrenade)
    {
        meshRenderer.material.color = new Color(255f, 100f, 100f, 255f) / 255f;


        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            meshRenderer.material.color = Color.white;
        }
        else
        {
            gameObject.layer = EnemyDeadLayer;
            meshRenderer.material.color = Color.gray;
            rigid.velocity = Vector3.zero;

            if (isGrenade) // 수류탄에 의해 사망했을 경우
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 2f;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * knockBackForce * 2f, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 5f, ForceMode.Impulse);
            }
            else // 근접 무기, 원거리 무기에 의해 사망했을 경우
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * knockBackForce, ForceMode.Impulse);
            }

            Destroy(gameObject, 3f);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Melee"))
        {
            Weapon weapon = collider.GetComponent<Weapon>();
            OnHit(weapon.Damage, collider.transform.position, false);
        }
        else if (collider.CompareTag("PlayerBullet"))
        {
            Bullet bullet = collider.GetComponent<Bullet>();
            OnHit(bullet.Damage, collider.transform.position, false);
        }
    }
}
