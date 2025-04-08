using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelListController : MonoBehaviour
{
    public BasePanel GameUIPanel;
    public BasePanel[] pagesPanel;
    private int pages = 0;
    private bool isIncreasing;

    private void Start()
    {
        pages = pagesPanel.Length;
        if(GameUIPanel == null)
            Debug.LogWarning("Haven't Assign the GameUIPanel Yet.");
        isIncreasing = false;
    }


    private void CloseOtherPages(int nowPage)
    {
        for (int i = 0; i < pages; i++) 
        {
            if(i!=nowPage) pagesPanel[i].ClosePanel();
        }
    }

    public void ChangeToSkillPage()
    {
        CloseOtherPages(0);
        pagesPanel[0].OpenPanel();
    }

    public void ChangeToAbilityPage()
    {
        CloseOtherPages(0);
        pagesPanel[0].OpenPanel();
    }

    public void ChangeToSpecialAttackPage()
    {
        CloseOtherPages(1);
        pagesPanel[1].OpenPanel();
    }

    public void ChangeToSettingPage()
    {
        CloseOtherPages(2);
        pagesPanel[2].OpenPanel();
    }
}

public enum Page
{
    SkillOrAbility,
    SpecialAttack,
    Setting
}
