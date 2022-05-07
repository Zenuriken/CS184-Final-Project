using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBlast : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Amount of damage per bullet")]
    private float dmg;

    private void OnCollisionEnter(Collision other) {
        if (other.collider.CompareTag("Enemy")) {
            //Debug.Log("Hit enemy!");
            EnemyController enemyScript = other.collider.GetComponent<EnemyController>();
            if (enemyScript.GetHealth() > 0) {
                enemyScript.DecreaseHealth(dmg);
            }
        }
    }
}
