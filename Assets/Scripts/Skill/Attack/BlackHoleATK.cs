using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleATK : AOEAttack
{
    public float pullForce = 10f;
    public float maxPullDistance = 10f;

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);
        PullObject(other);
    }

    void PullObject(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 direction = transform.position - other.transform.position;
        float distance = direction.magnitude;

        if (distance > maxPullDistance) return;

        direction.Normalize(); 
        float forceFactor = 1 / (distance + 0.1f);
        float distanceFactor = Mathf.Clamp01(1 - (distance / maxPullDistance));
        Vector3 force = direction * pullForce * distanceFactor * rb.mass;
        rb.AddForce(force, ForceMode.Force);
    }
}
