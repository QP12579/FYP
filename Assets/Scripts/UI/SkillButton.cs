using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    private System.Action onClickAction;

    public void Initialize(SkillData skill, System.Action onClick)
    {
        iconImage.sprite = skill.Icon;
        nameText.text = skill.Name;
        descriptionText.text = skill.Description;
        onClickAction = onClick;
    }

    public void OnClick()
    {
        onClickAction?.Invoke();
    }
}