using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public TextMeshProUGUI levelText;
    private int level = 0;
    public GameObject choicePanel;

    private void Awake()
    {
        instance = this; 
        level = Player.instance.level;
    }
    public void LevelUp()
    {
        level++;
        choicePanel.SetActive(true);
        UpdateLevelText();
        ChoiceManager.instance.AssignRandomChoice();
    }

    public void LevelDown()
    {
        if (level > 0)
        {
            level--;
        }
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        Player.instance.level = level;
        // Format to two digits
        levelText.text = level.ToString("D2"); 
    }
}
