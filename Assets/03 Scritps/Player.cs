using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody rigid;
    private Animator animator;
    private MeshRenderer[] meshRenderers;

    [SerializeField]
    private Camera followCamera;

    private Vector2 inputVec;
    public Vector2 InputVec => inputVec;
    private Vector3 moveVec;

    [SerializeField]
    private float originMoveSpeed;
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float jumpForce = 10f;

    private GameObject nearObject; // 근처에 있는 Object
    private Weapon equipWeapon = null; // 현재 장착하고 있는 Weapon
    public Weapon EquipWeapon => equipWeapon;
    [SerializeField]
    private Weapon[] weapons; // Player의 Weapon Prefab
    [SerializeField]
    private bool[] hasWeapons; // 해당 Weapon을 갖고 있는지
    public bool[] HasWeapons => hasWeapons;
    [SerializeField]
    private GameObject grenadePrefab;
    private Coroutine attackRoutine = null;

    [SerializeField]
    private GameObject[] orbitGrenades; // Player 주변을 돌고있는 Grenade

    [SerializeField]
    private int ammo;
    public int Ammo => ammo;
    [SerializeField]
    private int coin;
    public int Coin => coin;
    [SerializeField]
    private int health;
    public int Health => health;
    [SerializeField]
    private int hasGrenade;
    public int HasGrenade => hasGrenade;

    [SerializeField]
    private int maxAmmo;
    public int MaxAmmo => maxAmmo;
    [SerializeField]
    private int maxCoin;
    public int MaxCoin => maxCoin;
    [SerializeField]
    private int maxHealth;
    public int MaxHealth => maxHealth;
    [SerializeField]
    private int hasMaxGrenade;

    private bool isJump = false; // Jump 여부, true면 점프 중
    private bool isDodge = false; // 회피 여부, true면 회피 중
    private bool isSwap = false; // Swap 여부, true면 Swap 중
    private bool isAttack = false; // Attack 여부, true면 Attack 중
    private bool isReload = false; // Reload 여부, true면 Reload 중
    private bool isDamaged = false; // Damaged 여부, true면 Damage 받는 중 (Damage 받는 중에는 다시 Damage를 못 입음)
    private bool isKnockBack = false; // KnockBack 여부, true면 KnockBack 받는 중 (이때 못 움직임)
    private bool isShopping = false; // Shopping 여부, trun면 Shopping 중 (이 때 못 움직임)
    private bool isDead = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        hasWeapons = new bool[weapons.Length];
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private void Start()
    {
        moveSpeed = originMoveSpeed;
        health = maxHealth;
    }

    private void FixedUpdate()
    {
        if (!isKnockBack && !isDead)
        {
            rigid.velocity = new Vector3(moveVec.x * moveSpeed, rigid.velocity.y, moveVec.y * moveSpeed);
        }
    }

    // WASD or KeyArrow
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVec = context.action.ReadValue<Vector2>();

        if (isDodge || isSwap || isAttack || isDead)
        {
            return;
        }

        moveVec = inputVec;
        animator.SetBool("isWalk", moveVec != Vector3.zero);

        KeyBoardTurn(); // 이동 방향으로 회전
    }

    // 키보드에 의한 회전
    private void KeyBoardTurn()
    {
        if (isDead)
        {
            return;
        }

        transform.LookAt(transform.position + new Vector3(moveVec.x, 0, moveVec.y)); // 이동 방향으로 회전
    }

    // 마우스에 의한 회전
    private void MouseTurn()
    {
        if (isDead)
        {
            return;
        }

        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        if (Physics.Raycast(ray, out rayHit, 100))
        {
            Vector3 fireVec = rayHit.point - transform.position;
            fireVec.y = 0;
            transform.LookAt(transform.position + fireVec);
        }
    }

    // Left Shift Button Hold
    public void OnRun(InputAction.CallbackContext context)
    {
        if (isDodge || isDead)
        {
            return;
        }

        // Left Shift Button이 Hold 됐으면
        if (context.performed)
        {
            moveSpeed = originMoveSpeed * 2;
            animator.SetBool("isRun", true);
        }
        // Left Shift Button을 뗐으면
        else if (context.canceled)
        {
            moveSpeed = originMoveSpeed;
            animator.SetBool("isRun", false);
        }
    }

    // Space Button
    public void OnJump(InputAction.CallbackContext context)
    {
        if (isDodge || isJump || isSwap || isAttack || isReload || isDead)
        {
            return;
        }

        // Space Bar가 눌렸으면
        if (context.performed)
        {
            isJump = true;
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("isJump", isJump);
            animator.SetTrigger("doJump");
            AudioManager.Instance.PlaySfxAudio(Sfx.Jump);
        }
    }

    // Left Shift Button
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (isDodge || isJump || isSwap || isAttack || isReload || isDead)
        {
            return;
        }

        if (context.performed)
        {
            moveSpeed = originMoveSpeed * 2f;
            isDodge = true;
            animator.SetTrigger("doDodge");
            AudioManager.Instance.PlaySfxAudio(Sfx.Dodge);
        }
    }

    public void EndDodge()
    {
        isDodge = false;
        moveVec = inputVec; // 회피 도중 미리 입력으로 들어온 방향으로 초기화
        animator.SetBool("isWalk", moveVec != Vector3.zero);
        moveSpeed = originMoveSpeed;
        KeyBoardTurn(); // 이동 방향으로 회전
    }

    // E Button
    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (isDodge || isJump || isSwap || isAttack || isReload || isDead)
        {
            return;
        }

        if (context.performed && nearObject != null)
        {
            if (nearObject.CompareTag("Weapon"))
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.Value; // item.Value는 Weapon의 Index
                hasWeapons[weaponIndex] = true;
                AudioManager.Instance.PlaySfxAudio(Sfx.Item);
                Destroy(nearObject);
            }
            else if (nearObject.CompareTag("Shop"))
            {
                isShopping = true;
                Shop shop = nearObject.GetComponent<Shop>();
                shop.EnterShop(this);
            }
        }
    }

    // Buy Item
    public void OnBuyItem(int price)
    {
        coin -= price;
    }

    // 1, 2, 3 Button
    public void OnWeaponSwap(InputAction.CallbackContext context)
    {
        if (isDodge || isJump || isSwap || isAttack || isReload || isDead)
        {
            return;
        }

        if (context.performed)
        {
            int weaponIndex = Mathf.RoundToInt(context.ReadValue<float>());

            if ((equipWeapon == weapons[weaponIndex]) || !hasWeapons[weaponIndex])
            {
                return;
            }

            // 기존에 장착하고 있던 Weapon 비활성화
            if (equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }

            equipWeapon = weapons[weaponIndex];
            equipWeapon.gameObject.SetActive(true);

            isSwap = true;
            animator.SetTrigger("doSwap");

            moveVec = Vector3.zero;
        }
    }

    public void EndWeaponSwap()
    {
        isSwap = false;
        moveVec = inputVec;
        animator.SetBool("isWalk", moveVec != Vector3.zero);
        KeyBoardTurn();
    }

    // Mouse Left Button
    public void OnAttack(InputAction.CallbackContext context)
    {
        // 왼 클릭을 떼면 공격 중지
        if (context.canceled)
        {
            StopAttackRoutine();
        }

        if (isDodge || isJump || isSwap || isAttack || isReload || isShopping || isDead)
        {
            return;
        }
        // 장착 무기가 없는 경우 공격 불가
        else if (equipWeapon == null)
        {
            return;
        }
        // 원거리 무기의 현재 장전된 총알이 0개인 경우 공격 불가
        else if ((equipWeapon.WeaponType) == Weapon.Type.Range && equipWeapon.CurAmmo == 0)
        {
            return;
        }

        // 마우스 왼클릭을 했고, 해당 무기의 공격 Rate가 지났다면 공격 가능
        if (context.performed)
        {
            isAttack = true;
            moveVec = Vector3.zero;

            StartAttackRoutine();
        }
    }

    private void StartAttackRoutine()
    {
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return null;

        try
        {
            while (true)
            {
                isAttack = true;
                moveVec = Vector3.zero;
                MouseTurn(); // 마우스로 클릭한 지점으로 회전

                equipWeapon.Use();

                animator.SetTrigger(equipWeapon.WeaponType == Weapon.Type.Melee ? "doSwing" : "doShot");

                // 총알을 다 썼으면 Stop
                if (equipWeapon.CurAmmo == 0)
                {
                    StopAttackRoutine();
                }

                // 공격 대기 시간
                yield return new WaitForSeconds(equipWeapon.Rate);
            }
        }
        finally
        {
            // Use를 했는데 Trigger를 발동하지 못해서 Event가 발동하지 않을 때
            EndAttack();
        }
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
    }

    public void EndAttack()
    {
        isAttack = false;
        moveVec = inputVec;
        animator.SetBool("isWalk", moveVec != Vector3.zero);
        KeyBoardTurn();
    }

    // Mouse Right Button
    public void OnThrowGrenade(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (hasGrenade == 0)
            {
                return;
            }
            else if (isDodge || isJump || isSwap || isAttack || isReload || isShopping || isDead)
            {
                return;
            }

            // 화면에서 Mouse로 클릭한 지점에 Ray를 쏴서 해당 방향에 존재하는 Object의 정보를 가져옴 
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 throwVec = rayHit.point - transform.position;
                GameObject grenadeObj = Instantiate<GameObject>(grenadePrefab, transform.position, transform.rotation);
                grenadeObj.GetComponent<Grenade>().Throw(throwVec);

                hasGrenade--;
                orbitGrenades[hasGrenade].SetActive(false);

                animator.SetTrigger("doThrow");
            }
        }
    }

    // R Button
    public void OnReload(InputAction.CallbackContext context)
    {
        if (isDodge || isJump || isSwap || isAttack || isReload || isDead)
        {
            return;
        }
        // 장착 무기가 없거나 근접 무기인 경우 장전 불가
        else if (equipWeapon == null || equipWeapon.WeaponType == Weapon.Type.Melee)
        {
            return;
        }
        // 남아있는 총알이 하나도 없는 경우, 이미 최대치로 장전된 경우 장전 불가
        else if ((ammo == 0) || (equipWeapon.MaxAmmo == equipWeapon.CurAmmo))
        {
            return;
        }

        isReload = true;
        animator.SetTrigger("doReload");
    }

    public void EndReload()
    {
        isReload = false;

        // 현재 Player가 갖고 있는 Ammo에서 최대치만큼 Reload
        int needAmmo = equipWeapon.MaxAmmo - equipWeapon.CurAmmo;
        int reloadAmmo = Mathf.Min(ammo, needAmmo);
        ammo -= reloadAmmo;
        equipWeapon.Reload(reloadAmmo);
    }

    IEnumerator OnDamaged(bool isBossAtk)
    {
        AudioManager.Instance.PlaySfxAudio(Sfx.Hit);

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.color = new Color(255f, 100f, 100f, 255f) / 255f;
        }

        if (isBossAtk)
        {
            rigid.velocity = Vector3.zero;
            isKnockBack = true;
            rigid.AddForce(transform.forward * -30f, ForceMode.Impulse);
        }

        if (health > 0)
        {
            yield return new WaitForSeconds(0.7f);
            rigid.velocity = Vector3.zero;
            isKnockBack = false;

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material.color = Color.white;
            }
            isDamaged = false;
        }
        else
        {
            rigid.velocity = Vector3.zero;
            rigid.isKinematic = true;

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material.color = Color.gray;
            }
            OnDie();
        }
    }

    private void OnDie()
    {
        animator.SetTrigger("doDie");
        isDead = true;
        GameManager.Instance.GameOver();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥에 닿았을 때 점프 가능
        if (collision.gameObject.CompareTag("Floor"))
        {
            isJump = false;
            animator.SetBool("isJump", isJump);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Item"))
        {
            Item item = collider.GetComponent<Item>();

            switch (item.ItemType)
            {
                case Item.Type.Ammo:
                    ammo += item.Value;
                    ammo = Mathf.Min(ammo, maxAmmo);
                    break;
                case Item.Type.Coin:
                    coin += item.Value;
                    coin = Mathf.Min(coin, maxCoin);
                    break;
                case Item.Type.Heart:
                    health += item.Value;
                    health = Mathf.Min(health, maxHealth);
                    break;
                case Item.Type.Grenade:
                    orbitGrenades[hasGrenade].SetActive(true); // Player 주변을 돌고 있는 수류탄을 하나씩 활성화
                    hasGrenade += item.Value;
                    hasGrenade = Mathf.Min(hasGrenade, hasMaxGrenade);
                    break;
            }

            AudioManager.Instance.PlaySfxAudio(Sfx.Item);
            Destroy(collider.gameObject);
        }
        else if (collider.CompareTag("EnemyBullet"))
        {
            // 한 번 피격받으면 일정 시간 동안 무적
            if (!isDamaged && !isDead)
            {
                isDamaged = true;
                Bullet bullet = collider.GetComponent<Bullet>();
                health -= bullet.Damage;

                bool isBossAtk = (collider.name == "Boss Melee Area");

                StartCoroutine(OnDamaged(isBossAtk));
            }
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Weapon") || collider.CompareTag("Shop"))
        {
            nearObject = collider.gameObject;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Weapon"))
        {
            nearObject = null;
        }
        else if (collider.CompareTag("Shop"))
        {
            Shop shop = collider.GetComponent<Shop>();
            shop.ExitShop();
            isShopping = false;
            nearObject = null;
        }
    }
}

