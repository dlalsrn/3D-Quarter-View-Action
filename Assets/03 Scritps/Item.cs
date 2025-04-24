using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        Ammo,
        Coin,
        Grenade,
        Heart,
        Weapon,
    }

    private Rigidbody rigid;
    private SphereCollider sphereCollider;

    [SerializeField]
    private Type itemType;
    public Type ItemType => itemType;
    [SerializeField]
    private int value; // Weapon은 Index, Coin은 Score, Grenade는 개수, Heart는 회복량, Ammo는 충전량
    public int Value => value;

    private float rotateSpeed = 20f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 땅에 닿으면 물체의 바닥 Collider 비활성화
        if (collision.collider.CompareTag("Floor"))
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
