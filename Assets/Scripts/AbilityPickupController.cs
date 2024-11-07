using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickupController : MonoBehaviour
{
    public bool DoubleJumpUnlock, DashUnlock, WallJumpUnlock; //VARIABLE RESPONSIBLE FOR DETERMINING WHAT KIND OF UPGRADE THIS PICKUP IS

    private void OnTriggerEnter2D(Collider2D other) {
        PlayersAbilityTracker player = other.GetComponentInParent<PlayersAbilityTracker>(); //GETTING THE PLAYER ABILITY TRACKER FROM THE PLAYER ON TRIGGER ENTER
        if(DoubleJumpUnlock) //UNLOCKING WHATEVER THIS ITEM WAS MEANT TO UNLOCK
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

        Destroy(gameObject); //DESTROYING THIS GAME OBJECT AFTER PICKING IT UP
    }
}
