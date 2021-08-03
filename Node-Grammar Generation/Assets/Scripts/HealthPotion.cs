using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : Pickup
{
    public override void OnPickup(GameObject target)
    {
        target.GetComponent<PlayerControls>().GainHealth(20);
        Destroy(gameObject);
    }
}
