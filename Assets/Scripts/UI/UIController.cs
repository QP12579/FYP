using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject GamingUI;

    public GameObject SkillOrAbilityPanel;
    public GameObject SpecialAttackPanel;
    public GameObject SettingPanel;

    public Scrollbar SkillAbilityscrollbar;
    public Scrollbar SpecialAttackscrollbar;

    public RectTransform cyanSelectedBlock;
    private bool isIncreasing = false;
    private bool isDecreasing = false;
    
    public Button arrowButton; 
    public Button skillOrAbilityButton;
    public Button specialAttackButton;
    public Button settingButton;
    public Button EseButton;

    public GameObject SkillClickArea;
    public GameObject AbilityClickArea;

    public float scrollSpeed = 0.1f;

    private void Start()
    {
        GamingUI.SetActive(false);
        SkillAbilityscrollbar.value = 0;

        arrowButton.onClick.AddListener(OnArrowButtonClick);
        skillOrAbilityButton.onClick.AddListener(OnSkillOrAbilityButtonClick);
        specialAttackButton.onClick.AddListener(OnSpecialAttackButtonClick);
        settingButton.onClick.AddListener(OnSettingButtonClick);
        EseButton.onClick.AddListener(OnEseButtonClick);

        UpdateButtonActive();

        SkillOrAbilityPanel.SetActive(true);
        SpecialAttackPanel.SetActive(false);
        SettingPanel.SetActive(false);
        SkillClickArea.SetActive(false);
        AbilityClickArea.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { GamingUI.SetActive(true); }

        if(Input.GetKeyDown(KeyCode.T)) { 
            ShowSkillPart();
            ShowSkillAbilityPanel();
            GamingUI.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) { GamingUI.SetActive(false); }

        // get mouse scroll value
        float scrollDelta = Input.mouseScrollDelta.y;

        // update value if have scroll value
        if (scrollDelta != 0)
        {
            // update scrollbar value between 0 and 1
            SkillAbilityscrollbar.value += scrollDelta * scrollSpeed;
            SkillAbilityscrollbar.value = Mathf.Clamp01(SkillAbilityscrollbar.value);
        }

        if (isIncreasing)
        {
            isDecreasing = false;
            // value +0.2 per sec
            SkillAbilityscrollbar.value += 3f * Time.deltaTime;
            if (SkillAbilityscrollbar.value >= 1f)
            {
                SkillAbilityscrollbar.value = 1f; // make sure that is not more than 1
                isIncreasing = false; // stop growing
                UpdateButtonActive();
            }
        }

        if (isDecreasing) 
        {
            isIncreasing = false;
            // value -0.2 per sec
            SkillAbilityscrollbar.value -= 3f * Time.deltaTime;
            if (SkillAbilityscrollbar.value <= 0f)
            {
                SkillAbilityscrollbar.value = 0f; // make sure that is 0
                isDecreasing = false; // stop going down
                UpdateButtonActive();
            }
        }

        
        if (SkillOrAbilityPanel.activeInHierarchy == false) 
        { 
            skillOrAbilityButton.enabled = false;
            SkillClickArea.SetActive(true);
            AbilityClickArea.SetActive(true);
        }
        else
        {
            skillOrAbilityButton.enabled = true;
            SkillClickArea.SetActive(false);
            AbilityClickArea.SetActive(false);
        }
        // control button active
        UpdateButtonActive();

        // update CyanSelectedBlock position and scale according to the scrollbar value
        UpdateCyanSelectedBlock();
    }

    public void ShowSkillAbilityPanel()
    { 
        SkillOrAbilityPanel.SetActive(true);
        SpecialAttackPanel.SetActive(false);
        SettingPanel.SetActive(false);
    }

    public void ShowSkillPart() { SkillAbilityscrollbar.value = 0f; }

    public void ShowAbilityPart() { SkillAbilityscrollbar.value = 1f; }
    private void OnArrowButtonClick()
    {
        isIncreasing = true; 
    }

    private void OnSpecialAttackButtonClick()
    {
        SkillOrAbilityPanel.SetActive(false);
        SettingPanel.SetActive(false);
        SpecialAttackPanel.SetActive(true);

        //reset special attack scrollbar
        SpecialAttackscrollbar.value = 1f;

        // set scale to 0.5, and back to 1 in 0.2 sec
        specialAttackButton.transform.localScale = Vector3.one * 0.5f;
        specialAttackButton.transform.DOScale(Vector3.one, 0.2f);
    }

    private void OnSettingButtonClick()
    {
        SkillOrAbilityPanel.SetActive(false);
        SpecialAttackPanel.SetActive(false);
        SkillAbilityscrollbar.value = 1f;
        // set scale to 0.5, and back to 1 in 0.2 sec
        settingButton.transform.localScale = Vector3.one * 0.5f;
        settingButton.transform.DOScale(Vector3.one, 0.2f);
        SettingPanel.SetActive(true);
    }

    private void UpdateButtonActive()
    {
        // value < 0.7, ArrowButton setActive true
        arrowButton.gameObject.SetActive(SkillAbilityscrollbar.value < 0.7f);
    }

    private void OnEseButtonClick() 
    {
        specialAttackButton.transform.localScale = Vector3.one * 0.5f;
        specialAttackButton.transform.DOScale(Vector3.one, 0.2f);

        GamingUI.SetActive(false);
    }

    private void OnSkillOrAbilityButtonClick()
    {
        ShowSkillAbilityPanel();

        // set scale to 0.5, and back to 1 in 0.2 sec
        skillOrAbilityButton.transform.localScale = Vector3.one * 0.5f;
        skillOrAbilityButton.transform.DOScale(Vector3.one, 0.2f);

        if (SkillAbilityscrollbar.value < 0.6) { isIncreasing = true; }
        else { isDecreasing = true; }

    }
    
    private void UpdateCyanSelectedBlock()
    {
        if (SpecialAttackPanel.activeInHierarchy == true) 
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(275, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(365, cyanSelectedBlock.sizeDelta.y);
        }
        else if(SettingPanel.activeInHierarchy == true)
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(725, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(300, cyanSelectedBlock.sizeDelta.y);
        }
        else if (SkillAbilityscrollbar.value > 0.6f)
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(-138, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(185, cyanSelectedBlock.sizeDelta.y);
        }
        else
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(-290, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(145, cyanSelectedBlock.sizeDelta.y);
        }
    }
}
