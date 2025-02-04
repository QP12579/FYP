using Skill;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int MaxHP = 100;
    public int HP = 100;
    public Slider HPSlider;
    public int MaxMP = 50;
    public int MP = 50;
    public Slider MPSlider;
    public int level = 1;
    public TextMeshProUGUI levelText;
    public List<WeaponData> Weapons = null;
    public List<SkillData> Skills = null;
    public Transform weaponPosi;
    [SerializeField] private LayerMask groundMask;
    private PlayerMovement movement;
    private Animator animator;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    public Player()
    {
        level = 1;
        HP = MaxHP;
        MP = MaxMP;
    }

    public void UpdatePlayerUIInfo()
    {
        HPSlider.value = HP;
        MPSlider.value = MP;
        HPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = HP.ToString() + "/" + MaxHP.ToString();
        MPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = MP.ToString() + "/" + MaxMP.ToString();
        levelText.text = level.ToString();
    }

    public void GetHurt(int hurt)
    {
        HP -= hurt;
        UpdatePlayerUIInfo();
        animator.SetTrigger("Hurt");
        if (HP <= 0) Die();
    }

    public void Die()
    {
        Destroy(gameObject, 1f);
    }
/* // LevelUP
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Gate"))
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        //LevelManager.instance.LevelUp();
    }*/

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SpawnVFX();
            animator.SetTrigger("Attack");
        }
    }

    void SpawnVFX()
    {
        SkillData skillData = new SkillData();
        skillData = Skills[Skills.Count - 1]; // use the new Skill
        MP -= skillData.skillLevel;
        UpdatePlayerUIInfo();

        // Create VFX
        GameObject vfx = Instantiate(skillData.skillPrefab, transform.position, Quaternion.identity);
        Bomb newBomb = vfx.GetComponent<Bomb>();
        newBomb.damage = skillData.DamageOrHeal;
        newBomb.groundMask = groundMask;
        newBomb.SetTrapTypeBomb(weaponPosi);

        Destroy(vfx, level * 2f);
    }

    IEnumerator vfxCountTime(float time, GameObject gameObject)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}
