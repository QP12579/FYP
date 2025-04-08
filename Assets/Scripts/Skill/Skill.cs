using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public string name { get; set; }
    public string description { get; set; }
    public Sprite UIsprite { get; set; }
    public abstract GameObject SkillPrefab { get; set; } 
}