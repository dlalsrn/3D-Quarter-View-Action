using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] enemyPrefabs;
    [SerializeField]
    private Transform[] enemySpawnZones;
    private float[] enemyWeight = new float[3] {0.5f, 0.3f, 0.2f}; // Enemy의 Spawn 확률

    [SerializeField]
    private GameObject bossPrefab;

    public void StartEnemySwapn()
    {
        StartCoroutine(EnemySpawnRoutine());
    }

    IEnumerator EnemySpawnRoutine()
    {
        SetSpawnZone(true);

        // 5 Stage 마다 보스 소환
        if (GameManager.Instance.CurStageNum % 3 == 0)
        {
            Boss bossObj = Instantiate<GameObject>(bossPrefab, enemySpawnZones[0].position, Quaternion.identity).GetComponent<Boss>();
            GameManager.Instance.SpawnBoss(bossObj);
        }
        else
        {
            int enemyCnt = GameManager.Instance.CurStageNum * 5;
            yield return new WaitForSeconds(2f);
            
            for (int index = 0; index < enemyCnt; index++)
            {
                int enemyIndex = GenerateRandomIndex();
                int spawnIndex = Random.Range(0, enemySpawnZones.Length);
                GameObject enemyObj = Instantiate<GameObject>(enemyPrefabs[enemyIndex], enemySpawnZones[spawnIndex].position, Quaternion.identity);
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                GameManager.Instance.IncreaseEnemyCnt((int)enemy.EnemyType);
                yield return new WaitForSeconds(1f);
            }
        }

        yield return new WaitForSeconds(2f);
        SetSpawnZone(false);
        GameManager.Instance.EndSpawn();
    }

    private int GenerateRandomIndex()
    {
        int index = -1;

        float total = 0f;
        foreach (float weight in enemyWeight)
        {
            total += weight * 100;
        }

        int randomIndex = Random.Range(0, (int)total);
        int sum = 0;
        for (int i = 0; i < enemyWeight.Length; i++)
        {
            sum += (int)(enemyWeight[i] * 100);
            if (randomIndex < sum) // 현재 확률에 들어왔다면
            {
                index = i;
                break;
            }
        }

        return index;
    }

    public void SetSpawnZone(bool active)
    {
        foreach (Transform spawnZone in enemySpawnZones)
        {
            spawnZone.gameObject.SetActive(active);
        }
    }
}
