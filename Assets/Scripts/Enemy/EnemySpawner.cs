using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The spawn radius")]
    private float spawnRadius;

    [SerializeField]
    [Tooltip("A list of all enemies that can be spawned and their information")]
    private EnemySpawnInfo[] m_Enemies;
    #endregion

    #region Initialization
    private void Awake() {
        StartSpawning();
    }
    #endregion

    #region Spawn Methods
    public void StartSpawning() {
        for (int i = 0; i < m_Enemies.Length; i++) {
            StartCoroutine(Spawn(i));
        }
    }

    private IEnumerator Spawn(int enemyInd) {
        EnemySpawnInfo info = m_Enemies[enemyInd];
        int i = 0;
        bool alwaysSpawn = false;
        if (info.NumberToSpawn == 0) {
            alwaysSpawn = true;
        }
        while (alwaysSpawn || i < info.NumberToSpawn) {
            yield return new WaitForSeconds(info.TimeToNextSpawn);
            Vector3 spawnPos = new Vector3(Random.Range(-spawnRadius, spawnRadius), 0, Random.Range(-spawnRadius, spawnRadius));
            spawnPos += transform.position;
            Instantiate(info.EnemyGO, spawnPos, Quaternion.identity);
            if (!alwaysSpawn) {
                i++;
            }
        }
    }

    private void OnDrawGizmosSelected() {   
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRadius * 2, 5, spawnRadius * 2));
    }
    #endregion
}
