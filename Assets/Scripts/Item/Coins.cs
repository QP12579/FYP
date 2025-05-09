using UnityEngine;

public class Coins : BaseItem
{
    public Coins()
    {
        item = new Item
        {
            Type = ItemType.Coin,
            name = "Coin",
            description = "Can buy items"
        };
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void GetItem()
    {
        base.GetItem();
        Bag.instance.AddCoins();
    }
}
