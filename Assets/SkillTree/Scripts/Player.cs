using Skill;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public int Health = 100;
    public int level = 1;
    public List<WeaponData> Weapons = null;
    public List<ItemData> Items = null;
    public List<SkillData> Skills = null;
    public List<TrapData> Traps = null;
    public Transform WeaponPosi = null;

    // Start is called before the first frame update
    public Player()
    {
        if (instance == null)
        instance = this;
        level = 0;
        Health = 100;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("levelUp"))
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        LevelManager.instance.LevelUp();
    }
}
