using UnityEngine;

public class EnemyAmplifier : BaseItem
{
    public EnemyAmplifier()
    {
        item = new Item
        {
            Type = ItemType.EnemyAmplifier,
            name = "EnemyAmplifier",
            description = "Amplify your Competitor's Enemy"
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