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
    private Camera mainCamera;
    private PlayerMovement movement;
    private Animator animator;

    private void Start()
    {
        mainCamera = Camera.main;
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

        // Get Mouse Position
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        // Create VFX
        GameObject vfx = Instantiate(skillData.skillPrefab, transform.position, Quaternion.identity);

        if (vfx.GetComponent<Bomb>() != null)
        {
            vfx.GetComponent<Bomb>().damage = skillData.DamageOrHeal;
            vfx.transform.localScale = new Vector2(1.2f, 1.2f);

            if (vfx.GetComponent<Bomb>().type != BombType.trap)
            {
                vfx.transform.position = weaponPosi.transform.position;
                //Let vfx move front to mouse position
                Vector3 direction = (mouseWorldPosition - transform.position).normalized;
                direction.y = 0;

                vfx.GetComponent<Rigidbody>().velocity = direction * Time.timeScale;
            }
            else
                StartCoroutine(MoveAndThrowVFX(vfx, mouseWorldPosition));
        }
        // if vfx is particle System
        else if (vfx.GetComponent<GroundSlash>() != null)
        {

        }

        StartCoroutine(vfxCountTime(level * 2, vfx));
    }
    IEnumerator vfxCountTime(float time, GameObject gameObject)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }
    private IEnumerator MoveAndThrowVFX(GameObject vfx, Vector3 targetPosition)
    {
        float elapsedTime = 0.1f;
        float maxThrowTime = 1f; // 最大拋出時間
        float throwHeight = 1f; // 最大拋高

        while (Input.GetMouseButton(1) && elapsedTime < maxThrowTime)
        {
            vfx.transform.position = transform.position;
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Vector3 startPosition = vfx.transform.position;

        float throwAmount = Mathf.Lerp(0, throwHeight, elapsedTime / maxThrowTime);

        float duration = 1f;
        float moveElapsedTime = 0f;

        while (moveElapsedTime < duration)
        {
            // 計算 VFX 的位置
            float t = moveElapsedTime / duration;

            // 計算拋出位置
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition * elapsedTime, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * throwAmount;

            vfx.transform.position = currentPosition;

            moveElapsedTime += Time.deltaTime;
            yield return null; // 等待下一幀
        }
    }
}
