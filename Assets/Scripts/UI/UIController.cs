using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UIController : NetworkBehaviour
{
    // Reference to player-specific UI canvas
    private GameObject playerUICanvas;

    // UI elements
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
    [HideInInspector]
    public UIPanelState state = UIPanelState.None;
    private UIPanelState oldState = UIPanelState.None;

    private void Start()
    {
        // Only proceed for the local player
        if(netIdentity != null)
        if (!isLocalPlayer) return;

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
        GamingUI.SetActive(false);
        SkillAbilityscrollbar.value = 0;

        // Add listeners to buttons
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

        Debug.Log($"UIController initialized for player {gameObject.name}");
    }

    private void FindUIElements()
    {
        if (playerUICanvas == null) return;

        // Find the main gaming UI container
        GamingUI = playerUICanvas.transform.Find("GamingUI")?.gameObject;
        if (GamingUI == null)
        {
            Debug.LogError($"GamingUI not found in canvas for player {gameObject.name}");
            return;
        }

        // Find all sub-panels
        SkillOrAbilityPanel = GamingUI.transform.Find("SkillOrAbilityPanel")?.gameObject;
        SpecialAttackPanel = GamingUI.transform.Find("SpecialAttackPanel")?.gameObject;
        SettingPanel = GamingUI.transform.Find("SettingPanel")?.gameObject;

        // Find the scrollbars
        SkillAbilityscrollbar = GamingUI.transform.Find("SkillAbilityscrollbar")?.GetComponent<Scrollbar>();
        SpecialAttackscrollbar = GamingUI.transform.Find("SpecialAttackscrollbar")?.GetComponent<Scrollbar>();

        // Find the cyan block
        cyanSelectedBlock = GamingUI.transform.Find("CyanSelectedBlock")?.GetComponent<RectTransform>();

        // Find all the buttons
        arrowButton = GamingUI.transform.Find("ArrowButton")?.GetComponent<Button>();
        skillOrAbilityButton = GamingUI.transform.Find("SkillOrAbilityButton")?.GetComponent<Button>();
        specialAttackButton = GamingUI.transform.Find("SpecialAttackButton")?.GetComponent<Button>();
        settingButton = GamingUI.transform.Find("SettingButton")?.GetComponent<Button>();
        EseButton = GamingUI.transform.Find("EseButton")?.GetComponent<Button>();

        // Find click areas
        SkillClickArea = GamingUI.transform.Find("SkillClickArea")?.gameObject;
        AbilityClickArea = GamingUI.transform.Find("AbilityClickArea")?.gameObject;

        // Check if any elements are missing
        if (SkillOrAbilityPanel == null || SpecialAttackPanel == null || SettingPanel == null ||
            SkillAbilityscrollbar == null || SpecialAttackscrollbar == null || cyanSelectedBlock == null ||
            arrowButton == null || skillOrAbilityButton == null || specialAttackButton == null ||
            settingButton == null || EseButton == null || SkillClickArea == null || AbilityClickArea == null)
        {
            Debug.LogError($"Some UI elements are missing for player {gameObject.name}");
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        // If UI elements aren't initialized yet, try again
        if (GamingUI == null || SkillOrAbilityPanel == null)
        {
            InitializePlayerUI();
            return;
        }

        // Handle key inputs for UI control
        if (Input.GetKeyDown(KeyCode.Tab)) { GamingUI.SetActive(true); state = oldState; }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(" Showing Panel ");
            ShowSkillPart();
            ShowSkillAbilityPanel();
            GamingUI.SetActive(true);
            state = UIPanelState.SkillAbilityPanel;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GamingUI.SetActive(false);
            oldState = state;
            state = UIPanelState.None;
        }

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

        state = UIPanelState.SkillAbilityPanel;
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

        state = UIPanelState.SpeacialATKPanel;

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

        state = UIPanelState.SettingPanel;
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
        else if (SettingPanel.activeInHierarchy == true)
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