using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartTest : MonoBehaviour
{
    public GameObject effect;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameObject vfx = GameObject.Instantiate(effect, transform.position, Quaternion.identity);
        }
    }
}
