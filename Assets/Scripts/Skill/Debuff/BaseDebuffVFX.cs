using UnityEngine;

public class BaseDebuffVFX : MonoBehaviour
{
    private void Update()
    {
        if (!PlayerBuffSystem.instance.hasActiveDebuffs)
            Destroy(gameObject);
    }
}
