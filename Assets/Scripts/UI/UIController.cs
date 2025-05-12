using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIController : NetworkBehaviour
{
    // Reference to player-specific UI canvas
    private GameObject playerUICanvas;

    // UI elements
    [Header("Panel")]
    public BasePanel GamingUI;
    [SerializeField] private BasePanel SkillOrAbilityPanel;
    [SerializeField] private BasePanel SpecialAttackPanel;
    [SerializeField] private BasePanel SettingPanel;
    public SkillPanel skillPanel;

    [Header("Scrollbar")]
    public Scrollbar SkillAbilityscrollbar;
    public Scrollbar SpecialAttackscrollbar;

    public float scrollSpeed = 0.1f;

    [Header("CyanSelectedBlock")]
    public RectTransform cyanSelectedBlock;
    private bool isIncreasing = false;
    private bool isDecreasing = false;

    [Header("Button")]
    public Button arrowButton;
    public Button skillOrAbilityButton;
    public Button specialAttackButton;
    public Button settingButton;
    public Button EseButton;
    public Button TabButton;
    public Button TButton;

    [Header("Click Area")]
    public Button SkillClickArea;
    public Button AbilityClickArea;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip buttonClickSFX;

    [HideInInspector]
    public UIPanelState state = UIPanelState.None;
    private UIPanelState oldState = UIPanelState.None;

    private void Start()
    {
        // Only proceed for the local player
        if (netIdentity != null)
            if (!isLocalPlayer) {
                Debug.Log("Not local player");
                return; }

        Debug.Log($"Starting UIController for player {gameObject.name}");

        // Find or initialize player-specific UI
        InitializePlayerUI();
    }

    private void InitializePlayerUI()
    {
        // Look for player-specific UI canvas
        string canvasName = "PlayerUI_" + netId.ToString();
        playerUICanvas = GameObject.Find(canvasName);

        if (playerUICanvas == null)
        {
            Debug.LogWarning($"Player UI canvas '{canvasName}' not found for player {gameObject.name}, trying again soon...");
            // Try again after a short delay
            Invoke("InitializePlayerUI", 0.2f);
            return;
        }

        // Find all UI components within this player's canvas
        FindUIElements();

        // Set initial UI state
        SkillOrAbilityPanel.OpenPanel();
        GamingUI.ClosePanel();
        SkillAbilityscrollbar.value = 0;

        UpdateButtonActive();

        SpecialAttackPanel.ClosePanel();
        SettingPanel.ClosePanel();

        SkillClickArea.gameObject.SetActive(false);
        AbilityClickArea.gameObject.SetActive(false);

        Debug.Log($"UIController initialized for player {gameObject.name}");
    }

    private void FindUIElements()
    {
        if (playerUICanvas == null) playerUICanvas = GameObject.Find("UISavedPrefab");
        if (playerUICanvas == null) return;

        // Find the main gaming UI container
        GamingUI = playerUICanvas.transform.Find("GamingUI")?.GetComponent<BasePanel>();
        if (GamingUI == null)
        {
            Debug.LogError($"GamingUI not found in canvas for player {gameObject.name}");
            return;
        }
        if (SkillOrAbilityPanel == null || SpecialAttackPanel == null || SettingPanel == null ||
    SkillAbilityscrollbar == null || SpecialAttackscrollbar == null || cyanSelectedBlock == null ||
    arrowButton == null || skillOrAbilityButton == null || specialAttackButton == null ||
    settingButton == null || EseButton == null || SkillClickArea == null || AbilityClickArea == null)
        {
            // Find all sub-panels
            SkillOrAbilityPanel = GamingUI.transform.Find("SkillOrAbilityPanel")?.GetComponent<BasePanel>();
            SpecialAttackPanel = GamingUI.transform.Find("SpecialAttackPanel")?.GetComponent<BasePanel>();
            SettingPanel = GamingUI.transform.Find("SettingPanel")?.GetComponent<BasePanel>();

            // Find the scrollbars
            SkillAbilityscrollbar = GamingUI.transform.Find("SkillAbilityscrollbar")?.GetComponent<Scrollbar>();
            SpecialAttackscrollbar = GamingUI.transform.Find("SpecialAttackscrollbar")?.GetComponent<Scrollbar>();

            // Find the cyan block
            cyanSelectedBlock = GamingUI.transform.Find("CyanSelectedBlock")?.GetComponent<RectTransform>();

            // Find all the buttons
            arrowButton = GamingUI.transform.Find("ArrowButton")?.GetComponent<Button>();
            TButton = playerUICanvas.transform.Find("T")?.GetComponent<Button>();
            TabButton = playerUICanvas.transform.Find("Tab")?.GetComponent<Button>();
            skillOrAbilityButton = GamingUI.transform.Find("SkillOrAbilityButton")?.GetComponent<Button>();
            specialAttackButton = GamingUI.transform.Find("SpecialAttackButton")?.GetComponent<Button>();
            settingButton = GamingUI.transform.Find("SettingButton")?.GetComponent<Button>();
            EseButton = GamingUI.transform.Find("EseButton")?.GetComponent<Button>();

            // Find click areas
            SkillClickArea = GamingUI.transform.Find("SkillClickArea")?.GetComponent<Button>();
            AbilityClickArea = GamingUI.transform.Find("AbilityClickArea")?.GetComponent<Button>();

            // Check if any elements are missing
            if (SkillOrAbilityPanel == null || SpecialAttackPanel == null || SettingPanel == null ||
                SkillAbilityscrollbar == null || SpecialAttackscrollbar == null || cyanSelectedBlock == null ||
                arrowButton == null || skillOrAbilityButton == null || specialAttackButton == null ||
                settingButton == null || EseButton == null || SkillClickArea == null || AbilityClickArea == null)
            {
                Debug.LogError($"Some UI elements are missing for player {gameObject.name}");
            }
        }
    }
    private void Update()
    {        
        if (netIdentity != null) if (!isLocalPlayer) return;

        // If UI elements aren't initialized yet, try again
        if (GamingUI == null || SkillOrAbilityPanel == null)
        {
            InitializePlayerUI();
            return;
        }

        // Handle key inputs for UI control
        if (Input.GetKeyDown(KeyCode.Tab)) { OnTabButtonClick(TabButton); }

        if (Input.GetKeyDown(KeyCode.T))
        {
            OnTButtonClick(TButton);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEseButtonClick(EseButton);
        }

        if (state == UIPanelState.None) return;
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

        // control button active
        UpdateButtonActive();

        // update CyanSelectedBlock position and scale according to the scrollbar value
    }

    public void ShowSkillAbilityPanel()
    {
        SkillOrAbilityPanel.OpenPanel();
        SpecialAttackPanel.ClosePanel();
        SettingPanel.ClosePanel();

        skillOrAbilityButton.interactable = true;
        SkillClickArea.gameObject.SetActive(false);
        AbilityClickArea.gameObject.SetActive(false);

        skillPanel.OpenEquippedButton();

        state = UIPanelState.SkillAbilityPanel;
    }

    public void ShowSkillPart() { SkillAbilityscrollbar.value = 0f; }

    public void ShowAbilityPart() { SkillAbilityscrollbar.value = 1f; }
    private void OnArrowButtonClick()
    {
        isIncreasing = true;
    }

    public void OnSpecialAttackButtonClick(Button but)
    {
        SkillOrAbilityPanel.ClosePanel();
        SettingPanel.ClosePanel();
        SpecialAttackPanel.OpenPanel();
        SkillAbilityButInteratableFalse();
        state = UIPanelState.SpeacialATKPanel;

        //reset special attack scrollbar
        SpecialAttackscrollbar.value = 1f;

        OnClickButtonDOScaleVisual(but);
    }

    public void OnSettingButtonClick(Button but)
    {
        SkillOrAbilityPanel.ClosePanel();
        SpecialAttackPanel.ClosePanel();
        SkillAbilityscrollbar.value = 1f;
        SkillAbilityButInteratableFalse();
        state = UIPanelState.SettingPanel;
        OnClickButtonDOScaleVisual(but);

        SettingPanel.OpenPanel();
    }

    private void SkillAbilityButInteratableFalse()
    {
        skillOrAbilityButton.interactable = false;
        SkillClickArea.gameObject.SetActive(true);
        AbilityClickArea.gameObject.SetActive(true);
    }

    private void UpdateButtonActive()
    {
        // value < 0.7, ArrowButton setActive true
        arrowButton.gameObject.SetActive(SkillAbilityscrollbar.value < 0.7f);
        UpdateCyanSelectedBlock();
    }

    public void OnTabButtonClick(Button but)
    {
        OnClickButtonDOScaleVisual(but);
        GamingUI.OpenPanel();
        state = oldState;
    }

    public void OnTButtonClick(Button but)
    {
        OnClickButtonDOScaleVisual(but);

        Debug.Log(" Showing Panel ");
        ShowSkillPart();
        ShowSkillAbilityPanel();
        GamingUI.OpenPanel();
        state = UIPanelState.SkillAbilityPanel;
    }

    public void OnEseButtonClick(Button but)
    {
        OnClickButtonDOScaleVisual(but);

        oldState = state;
        state = UIPanelState.None;
        GamingUI.ClosePanel();
    }

    public void OnSkillOrAbilityButtonClick(Button but)
    {
        ShowSkillAbilityPanel();

        OnClickButtonDOScaleVisual(but);

        if (SkillAbilityscrollbar.value < 0.6) { isIncreasing = true; }
        else { isDecreasing = true; }

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
        }
        else if (state == UIPanelState.SettingPanel)
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

public enum UIPanelState
{
    None,
    SkillAbilityPanel,
    SpeacialATKPanel,
    SettingPanel
}