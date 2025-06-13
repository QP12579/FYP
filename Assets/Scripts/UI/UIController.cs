using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header( "Panel ")]
    public GameObject GamingUI;

    public GameObject SkillOrAbilityPanel;
    public GameObject SpecialAttackPanel;
    public GameObject SettingPanel;

    [Header(" Scrollbar ")]
    public Scrollbar SkillAbilityscrollbar;
    public Scrollbar SpecialAttackscrollbar;
    public float scrollSpeed = 1f;

    [Header(" CyanSelectedBlock ")]
    public RectTransform cyanSelectedBlock;
    private bool isIncreasing = false;
    private bool isDecreasing = false;

    [Header(" Button ")]
    public Button arrowButton;
    public Button skillOrAbilityButton;
    public Button specialAttackButton;
    public Button settingButton;
    public Button EseButton;
    public Button TabButton;
    public Button TButton;

    [Header(" Click Area ")]
    public Button SkillClickArea;
    public Button AbilityClickArea;

    [Header(" Audio Clip ")]
    [SerializeField] private AudioClip buttonClickSFX;

    public UIPanelState state = UIPanelState.None;
    private UIPanelState oldState = UIPanelState.None;

    [Header("KeyCode")]
    [SerializeField] private InputActionAsset inputActions;

    private void Start()
    {
        SkillAbilityscrollbar.value = 0;

        UpdateButtonActive();

        SkillOrAbilityPanel.SetActive(true);
        SpecialAttackPanel.SetActive(false);
        SettingPanel.SetActive(false);
        SkillClickArea.gameObject.SetActive(false);
        AbilityClickArea.gameObject.SetActive(false);
        GamingUI.SetActive(false);
    }

    private void Update()
    {
        if (inputActions.FindAction(Constraints.InputKey.Tab).triggered)
        {
            OnTabButtonClick(TabButton);
        }
        
        if (inputActions.FindAction(Constraints.InputKey.T).triggered)
        {
            OnTButtonClick(TButton);
        }

        if (inputActions.FindAction(Constraints.InputKey.ESC).triggered)
        {
            OnEseButtonClick(EseButton);
        }

        if (state == UIPanelState.None) return;
        // get mouse scroll value
        float scrollDelta = inputActions.FindAction(Constraints.InputKey.Aim).ReadValue<Vector2>().y;

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

        skillOrAbilityButton.interactable = true;
        SkillClickArea.gameObject.SetActive(false);
        AbilityClickArea.gameObject.SetActive(false);

        state = UIPanelState.SkillAbilityPanel;

        UpdateCyanSelectedBlock();
    }

    public void ShowSkillPart() { SkillAbilityscrollbar.value = 0f; }

    public void ShowAbilityPart() { SkillAbilityscrollbar.value = 1f; }
    public void OnArrowButtonClick()
    {
        isIncreasing = true;
        UpdateButtonActive();
    }

    public void OnSpecialAttackButtonClick(Button but)
    {
        SkillOrAbilityPanel.SetActive(false);
        SettingPanel.SetActive(false);
        SpecialAttackPanel.SetActive(true);

        SkillAbilityButInteratableFalse();
        state = UIPanelState.SpeacialATKPanel;

        //reset special attack scrollbar
        SpecialAttackscrollbar.value = 1f;

        OnClickButtonDOScaleVisual(but);
    }

    public void OnSettingButtonClick(Button but)
    {
        SkillOrAbilityPanel.SetActive(false);
        SpecialAttackPanel.SetActive(false);
        SkillAbilityscrollbar.value = 1f;

        SkillAbilityButInteratableFalse();
        state = UIPanelState.SettingPanel;
        // set scale to 0.5, and back to 1 in 0.2 sec
        OnClickButtonDOScaleVisual(but);

        SettingPanel.SetActive(true);
    }

    private void UpdateButtonActive()
    {
        // value < 0.7, ArrowButton setActive true
        arrowButton.gameObject.SetActive(SkillAbilityscrollbar.value < 0.7f);
        UpdateCyanSelectedBlock();
    }

    private void SkillAbilityButInteratableFalse()
    {
        skillOrAbilityButton.interactable = false;
        SkillClickArea.gameObject.SetActive(true);
        AbilityClickArea.gameObject.SetActive(true);
    }

    public void OnTabButtonClick(Button but)
    {
        GamingUI.SetActive(true);
        state = oldState;
        OnClickButtonDOScaleVisual(but);
    }

    public void OnTButtonClick(Button but)
    {
        Debug.Log(" Showing Panel ");
        GamingUI.SetActive(true);
        ShowSkillAbilityPanel();
        ShowSkillPart();
        state = UIPanelState.SkillAbilityPanel;

        OnClickButtonDOScaleVisual(but);
    }

    public void OnEseButtonClick(Button but)
    {
        oldState = state;
        state = UIPanelState.None;
        GamingUI.SetActive(false);

        OnClickButtonDOScaleVisual(but);
    }

    public void OnSkillOrAbilityButtonClick(Button but)
    {
        ShowSkillAbilityPanel();

        if (SkillAbilityscrollbar.value < 0.6) { isIncreasing = true; }
        else { isDecreasing = true; }
        // set scale to 0.5, and back to 1 in 0.2 sec
        OnClickButtonDOScaleVisual(but);
    }

    private void OnClickButtonDOScaleVisual(Button but)
    {
        // set scale to 0.5, and back to 1 in 0.2 sec
        but.transform.localScale = Vector3.one * 0.5f;
        but.transform.DOScale(Vector3.one, 0.2f);

        UpdateCyanSelectedBlock();
        if (buttonClickSFX != null & SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(buttonClickSFX);
        }
    }

    private void UpdateCyanSelectedBlock()
    {
        if (state == UIPanelState.SpeacialATKPanel)
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(275, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(365, cyanSelectedBlock.sizeDelta.y);
            SkillAbilityButInteratableFalse();
        }
        else if (state == UIPanelState.SettingPanel)
        {
            cyanSelectedBlock.anchoredPosition = new Vector2(725, 8);
            cyanSelectedBlock.sizeDelta = new Vector2(300, cyanSelectedBlock.sizeDelta.y);
            SkillAbilityButInteratableFalse();
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

public enum UIPanelState
{
    None,
    SkillAbilityPanel,
    SpeacialATKPanel,
    SettingPanel
}