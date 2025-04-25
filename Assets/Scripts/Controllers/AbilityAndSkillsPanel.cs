using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAndSkillsPanel : BasePanel
{
    public Scrollbar SkillAbilityscrollbar;
    private bool isIncreasing = false;
    private bool isDecreasing = false;
    public Button arrowButton;
    public Button skillOrAbilityButton;

    public GameObject SkillClickArea;
    public GameObject AbilityClickArea;
    public float scrollSpeed = 0.1f;
    protected override void Start()
    {
        base.Start();
        SkillAbilityscrollbar.value = 0;
    }

    // Update is called once per frame
    void Update()
    {

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


        if (this.gameObject.activeInHierarchy == false)
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

    }
    private void UpdateButtonActive()
    {
        // value < 0.7, ArrowButton setActive true
        arrowButton.gameObject.SetActive(SkillAbilityscrollbar.value < 0.7f);
    }
}
