using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float orbitSpeed;
    private Vector3 offset;
    private Transform parentTransform;

    private void Start()
    {
        parentTransform = transform.parent;
        offset = transform.position - parentTransform.position;
    }

    private void Update()
    {
        parentTransform.position = target.position;
        transform.position = parentTransform.position + offset;
        transform.RotateAround(parentTransform.position, Vector3.up, orbitSpeed * Time.deltaTime);
        offset = transform.position - parentTransform.position;
    }
}
