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
        movement = GetComponent<PlayerMovement>();
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
        if (collision.gameObject.CompareTag("levelUp"))
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
        skillData = Skills[0];
        GameObject vfx = Instantiate(skillData.skillPrefab, Vector3.zero, Quaternion.identity);
        vfx.transform.localScale = new Vector2(1.2f, 1.2f);

        SpriteRenderer vfxsp = vfx.GetComponent<SpriteRenderer>();
        Animator vfxAnim = vfx.GetComponent<Animator>();
        Rigidbody vfxRb = vfx.GetComponent<Rigidbody>();
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
        //vfxRb.AddForce(new Vector3(movement.sr.flipX? -50: 50, 0, 0));
        StartCoroutine(vfxCountTime(1.5f, vfx));
    }

    IEnumerator vfxCountTime(float time, GameObject gameObject)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
