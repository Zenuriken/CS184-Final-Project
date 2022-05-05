using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBlast : MonoBehaviour
{

    private void OnCollisionEnter(Collision other) {
        if (other.collider.CompareTag("Enemy")) {
            Debug.Log("Hit enemy!");
            other.collider.GetComponent<EnemyController>().DecreaseHealth(10);
        }
    }
}
