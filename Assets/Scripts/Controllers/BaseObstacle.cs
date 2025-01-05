using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    public void PlayCollisionSound()
    {
        GetComponent<AudioSource>().Play();
        Debug.Log("Play");
    }
}
