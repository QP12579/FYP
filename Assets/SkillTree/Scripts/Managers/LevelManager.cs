using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public TextMeshProUGUI levelText;
    private int levelP1 = 0, levelP2 = 0;
    public GameObject choicePanel;
    public Player P1, P2;

    private void Awake()
    {
        instance = this; 
        levelP1 = P1.level;
        levelP2 = P2.level;
    }
    public void P1LevelUp()
    {
        levelP1++;
        choicePanel.SetActive(true);
        UpdateLevelText();
        ChoiceManager.instance.AssignRandomChoice();
    }

    private void UpdateLevelText()
    {
        P1.level = levelP1;
        // Format to two digits
        levelText.text = levelP1.ToString("D2"); 
    }
}
