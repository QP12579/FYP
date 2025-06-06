using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[System.Serializable]
public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public int id;
    public int level;
    public Image iconImage;

    [SerializeField] public bool isUnlocked;
    [SerializeField] public bool isSelected;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SkillPanel.instance.ShowTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillPanel.instance.HideTooltip();
    }
}
