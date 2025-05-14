using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnRecieveUI : MonoBehaviour
{
   
    public TMP_Text SpecialstatusText;

    private void Awake()
    {
        if (SpecialstatusText == null)
        {            
            GameObject obj = GameObject.Find("SpecialStatusText");
            if (obj)
                SpecialstatusText = obj.GetComponent<TMP_Text>();
            else
                Debug.LogWarning("StatusText object not found in the scene.");
        }
    }

    public void ShowStatusMessage(string message, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        SpecialstatusText.text = message;
        yield return new WaitForSeconds(duration);
        SpecialstatusText.text = "";
    }
}
