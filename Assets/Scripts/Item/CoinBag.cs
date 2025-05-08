using UnityEngine;

public class CoinBag : BaseItem
{
    public CoinBag()
    {
        item = new Item
        {
            Type = ItemType.CoinBag,
            name = "CoinBag",
            description = "You are rich!"
        };
        getCoins = 15;
    }

    public int getCoins = 15;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void GetItem()
    {
        Bag.instance.AddCoins(getCoins);
        base.GetItem();
    }
}
