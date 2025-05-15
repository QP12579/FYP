using UnityEngine;

public class Coins : BaseItem
{
    public AudioClip CoinSound;

    public Coins()
    {
        item = new Item
        {
            Type = ItemType.Coin,
            name = "Coin",
            description = "Can buy items"
        };
    }
    public int CoinValue = 5;
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (CoinSound != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(CoinSound);
    }

    protected override void GetItem()
    {
        Bag.instance.AddCoins(CoinValue);
        base.GetItem();
    }
}
