using UnityEngine;

public class BananaPeel : BaseItem
{
    public BananaPeel()
    {
        item = new Item
        {
            Type = ItemType.Banana,
            name = "BananaPeel",
            description = "Somethings will happen to your Competitor."
        };
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void GetItem()
    {
        Bag.instance.AddItem(item);
        base.GetItem();
    }
}
