using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Skill
{
    public class Choice : MonoBehaviour
    {
        private string dataPath = ("Assets/SkillTree/Data/");
        public choices c;
        public int level = 0;
        private int r = 0;
        private Weapon Weapon;
        private Item Item;
        private Skill Skill;
        private Trap Trap;
        [SerializeField] private TextMeshProUGUI choiceName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Image icon;
        private WeaponData chosenWeapon;
        private ItemData chosenItem;
        private SkillData chosenSkill;
        private TrapData chosenTrap;
        private Sprite defaultImage;
        public Player player;

        private void Start()
        {
            defaultImage = icon.sprite;
           // Weapon = AssetDatabase.LoadAssetAtPath<Weapon>(dataPath + "Weapon.asset"); // it can automatically find the path of the asset file and assign in inspector
           // Item = AssetDatabase.LoadAssetAtPath<Item>(dataPath + "Item.asset");
           //Skill = AssetDatabase.LoadAssetAtPath<Skill>(dataPath + "Skill.asset");
           // Trap = AssetDatabase.LoadAssetAtPath<Trap>(dataPath + "Trap.asset");
        }
        public void RChoice(choices type, Player p)
        {
            player = p;
            gameObject.SetActive(true);
            r = 0;
            c = type;
            level = player.level;

            chosenItem = null;
            chosenSkill = null;
            chosenTrap = null;
            chosenWeapon = null;

            switch (c)
            {
                case choices.skill:
                    chosenSkill = SkillC();
                    break;
                case choices.item:
                    chosenItem = ItemC();
                    break;
                case choices.trap:
                    chosenTrap = TrapC();
                    break;
            }
        }

        public void OnChosenEnter()
        {
            LevelManager.instance.choicePanel.SetActive(false);
            switch (c)
            {
                case choices.skill:
                    if (chosenSkill != null) player.Skills.Add(chosenSkill);
                    break;
                case choices.item:
                    /*ItemData itemData = chosenItem;
                    foreach (var hadItemData in Player.instance.Items)
                    {
                        if (hadItemData == itemData)
                        {
                            hadItemData.number++;
                            chosenItem = null;
                            break;
                        }
                    }
                    if (chosenItem != null)
                    {
                        chosenItem.number = 1;
                        Player.instance.Items.Add(chosenItem);
                    }*/
                    break;
               
                case choices.trap:
                    /*TrapData trapData = chosenTrap;
                    foreach (var hadTrapData in Player.instance.Traps)
                    {
                        if (hadTrapData == trapData)
                        {
                            hadTrapData.number++;
                            chosenTrap = null;
                            break;
                        }
                    }
                    if (chosenTrap != null)
                    {
                        chosenTrap.number = 1;
                        Player.instance.Traps.Add(chosenTrap);
                    }*/
                    break;
            }
        }

        private static readonly System.Random random = new System.Random();
        private ItemData ItemC()
        {
            r = Random.Range(0, Item.Items.Count);
            ItemData canChooseItem = Item.Items[r];
            choiceName.text = canChooseItem.itemName;
            description.text = canChooseItem.description;
            icon.sprite = canChooseItem.icon;
            return canChooseItem;
        }

        
        private SkillData SkillC()
        {
            bool haveskill = false;
            List<SkillT> countT = new List<SkillT>((SkillT[])System.Enum.GetValues(typeof(SkillT)));
            do
            {
                r = random.Next(0, countT.Count);
                List<SkillData> canChooseSkills = new List<SkillData>();
                List<SkillData> playerSkills = new List<SkillData>();
                SkillT skillT = countT[r];
                countT.Remove(skillT);
                Debug.Log("skillT: "+skillT);
                foreach (var skillData in Skill.skillKeeper) //pick the type which is same from Database
                {
                    foreach (var s in player.Skills) //pick the type which is same from player
                    {
                        if (s.skillType == skillT)
                            playerSkills.Add(s);
                    }
                    if (skillData.skillType == skillT && skillData.skillLevel <= level) //when player's level higher than the skill data's level, it can be add
                    {
                        canChooseSkills.Add(skillData);
                    }
                }

                foreach (var hadSkillData in playerSkills)
                {
                    if (canChooseSkills.Count <= 0) break;
                    canChooseSkills.Sort((x, y) => { return x.skillLevel.CompareTo(y.skillLevel); });
                    if (hadSkillData == canChooseSkills[0])
                        canChooseSkills.Remove(hadSkillData);
                }

                if (canChooseSkills.Count > 0)
                {
                    canChooseSkills.Sort((x, y) => { return x.skillLevel.CompareTo(y.skillLevel); });
                    SkillData canChooseSkill = canChooseSkills[0]; //take the first one
                    icon.sprite = canChooseSkill.icon;
                    choiceName.text = canChooseSkill.skillName;
                    description.text = canChooseSkill.description;
                    haveskill = true;
                    return canChooseSkill;
                }
                else
                {
                    icon.sprite = defaultImage;
                    choiceName.text = "Your Skills";
                    description.text = "No need to teach you now. You already have a lot.";
                }
            } while (!haveskill && countT.Count > 0);
            Debug.LogWarning("None Skill.");
            gameObject.SetActive(false);
            return null;
        }

        private TrapData TrapC()
        {
            r = Random.Range(0, Trap.trapData.Count);
            TrapData trapData = Trap.trapData[r];
            icon.sprite = trapData.icon;
            choiceName.text = trapData.trapName;
            description.text = trapData.description;
            return trapData;
        }
    }

}