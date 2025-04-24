using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    private Vector3 lookVec;
    private Vector3 tauntVec;

    [SerializeField]
    private GameObject missilePrefab;
    [SerializeField]
    private Transform missilePortA;
    [SerializeField]
    private Transform missilePortB;

    int randomPattern;

    private bool isLook; // Player Look, true면 바라보는 중

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();

        target = FindObjectOfType<Player>().transform;
        navMeshAgent.isStopped = true;
    }

    private void Start()
    {
        curHealth = maxHealth;
        isLook = true;
        StartCoroutine(PatternRoutine());
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (isLook)
        {
            Vector2 playerVec = target.GetComponent<Player>().InputVec;
            lookVec = new Vector3(playerVec.x, 0, playerVec.y) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
        {
            navMeshAgent.SetDestination(tauntVec);
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (isChase)
        {
            FreezeVelocity();
        }
    }

    IEnumerator PatternRoutine()
    {
        yield return new WaitForSeconds(2f);

        rigid.velocity = Vector3.zero;
        randomPattern = Random.Range(0, 3);
        switch (randomPattern)
        {
            case 0:
                StartCoroutine(MissileShotRoutine());
                break;
            case 1:
                StartCoroutine(RockShotRoutine());
                break;
            case 2:
                StartCoroutine(TauntRoutine());
                break;
        }
    }

    IEnumerator MissileShotRoutine()
    {
        yield return null;

        animator.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);
        Instantiate<GameObject>(missilePrefab, missilePortA.position, missilePortA.rotation);
        AudioManager.Instance.PlaySfxAudio(Sfx.Missile);

        yield return new WaitForSeconds(0.3f);
        Instantiate<GameObject>(missilePrefab, missilePortB.position, missilePortB.rotation);
        AudioManager.Instance.PlaySfxAudio(Sfx.Missile);
    }

    IEnumerator RockShotRoutine()
    {
        yield return null;

        isLook = false;
        animator.SetTrigger("doRock");

        Instantiate<GameObject>(bulletPrefab, transform.position + transform.forward * 5f + transform.up * 3f, transform.rotation);
        AudioManager.Instance.PlaySfxAudio(Sfx.Rock);
    }

    IEnumerator TauntRoutine()
    {
        yield return null;

        tauntVec = target.position + lookVec; // 착지할 위치
        isLook = false; // Player 주시 중지
        navMeshAgent.isStopped = false;
        boxCollider.enabled = false;
        animator.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
    }
   
    public void EndPattern()
    {
        rigid.velocity = Vector3.zero;
        isLook = true;
        navMeshAgent.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(PatternRoutine());
    }
}
