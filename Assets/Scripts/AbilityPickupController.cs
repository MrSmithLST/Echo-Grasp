using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickupController : MonoBehaviour
{
    public bool DoubleJumpUnlock, DashUnlock, WallJumpUnlock;

    private void OnTriggerEnter2D(Collider2D other) {
        PlayersAbilityTracker player = other.GetComponentInParent<PlayersAbilityTracker>();
        if(DoubleJumpUnlock)
        {
            player.canDoubleJump = true;
        }
        if(DashUnlock)
        {
            player.canDash = true;
        }
        if(WallJumpUnlock)
        {
            player.canWallJump = true;
        }

        Destroy(gameObject);
    }
}
