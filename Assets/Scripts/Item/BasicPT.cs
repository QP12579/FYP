using UnityEngine;

public class BasicPT : BaseItem
{
    public BasicPT()
    {
        item = new Item
        {
            Type = ItemType.BasicPT,
            name = "BasicPoint",
            description = "Can Upgrade your Ability."
        };
        getPoint = 1;
    }
    public int getPoint = 1;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void GetItem()
    {
        Bag.instance.AddBasePoint(getPoint);
        base.GetItem();
    }
}
