using UnityEngine;

public class BaseBuff : MonoBehaviour
{
    private Buff buff = new Buff();
    public void ActiveBuff()
    {
        PlayerBuffSystem.instance.AddBuff(buff.type);
    }

    public void AssignBuffType(BuffType type)
    {
        buff.type = type;
        ActiveBuff();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
            ActiveBuff();
    }
}
