using UnityEngine;

public class BaseBuff : MonoBehaviour
{
    private void Update()
    {
        if (!PlayerBuffSystem.instance.hasActiveBuffs)
            Destroy(gameObject);
    }
}
