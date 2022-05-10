using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Amount of damage per bullet")]
    private float dmg;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController playerScript = other.GetComponent<PlayerController>();
            playerScript.DecreaseHealth(dmg);
            //Debug.Log("hit player");
        }

        if (!other.CompareTag("EnemyBullet")) {
            Destroy(this);
        }
    }
}
