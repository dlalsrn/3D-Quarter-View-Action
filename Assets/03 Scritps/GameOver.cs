using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI curScoreText;
    [SerializeField]
    private TextMeshProUGUI bestText;

    public void UpdateTotalScore(int score)
    {
        int maxScore = PlayerPrefs.GetInt("MaxScore", 0);
        curScoreText.SetText($"{score}");

        if (maxScore < score)
        {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", score);
        }
    }
}
