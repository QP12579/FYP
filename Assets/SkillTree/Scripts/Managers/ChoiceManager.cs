using Skill;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager instance;
    public GameObject[] ChoicePrefab;
    private Player P1;
    private int level = 0;
    private void Awake()
    {
        level = P1.level;
        instance = this;
    }
    void Start()
    {
        for (int i = 0;i< ChoicePrefab.Length; i++)
        {
            ChoicePrefab[i].SetActive(false);
        }
    }
    public void AssignRandomChoice(Player player)
    {
        P1 = player;
        /*List<choices> rand = new List<choices>((choices[])Enum.GetValues(typeof(choices)));
        for (int i = 0; i < ChoicePrefab.Length; i++)
        {
            if(rand.Count <= 0)
            {
                ChoicePrefab[i].SetActive(false);
                Debug.LogWarning("No More choices Type.");
                break;
            }
            choices choose = GetRandomEnumValue(rand.ToArray());
            rand.Remove(choose);
            Choice choice = ChoicePrefab[i].GetComponent<Choice>();
            choice.RChoice(choose, P1);            
        }*/
        for (int i = 0; i < ChoicePrefab.Length; i++)
        {            
            Choice choice = ChoicePrefab[i].GetComponent<Choice>();
            choice.RChoice(choices.skill, P1);
        }

    }

    public choices AssignOtherChoice(choices now)
    {
        List<choices> rand = new List<choices>((choices[])Enum.GetValues(typeof(choices)));
        rand.Remove(now);
        return GetRandomEnumValue(rand.ToArray());
    }

    private static readonly System.Random random = new System.Random();
    public static T GetRandomEnumValue<T>(T[] enumValues) where T : Enum
    {
        int r = random.Next(0, enumValues.Length);
        return enumValues[r];
    }
}

public enum choices
{
    skill,
    item,
    weapon,
    trap
}