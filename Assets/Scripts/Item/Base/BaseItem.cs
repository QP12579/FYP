using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public Sprite icon;
    public ItemType Type;
    public string name;
    public string description;
}

public class BaseItem : MonoBehaviour
{
    public Item item;

    [SerializeField] protected AudioClip GetSound;
    protected virtual void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !Bag.instance.isItemBagFull)
        {
            if (GetSound != null)
                SoundManager.instance.PlaySFX(GetSound);
            GetItem();
        }
    }

    protected virtual void GetItem()
    {
        Destroy(gameObject);
    }
}

public enum ItemType
{
    Coin,
    HPFill,
    CoinBag,
    SkillPT,
    BasicPT,
    Gift,
    Banana,
    Broom,
    TrapAmplifier,
    EnemyAmplifier
}