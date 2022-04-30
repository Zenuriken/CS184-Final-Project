using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pl = collision.gameObject.GetComponent<PlayerController>();
            if (pl.key_amount() >= 1)
            {
                GetComponentInParent<Transform>().position += new Vector3(0, 3, 0);
                pl.key_decrease();
            }
        }
    }
}
