using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private BaseItem itemPrefab; // 物品預製件
    [SerializeField] private int cost = 10;
    [SerializeField] private TextMeshPro costText;
    [SerializeField] private KeyCode purchaseKey = KeyCode.R;
    [SerializeField] private GameObject purchasePrompt; // 購買提示UI

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

        costText.text = cost.ToString("C");

        // 隱藏購買提示
        if (purchasePrompt != null) purchasePrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(purchaseKey))
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
            Debug.Log("背包已滿!");
            return;
        }

        if (Bag.instance.coins < cost)
        {
            Debug.Log("金幣不足!");
            return;
        }

        // 執行購買
        Bag.instance.coins -= cost;
        Bag.instance.AddItem(itemPrefab.item);
        Bag.instance.UpdateBagUI();

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