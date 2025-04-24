using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private int damage;
    public int Damage => damage;
    [SerializeField]
    private bool isMelee;
    [SerializeField]
    private bool isRock;
    [SerializeField]
    private float moveSpeed;

    public void Fire()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * moveSpeed;
    }

    private void OnTriggerEnter(Collider collider)
    {
        // 근접 공격을 하는 Enemy의 Attack Collider가 사라지는 것을 방지 
        if (isMelee)
        {
            return;
        }

        if (!isRock && (collider.CompareTag("Floor") || collider.CompareTag("Wall")))
        {
            Destroy(gameObject);
        }
        else if (collider.CompareTag("Player") || collider.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject, 3f);
        }
    }
}
