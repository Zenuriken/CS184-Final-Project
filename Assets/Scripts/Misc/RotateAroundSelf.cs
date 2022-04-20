using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RotateAroundSelf : MonoBehaviour
{
    #region Main Updates
    private void Update()
    {
        transform.rotation *= Quaternion.Euler(0, -5, 0);
    }
    #endregion
}
