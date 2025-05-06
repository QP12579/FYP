using UnityEngine;

public class BaseBuff : MonoBehaviour
{
    private Buff buff = new Buff();
    public void ActiveBuff()
    {
        PlayerBuffSystem.instance.AddBuff(buff.type);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
            ActiveBuff();
    }
}
