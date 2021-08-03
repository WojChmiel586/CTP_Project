using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Pickup
{
    public override void OnPickup(GameObject target)
    {
        PlayerControls player = target.GetComponent<PlayerControls>();
        if (player.inventory.keyCount > 0)
        {
            player.inventory.keyCount--;
            switch (player.playerArchetype)
            {
                case PlayerControls.PlayerArchetypes.MELEE:
                    player.GainCoins(Random.Range(1, 12));
                    player.GainArrows(Random.Range(1, 4));
                    player.GainHealth(15);
                    player.melee.meleeDamage += 3;
                    break;
                case PlayerControls.PlayerArchetypes.RANGED:
                    player.GainCoins(Random.Range(1, 5));
                    player.GainArrows(Random.Range(4, 10));
                    player.GainHealth(5);
                    player.ranged.rangedDamage += 5;
                    break;
            }
            Destroy(gameObject);
        }
    }
}
