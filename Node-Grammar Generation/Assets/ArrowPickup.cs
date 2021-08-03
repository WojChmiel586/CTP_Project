using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPickup : Pickup
{
    public override void OnPickup(GameObject target)
    {
        target.GetComponent<PlayerControls>().GainArrows(Random.Range(1,5));
        Destroy(gameObject);
    }
}
