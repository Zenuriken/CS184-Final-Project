using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Amount of damage per bullet")]
    private float dmg;

    private void OnCollisionEnter(Collision other) {
        if (other.collider.CompareTag("Player")) {
            PlayerController playerScript = other.collider.GetComponent<PlayerController>();
            playerScript.DecreaseHealth(dmg);
        }

        if (!other.collider.CompareTag("EnemyBullet")) {
            Destroy(this);
        }
    }
}
