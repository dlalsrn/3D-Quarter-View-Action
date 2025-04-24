using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet
{
    private Rigidbody rigid;
    private float angularForce = 2f;
    private float scaleValue = 0.1f;

    private bool isShoot;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(GainPowerTime());
        StartCoroutine(GainPower());
    }

    IEnumerator GainPowerTime()
    {
        yield return new WaitForSeconds(2f);
        isShoot = true;
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularForce += 0.1f;
            scaleValue += 0.01f;
            // angularForce += 0.04f;
            // scaleValue += 0.005f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularForce, ForceMode.Acceleration);
            yield return new WaitForSeconds(0.01f);
        }

        rigid.useGravity = true;
    }
}
