using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Pickup
{
    public override void OnPickup(GameObject target)
    {
        target.GetComponent<PlayerControls>().inventory.keyCount++;
        Destroy(gameObject);
    }
}
