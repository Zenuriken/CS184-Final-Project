using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    #region Editor Variables
    [SerializeField]
    [Tooltip("The name of this enemy")]
    private string m_Name;
    public string EnemyName {
        get {
            return m_Name;
        }
    }

    [SerializeField]
    [Tooltip("The prefab of this enemy that will be spawned")]
    private GameObject m_EnemyGO;
    public GameObject EnemyGO {
        get {
            return m_EnemyGO;
        }
    }

    [SerializeField]
    [Tooltip("The number of seconds before the next enemy is spawned")]
    private float m_TimeToNextSpawn;
    public float TimeToNextSpawn {
        get {
            return m_TimeToNextSpawn;
        }
    }

    [SerializeField]
    [Tooltip("The number of enemies to spawn. If set to 0, it will spawn endlessly")]
    private int m_NumberToSpawn;
    public int NumberToSpawn {
        get {
            return m_NumberToSpawn;
        }
    }
    #endregion
}
