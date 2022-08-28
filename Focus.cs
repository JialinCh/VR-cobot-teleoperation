using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour
{
    // The target we are following
    [SerializeField] private Transform target;

    [SerializeField] private bool once;

    [SerializeField] private bool reverse;

    // The distance in the x-z plane to the target
    [SerializeField] private float distance = 10.0f;

    private void OnEnable()
    {
        Follow();
    }

    void LateUpdate()
    {
        if (once) return;

        Follow();
    }

    private void Follow()
    {
        var forward = target.TransformDirection(Vector3.forward);

        transform.position = target.position + forward * distance;

        var toward = Quaternion.identity;

        toward.SetLookRotation(!reverse ? target.forward : -target.forward, target.up);

        transform.rotation = toward;
    }
}

