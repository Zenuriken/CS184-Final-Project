using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : MonoBehaviour
{
    private List<Collider> enemyColliders;

    private void Awake() {
        enemyColliders = new List<Collider>();
    }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            Collider collider = other.GetComponent<Collider>();
            if (!enemyColliders.Contains(collider)) {
                enemyColliders.Add(collider);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy")) {
            Collider collider = other.GetComponent<Collider>();
            if (enemyColliders.Contains(collider)) {
                enemyColliders.Remove(collider);
            }
        }
    }

    public void RemoveEnemy(Collider collider) {
        if (enemyColliders.Contains(collider)) {
            enemyColliders.Remove(collider);
        }
    }

    public List<Collider> GetEnemyList() {
        return enemyColliders;
    }
}
