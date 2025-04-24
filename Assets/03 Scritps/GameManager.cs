using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private GameObject menuCamera;
    [SerializeField]
    private GameObject gameCamera;
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private GameInfo gameInfoPanel;
    [SerializeField]
    private GameOver gameOverPanel;
    [SerializeField]
    private GameObject itemShop;
    [SerializeField]
    private GameObject weaponShop;
    [SerializeField]
    private GameObject startZone;
    [SerializeField]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private Player player;
    private Vector3 playerInitPos = new Vector3(0f, 0.5f, -8f);

    private int score;
    private int curStageNum = 0;
    public int CurStageNum => curStageNum;
    private float playTime = 0f;

    private int[] enemyCnt = new int[3] {0, 0, 0};

    private Boss boss;

    [SerializeField]
    private TextMeshProUGUI maxScoreText;

    private bool isBattle = false; // 전투 유무, true면 전투 중
    private bool isEndSpawn = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        maxScoreText.SetText(string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore", 0)));
        Init();
        AudioManager.Instance.PlayBgmAudio(true);
    }

    private void Update()
    {
        if (isBattle)
        {
            playTime += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        // 상단 UI
        gameInfoPanel.UpdateScore(score);

        gameInfoPanel.UpdatePlayTime(playTime);
        gameInfoPanel.UpdateStageText(curStageNum);

        // Player UI
        gameInfoPanel.UpdatePlayerHealth(player.Health, player.MaxHealth);
        gameInfoPanel.UpdatePlayerCoin(player.Coin);
        if ((player.EquipWeapon == null) || (player.EquipWeapon.WeaponType == Weapon.Type.Melee))
        {
            gameInfoPanel.UpdatePlayerAmmo(-1, player.Ammo);
        }
        else
        {
            gameInfoPanel.UpdatePlayerAmmo(player.EquipWeapon.CurAmmo, player.Ammo);
        }

        // Weapon UI
        gameInfoPanel.UpdateWeaponSelect(player.HasWeapons, player.HasGrenade);

        // Enemy Num UI
        gameInfoPanel.UpdateEnemyCount(enemyCnt);

        if (boss != null)
        {
            gameInfoPanel.UpdateBossHealthBar(boss.CurHealth, boss.MaxHealth);
        }
    }

    private void InitPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    private void Init()
    {
        menuCamera.SetActive(true);
        gameCamera.SetActive(false);

        menuPanel.SetActive(true);
        gameInfoPanel.gameObject.SetActive(false);

        isBattle = false;
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        player.gameObject.SetActive(false);
    }

    public void GameStart()
    {
        menuCamera.SetActive(false);
        gameCamera.SetActive(true);

        menuPanel.SetActive(false);
        gameInfoPanel.gameObject.SetActive(true);

        player.gameObject.SetActive(true);

        // 초기화
        EndStage();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void KillEnemy(int index)
    {
        enemyCnt[index]--;

        // Spawn이 끝났고, 남은 적이 없다면 Stage 종료
        if (isEndSpawn && CheckRemainEnemy())
        {
            Invoke("EndStage", 4f);
        }
    }

    private bool CheckRemainEnemy()
    {
        bool check = true;
        foreach (int cnt in enemyCnt)
        {
            if (cnt != 0)
            {
                check = false;
                break;
            }
        }

        return check;
    }
    public void AddScore(int score)
    {
        this.score += score;
    }

    public void IncreaseEnemyCnt(int index)
    {
        enemyCnt[index]++;
    }

    public void StartStage()
    {
        isBattle = true;
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);
        curStageNum++;
        player.transform.position = playerInitPos;

        // Spawn Enemy Num = CurStage * 5
        enemySpawner.StartEnemySwapn();
    }

    private void EndStage()
    {
        isBattle = false;
        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);
        player.transform.position = playerInitPos;
    }

    public void EndSpawn()
    {
        isEndSpawn = true;
    }

    public void SpawnBoss(Boss boss)
    {
        this.boss = boss;
        gameInfoPanel.SetBossUiGroup(true);
    }

    public void KillBoss()
    {
        boss = null;
        gameInfoPanel.SetBossUiGroup(false);
        Invoke("EndStage", 4f);
    }

    public void GameOver()
    {
        gameInfoPanel.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.UpdateTotalScore(score);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
