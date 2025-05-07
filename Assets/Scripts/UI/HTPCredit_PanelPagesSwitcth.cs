using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTPCredit_PanelPagesSwitcth : MonoBehaviour
{
    public GameObject[] pages;
    private int currentPageIndex = 0;

    void Start()
    {
        UpdatePageVisibility();
    }

    public void ShowNextPage()
    {
        currentPageIndex = (currentPageIndex + 1) % pages.Length;
        UpdatePageVisibility();
    }

    public void ShowPreviousPage()
    {
        currentPageIndex = (currentPageIndex - 1 + pages.Length) % pages.Length;
        UpdatePageVisibility();
    }

    private void UpdatePageVisibility()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
    }
}
