using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A, // 일반형
        B, // 돌격형
        C, // 원거리형
        D, // 보스
    }

    // private Rigidbody rigid;
    // private MeshRenderer[] meshRenderers;
    // private NavMeshAgent navMeshAgent;
    // private Animator animator;
    // private BoxCollider boxCollider;

    // [SerializeField]
    // private Type enemyType;
    // public Type EnemyType => enemyType;

    // [SerializeField]
    // private int maxHealth;
    // [SerializeField]
    // private int curHealth;

    // [SerializeField]
    // private Transform target;
    // [SerializeField]
    // private BoxCollider attackArea;
    // [SerializeField]
    // private GameObject bulletPrefab;

    // [SerializeField]
    // private float rushForce = 20f;

    // [SerializeField]
    // private float knockBackForce = 5f;
    // private const int EnemyDeadLayer = 13;

    // private bool isChase = false; // target 추적 유무, true면 추적 중
    // private bool isAttack = false; // Attack 유무, true면 Attack 중
    // private bool isDead = false;

    protected Rigidbody rigid;
    protected MeshRenderer[] meshRenderers;
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    protected BoxCollider boxCollider;

    [SerializeField]
    protected Type enemyType;
    public Type EnemyType => enemyType;

    [SerializeField]
    protected int maxHealth;
    public int MaxHealth => maxHealth;
    [SerializeField]
    protected int curHealth;
    public int CurHealth => curHealth;
    [SerializeField]
    private int score;
    [SerializeField]
    private GameObject[] coinPrefabs;

    [SerializeField]
    protected Transform target;
    [SerializeField]
    protected BoxCollider attackArea;
    [SerializeField]
    protected GameObject bulletPrefab;

    [SerializeField]
    protected float rushForce = 20f;

    [SerializeField]
    protected float knockBackForce = 5f;
    protected const int EnemyDeadLayer = 13;

    protected bool isChase = false; // target 추적 유무, true면 추적 중
    protected bool isAttack = false; // Attack 유무, true면 Attack 중
    protected bool isDead = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        target = FindObjectOfType<Player>().transform;
    }

    private void Start()
    {
        curHealth = maxHealth;
        StartChase();
    }

    private void Update()
    {
        // target 추적
        if (navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = !isChase;
            navMeshAgent.SetDestination(target.position);
        }
    }

    private void FixedUpdate()
    {
        if (isChase)
        {
            Targeting();
            FreezeVelocity();
        }
    }

    private void StartChase()
    {
        isChase = true;
        animator.SetBool("isWalk", true);
    }

    protected void FreezeVelocity()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    private void Targeting()
    {
        if (isAttack || isDead)
        {
            return;
        }

        float radius = 0f;
        float range = 0f;

        switch (enemyType)
        {
            case Type.A: // 기본형
                radius = 1.5f;
                range = 2f;
                break;
            case Type.B: // 돌격형
                radius = 1f;
                range = 10f;
                break;
            case Type.C: // 원거리형
                radius = 0.5f;
                range = 30f;
                break;
        }

        // Player가 앞에 있고, 아직 공격 중이 아니라면
        RaycastHit rayHit;
        if (Physics.SphereCast(transform.position, radius, transform.forward, out rayHit, range, LayerMask.GetMask("Player")) && !isAttack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttack = true; // 공격 중 설정
        isChase = false; // 추적 중지
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        animator.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A: // 일반형
                break;
            case Type.B: // 돌격형
                yield return new WaitForSeconds(0.3f);
                rigid.AddForce(transform.forward * rushForce + Vector3.up, ForceMode.Impulse); // 앞으로 돌진
                break;
            case Type.C: // 원거리형
                yield return new WaitForSeconds(0.6f);
                GameObject bullet = Instantiate<GameObject>(bulletPrefab, transform.position  + Vector3.up * 2f, transform.rotation);
                bullet.GetComponent<Bullet>().Fire();
                AudioManager.Instance.PlaySfxAudio(Sfx.Missile);
                break;
            default:
                break;
        }
        
        yield return null;
    }

    public void EnableAttackArea()
    {
        attackArea.enabled = true;
    }

    public void DisableAttackArea()
    {
        if (enemyType == Type.D)
        {
            AudioManager.Instance.PlaySfxAudio(Sfx.Taunt);
        }
        attackArea.enabled = false;
    }

    public void EndAttack()
    {
        animator.SetBool("isAttack", false);

        switch (enemyType)
        {
            case Type.A: // 일반형
                isAttack = false; // 공격 끝
                isChase = true; // 추적 시작
                break;  
            case Type.B: // 돌격형
                rigid.velocity = Vector3.zero; // 돌진 후 정지
                StartCoroutine(DelayChaseRoutine(2f)); // 돌진 후 2초 휴식
                break;
            case Type.C: // 원거리형
                StartCoroutine(DelayChaseRoutine(2f)); // 발사 후 2초 휴식
                break;
            default:
                break;
        }
    }

    IEnumerator DelayChaseRoutine(float delayTime)
    {
        animator.SetBool("isWalk", false); // 돌진 후 쉬는 동안 걷는 모션 정지
        yield return new WaitForSeconds(delayTime);
        isChase = true;
        isAttack = false;
        animator.SetBool("isWalk", true); // 휴식 후 다시 Walk
    }

    public void OnHit(int damage, Vector3 firePos, bool isGrenade)
    {
        if (!isDead)
        {
            curHealth -= damage;
            Vector3 reactVec = transform.position - firePos;
            StartCoroutine(OnHitRoutine(reactVec, isGrenade));
        }
    }

    IEnumerator OnHitRoutine(Vector3 reactVec, bool isGrenade)
    {
        AudioManager.Instance.PlaySfxAudio(Sfx.Hit);

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.color = new Color(255f, 100f, 100f, 255f) / 255f;
        }

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material.color = Color.white;
            }
        }
        else
        {
            StopAllCoroutines();
            isDead = true;
            isChase = false;
            navMeshAgent.enabled = false;
            gameObject.layer = EnemyDeadLayer;
            rigid.velocity = Vector3.zero;

            GameManager.Instance.AddScore(score);
            if (enemyType != Type.D)
            {
                GameManager.Instance.KillEnemy((int)enemyType);
            }
            else
            {
                GameManager.Instance.KillBoss();
            }

            int randomIndex = Random.Range(0, coinPrefabs.Length);
            Item item = Instantiate<GameObject>(coinPrefabs[randomIndex], transform.position + Vector3.up * 4f, Quaternion.identity).GetComponent<Item>();
            Vector3 randomVec = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f));
            item.GetComponent<Rigidbody>().AddForce(randomVec * 2f, ForceMode.Impulse);

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material.color = Color.gray;
            }
            
            animator.SetTrigger("doDie");

            if (isGrenade) // 수류탄에 의해 사망했을 경우
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 2f;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * knockBackForce * 2f, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 5f, ForceMode.Impulse);
            }
            else // 근접 무기, 원거리 무기에 의해 사망했을 경우
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * knockBackForce, ForceMode.Impulse);
            }

            Destroy(gameObject, 3f);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Melee"))
        {
            Weapon weapon = collider.GetComponent<Weapon>();
            OnHit(weapon.Damage, collider.transform.position, false);
        }
        else if (collider.CompareTag("PlayerBullet"))
        {
            Bullet bullet = collider.GetComponent<Bullet>();
            OnHit(bullet.Damage, collider.transform.position, false);
        }
    }
}
