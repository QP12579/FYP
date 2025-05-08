using UnityEngine;

public class Broom : BaseItem
{
    public Broom()
    {
        item = new Item
        {
            Type = ItemType.Broom,
            name = "Broom",
            description = "Clear All your Debuff state."
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
