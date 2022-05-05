using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunAnim : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The shotgun's animator component")]
    private Animator shotgunAnim;

    [SerializeField]
    [Tooltip("The time to reset the animation trigger")]
    private float animTriggerTime;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            StartCoroutine("Rotate");
            Debug.Log("its boom time");
            shotgunAnim.SetTrigger("Rotate");
        }
    }

    IEnumerator Rotate() {
        yield return new WaitForSeconds(animTriggerTime);
        shotgunAnim.ResetTrigger("Rotate");
    }
}
