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
    public Transform VFXPosiR = null, VFXPosiL = null;
    private PlayerMovement movement;
    private Animator animator;
    private float x = 0;

    private void Start()
    {
        movement = gameObject.GetComponent<PlayerMovement>();
        animator = movement.anim;
        x = VFXPosiR.transform.position.x;
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
        gameObject.GetComponent<Animator>().SetTrigger("Hurt");
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnVFX();
        }
    }

    void SpawnVFX()
    {
        SkillData skillData = new SkillData();
        skillData = Skills[Skills.Count - 1]; // use the new one
        MP -= skillData.skillLevel;
        UpdatePlayerUIInfo();
        GameObject vfx = Instantiate(skillData.skillPrefab, Vector3.zero, Quaternion.identity);

        if (vfx.GetComponent<Bomb>() != null) //if vfx is Sprite vfx
        {
            vfx.GetComponent<Bomb>().damage = skillData.DamageOrHeal;
            vfx.transform.localScale = new Vector2(1.2f, 1.2f);

            SpriteRenderer vfxsp = vfx.GetComponent<SpriteRenderer>();
            Rigidbody vfxRb = vfx.GetComponent<Rigidbody>();

            if (vfx.GetComponent<Bomb>().type != BombType.trap)
            {
                if (!movement.sr.flipX)
                {
                    //vfx.transform.SetParent(VFXPosiR.transform, false);
                    vfx.transform.position = VFXPosiR.transform.position;
                    vfxRb.AddForce(Vector3.right * 500 * Time.deltaTime);
                }
                else
                {
                    //vfx.transform.SetParent(VFXPosiL.transform, false);
                    vfx.transform.position = VFXPosiL.transform.position;
                    vfxRb.AddForce(Vector3.left * 500 * Time.deltaTime);
                    //vfxsp.flipX = true;
                    vfx.transform.localScale = new Vector2(-1f, 1f);
                }
                StartCoroutine(vfxCountTime(1f * skillData.skillLevel, vfx));
            }
            else
            {
                if (!movement.sr.flipX)
                {
                    //vfx.transform.SetParent(VFXPosiR.transform, false);
                    vfx.transform.position = VFXPosiR.transform.position;
                }
                else
                {
                    //vfx.transform.SetParent(VFXPosiL.transform, false);
                    vfx.transform.position = VFXPosiL.transform.position;
                }
            }
        }
        // if vfx is particle System
        else if (vfx.GetComponent<GroundSlash>()!=null)
        {

        }
    }
    IEnumerator vfxCountTime(float time, GameObject gameObject)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
