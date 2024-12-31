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
            Weapon = AssetDatabase.LoadAssetAtPath<Weapon>(dataPath + "Weapon.asset"); // it can automatically find the path of the asset file and assign in inspector
            Item = AssetDatabase.LoadAssetAtPath<Item>(dataPath + "Item.asset");
            Skill = AssetDatabase.LoadAssetAtPath<Skill>(dataPath + "Skill.asset");
            Trap = AssetDatabase.LoadAssetAtPath<Trap>(dataPath + "Trap.asset");
        }
        public void RChoice(choices type)
        {
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
                case choices.weapon:
                    chosenWeapon = WeaponC();
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
                case choices.weapon:
                    WeaponData weaponData = chosenWeapon;
                    foreach(var hadWeapon in player.Weapons)
                    {
                        if(hadWeapon == weaponData)
                        {
                            hadWeapon.damage += 5; // upgrate weapon damage
                            chosenWeapon = null;
                            break;
                        }
                    }
                    if (chosenWeapon != null) player.Weapons.Add(chosenWeapon);
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

        private WeaponData WeaponC()
        {
            bool haveweapon = false;
            List<WeaponType> countT = new List<WeaponType>((WeaponType[])System.Enum.GetValues(typeof(WeaponType)));
            do
            {
                r = random.Next(0, countT.Count);
                List<WeaponData> canChooseWeapons = new List<WeaponData>();
                List<WeaponData> playerWeapons = new List<WeaponData>();
                WeaponType weaponT = countT[r];
                countT.Remove(weaponT);
                Debug.Log("weaponT:"+weaponT);
                //Randomly get a type in Weapon.data_weapon
                foreach (var weaponData in Weapon.data_weapon)
                {
                    foreach (var s in player.Weapons) //pick the type which is same from player
                    {
                        if (s.type == weaponT)
                            playerWeapons.Add(s);
                    }
                    if (weaponData.type == weaponT && weaponData.Level <= level) //when player's level higher than the weapon data's level, it can be add
                    {
                        canChooseWeapons.Add(weaponData);
                    }
                }
                //get player's had weapons
                foreach (var hadWeapon in playerWeapons)
                {
                    if (canChooseWeapons.Count <= 0) break;
                    canChooseWeapons.Sort((x, y) => { return x.Level.CompareTo(y.Level); });
                    if (hadWeapon == canChooseWeapons[0] && canChooseWeapons.Count != 1)
                        canChooseWeapons.Remove(hadWeapon);
                    if (hadWeapon == canChooseWeapons[0] && canChooseWeapons.Count==1) // check if this is the last weapon, just upgrate it.
                    {
                        WeaponData canChooseWeapon = canChooseWeapons[0];
                        choiceName.text = canChooseWeapon.weaponName;
                        description.text = "Upgrade " + choiceName.text;
                        icon.sprite = canChooseWeapon.icon;
                        return canChooseWeapon;
                    }
                }
                if (canChooseWeapons.Count > 0)
                {
                    canChooseWeapons.Sort((x, y) => { return x.Level.CompareTo(y.Level); });
                    haveweapon = true;
                    WeaponData canChooseWeapon = canChooseWeapons[0];
                    choiceName.text = canChooseWeapon.weaponName;
                    description.text = canChooseWeapon.description;
                    icon.sprite = canChooseWeapon.icon;
                    return canChooseWeapon;
                }
                else
                {
                    icon.sprite = defaultImage;
                    choiceName.text = "Weapons";
                    description.text = "Nothing can give you now. You already have a lot.";
                }
            } while (!haveweapon && countT.Count > 0);
            Debug.LogWarning("None Weapon.");
            gameObject.SetActive(false);
            return null;
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