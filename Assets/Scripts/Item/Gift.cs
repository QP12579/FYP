using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gift : BaseItem
{
    public Gift()
    {
        item = new Item
        {
            Type = ItemType.Gift,
            name = "Gift",
            description = "Dear Competitor, \nHere is A Gift to you!"
        };
    }
    [Header("Prefabs")]
    [SerializeField] private GameObject buffPrefab;
    [SerializeField] private GameObject debuffPrefab;
    [SerializeField] private GameObject EnemyPrefab;

    [Header("Persentage(%)")]
    [Range(0, 100)] public float buffChance = 60f;
    [Range(0, 100)] public float debuffChance = 30f;
    [Range(0, 100)] public float enemyChance = 10f;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void GetItem()
    {
        Bag.instance.AddItem(item);
        base.GetItem();
    }

    private void OpenGift()
    {
        //( Buff60% / Debuff30% / Enemy10% )
        float randomValue = Random.Range(0f, buffChance + debuffChance + enemyChance);
        if(randomValue < buffChance)
        {
            // Buff
            if(buffPrefab!= null)
            PlayerSkillController.instance.AddRandomBuff(buffPrefab, 1);
        }
        else if(randomValue < buffChance + debuffChance)
        {
            //Debuff
            if(debuffPrefab!= null)
            PlayerSkillController.instance.AddRandomDebuff(debuffPrefab, 1);
        }
        else
        {
            //Enemy
            if (EnemyPrefab != null){
                GameObject HealSkill = Instantiate(
                    EnemyPrefab,
                    transform.position,
                    transform.rotation
                );
            }
            else
            {
                Debug.Log("No Enemy Prefab in Gift.");
            }
        }
    }
    
}
