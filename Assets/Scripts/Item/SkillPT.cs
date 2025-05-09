using UnityEngine;

public class SkillPT : BaseItem
{
    public SkillPT()
    {
        item = new Item
        {
            Type = ItemType.SkillPT,
            name = "SkillPoint",
            description = "Can Buy your Skill."
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
        Bag.instance.AddSkillPoint(getPoint);
        base.GetItem();
    }
}