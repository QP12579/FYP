using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] public BaseItem itemPrefab; // 物品預製件
    public int cost = 10;
    [SerializeField] private TextMeshPro costText;
    // [SerializeField] private KeyCode purchaseKey = KeyCode.R;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject purchasePrompt; // 購買提示UI
    [SerializeField] private AudioClip Buy_SFX;
    [SerializeField] private AudioClip BuyFail_SFX;
    [SerializeField] private AudioClip BagFull_SFX;

    private bool playerInRange = false;

    private void Start()
    {
        if (itemPrefab == null)
            itemPrefab = GetComponentInChildren<BaseItem>();

        // 禁用物品的觸發碰撞體
        if (itemPrefab != null)
        {
            Collider itemCollider = itemPrefab.GetComponent<Collider>();
            if (itemCollider != null) itemCollider.isTrigger = false;
        }

        costText.text = "$ " + cost.ToString();

        // 隱藏購買提示
        if (purchasePrompt != null) purchasePrompt.SetActive(false);
    }

    public void OnPurchaseButtonClicked()
    {
        if (playerInRange)
        {
            AttemptPurchase();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (purchasePrompt != null) purchasePrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (purchasePrompt != null) purchasePrompt.SetActive(false);
        }
    }

    private void AttemptPurchase()
    {
        // 檢查條件
        if (Bag.instance.isItemBagFull)
        {
            if (BagFull_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(BagFull_SFX);
            Debug.Log("背包已滿!");
            return;
        }

        if (Bag.instance.coins < cost)
        {
            if (BuyFail_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(BuyFail_SFX);
            Debug.Log("金幣不足!");
            return;
        }

        // 執行購買
        Bag.instance.coins -= cost;
        Bag.instance.AddItem(itemPrefab.item);
        Bag.instance.UpdateBagUI();

        if (Buy_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(Buy_SFX);
        Debug.Log($"購買了 {itemPrefab.item.name}!");
    }

    // 可選: 在編輯器中更新價格顯示
    private void OnValidate()
    {
        if (costText != null)
        {
            costText.text = "$ " + cost.ToString();
        }
    }
}