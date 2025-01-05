using Skill;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int Health = 100;
    public int level = 1;
    public List<WeaponData> Weapons = null;
    public List<SkillData> Skills = null;
    public Transform VFXPosiR = null, VFXPosiL = null;
    private PlayerMovement movement;
    private float x = 0;

    private void Start()
    {
        movement = gameObject.GetComponent<PlayerMovement>();
        x = VFXPosiR.transform.position.x;
    }

    // Start is called before the first frame update
    public Player()
    {
        level = 0;
        Health = 100;
    }
/*
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

        GameObject vfx = Instantiate(skillData.skillPrefab, Vector3.zero, Quaternion.identity);
        vfx.GetComponent<Bomb>().damage = skillData.DamageOrHeal;
        vfx.transform.localScale = new Vector2(1.2f, 1.2f);

        SpriteRenderer vfxsp = vfx.GetComponent<SpriteRenderer>();
        Rigidbody vfxRb = vfx.GetComponent<Rigidbody>();

        if (vfx.GetComponent<Bomb>().type != BombType.trap)
        {
            if (!movement.sr.flipX)
            {
                vfx.transform.SetParent(VFXPosiR.transform, false);
                vfx.transform.position = VFXPosiR.transform.position;
                vfxRb.AddForce(Vector3.right * 500 * Time.deltaTime);
            }
            else
            {
                vfxsp.flipX = true;
                vfx.transform.SetParent(VFXPosiL.transform, false);
                vfx.transform.position = VFXPosiL.transform.position;
                vfxRb.AddForce(Vector3.left * 500 * Time.deltaTime);
            }
        }
        else
        {
            if (!movement.sr.flipX)
            {
                vfx.transform.SetParent(VFXPosiR.transform, false);
                vfx.transform.position = VFXPosiR.transform.position;
            }
            else
            {
                vfx.transform.SetParent(VFXPosiL.transform, false);
                vfx.transform.position = VFXPosiL.transform.position;
            }
        }
    }
}
