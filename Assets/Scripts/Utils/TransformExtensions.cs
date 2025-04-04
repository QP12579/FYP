using Mirror.BouncyCastle.Asn1.X509;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TransformExtensions 
{
    public static void Clear(this Transform transform)
    {
        while (transform.childCount > 0 )
        {
            Transform child = transform.transform.GetChild(0);
            child.SetParent(null);
            Object.Destroy(child.gameObject);
        }
    }
}
