using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCase : MonoBehaviour
{
    [SerializeField]
    private float force;

    // 특정 방향으로 탄피 배출
    public void Init()
    {
        Vector3 caseVec = transform.forward * Random.Range(-3f, -2f) + Vector3.up * Random.Range(3f, 4f);
        GetComponent<Rigidbody>().AddForce(caseVec * force, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(Vector3.up * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Floor") || collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject, 3f);
        }
    }
}
