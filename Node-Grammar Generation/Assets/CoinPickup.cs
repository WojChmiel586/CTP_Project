using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : Pickup
{
    public override void OnPickup(GameObject target)
    {
        target.GetComponent<PlayerControls>().GainCoins(Random.Range(1,3));
        Destroy(gameObject);
    }
}
