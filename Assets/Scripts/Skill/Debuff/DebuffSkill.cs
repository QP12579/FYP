using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffSkill : MonoBehaviour
{
    public DeBuffType DebuffType;
    public float debuffP = 5f;
    public float time = 5f;

    public void DebuffTarget(IDebuffable target)
    {
        target.DeBuff(DebuffType, time, debuffP);
    }
}
