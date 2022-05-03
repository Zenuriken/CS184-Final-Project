using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBlast : Ability
{
    private Transform camTransform;


    public override void Use(Vector3 spawnPos) {
        Debug.Log("BLaST");
    }
}
