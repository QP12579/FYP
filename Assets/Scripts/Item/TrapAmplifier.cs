using UnityEngine;

public class TrapAmplifier : BaseItem
{
    public TrapAmplifier()
    {
        item = new Item
        {
            Type = ItemType.TrapAmplifier,
            name = "TrapAmplifier",
            description = "Amplify your Competitor's Trap"
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
