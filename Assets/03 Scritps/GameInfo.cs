using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI stageText;
    [SerializeField]
    private TextMeshProUGUI playTimeText;

    [SerializeField]
    private TextMeshProUGUI playerHealthText;
    [SerializeField]
    private TextMeshProUGUI playerAmmoText;
    [SerializeField]
    private TextMeshProUGUI playerCoinText;

    [SerializeField]
    private Image[] weaponImages;
    [SerializeField]
    private Image weaponRImage;

    [SerializeField]
    private TextMeshProUGUI[] enemyCntTexts;

    [SerializeField]
    private GameObject bossHealthGroup;
    [SerializeField]
    private RectTransform bossHealthBar;
    
    public void UpdateScore(int score)
    {
        scoreText.SetText(string.Format("{0:n0}", score));
    }

    public void UpdateStageText(int curStageNum)
    {
        stageText.SetText($"STAGE {curStageNum}");
    }

    public void UpdatePlayTime(float playTime)
    {
        int h = (int)playTime / 3600;
        int m = (int)playTime % 3600 / 60;
        int s = (int)playTime % 60;
        playTimeText.SetText(string.Format("{0:00}:{1:00}:{2:00}", h, m, s));
    }

    public void UpdatePlayerHealth(int curHp, int maxHp)
    {
        playerHealthText.SetText($"{curHp} / {maxHp}");
    }

    public void UpdatePlayerAmmo(int curAmmo, int playerAmmo)
    {
        if (curAmmo == -1) // 근접 무기
        {
            playerAmmoText.SetText($"- / {playerAmmo}");
        }
        else
        {
            playerAmmoText.SetText($"{curAmmo} / {playerAmmo}");
        }
    }

    public void UpdatePlayerCoin(int coin)
    {
        playerCoinText.SetText(string.Format("{0:n0}", coin));
    }

    public void UpdateEnemyCount(int[] enemyCnt)
    {
        for (int index = 0; index < enemyCnt.Length; index++)
        {
            enemyCntTexts[index].SetText($"x {enemyCnt[index]}");
        }
    }

    public void UpdateWeaponSelect(bool[] hasWeapon, int hasGrenade)
    {
        for (int index = 0; index < hasWeapon.Length; index++)
        {
            weaponImages[index].color = new Color(1, 1, 1, hasWeapon[index] ? 1 : 0);
        }

        weaponRImage.color = new Color(1, 1, 1, hasGrenade != 0 ? 1 : 0);
    }

    public void UpdateBossHealthBar(int curHealth, int maxHealth)
    {
        bossHealthBar.localScale = new Vector3(curHealth / (float)maxHealth, 1, 1);
    }

    public void SetBossUiGroup(bool active)
    {
        bossHealthGroup.SetActive(active);
    }
}
