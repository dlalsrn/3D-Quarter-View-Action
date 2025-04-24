using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private RectTransform shopUi;

    private Player enterPlayer;

    [SerializeField]
    private GameObject[] itemPrefabs;
    [SerializeField]
    private int[] itemPrices;
    [SerializeField]
    private Transform[] itemSpawnPos;
    [SerializeField]
    private TextMeshProUGUI talkText;
    [SerializeField]
    private string[] talkData;
    private Coroutine talkCoroutine;

    public void EnterShop(Player player)
    {
        enterPlayer = player;
        shopUi.anchoredPosition = Vector3.zero;
    }

    public void ExitShop()
    {
        animator.SetTrigger("doHello");
        shopUi.anchoredPosition = Vector3.down * 1000f;
    }

    public void Buy(int index)
    {
        int price = itemPrices[index];
        if (price > enterPlayer.Coin)
        {
            if (talkCoroutine != null)
            {
                StopCoroutine(talkCoroutine);
            }
            talkCoroutine = StartCoroutine(TalkRoutine());
            return;
        }

        enterPlayer.OnBuyItem(price);
        AudioManager.Instance.PlaySfxAudio(Sfx.Buy);
        
        Vector3 randomVec = Vector3.right * Random.Range(-3f, 3f) + Vector3.forward * Random.Range(-3f, 3f);
        Instantiate<GameObject>(itemPrefabs[index], itemSpawnPos[index].position + randomVec, itemSpawnPos[index].rotation);
    }

    IEnumerator TalkRoutine()
    {
        talkText.SetText(talkData[1]);
        yield return new WaitForSeconds(2f);
        talkText.SetText(talkData[0]);
        talkCoroutine = null;
    }
}
