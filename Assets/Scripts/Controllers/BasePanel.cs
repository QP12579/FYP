using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    public bool isOpened { get; protected set; }
    protected virtual void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        isOpened = false;
    }

    public void OpenPanel()
    {
        if (!isOpened)
        {
            LeanTween.alphaCanvas(canvasGroup, 1f, 1f).setEaseInCubic();
            isOpened = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void ClosePanel()
    {
        if (isOpened)
        {
            LeanTween.alphaCanvas(canvasGroup, 0f, 1f).setEaseOutCubic().setOnComplete(
                () =>
                {
                    isOpened = false;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }
                );
        }
    }
}