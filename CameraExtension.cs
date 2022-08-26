using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CameraExtension
{
    public Transform GetClosestTarget(Transform target)
    {
        LayerMask layer = LayerMask.NameToLayer("Enemy");
        int layerIndex = 1 << layer.value;
        Collider[] colliders = UnityEngine.Physics.OverlapSphere(target.transform.position, 10000, layerIndex);
        Array.Sort(colliders, new FindClosestEnemy(target.transform));
        Collider col = colliders.FirstOrDefault(c => c.GetComponent<Transform>());
        return col.GetComponent<Transform>();
    }
}
