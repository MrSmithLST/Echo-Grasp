using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickupController : MonoBehaviour
{
    public bool DoubleJumpUnlock, DashUnlock, WallJumpUnlock; //VARIABLE RESPONSIBLE FOR DETERMINING WHAT KIND OF UPGRADE THIS PICKUP IS

    private void OnTriggerEnter2D(Collider2D other) {
        
        if(DoubleJumpUnlock) //UNLOCKING WHATEVER THIS ITEM WAS MENT TO UNLOCK
        {
            GameManager.instance.CanDoubleJump = true;
        }
        if(DashUnlock)
        {
            GameManager.instance.CanDash = true;
        }
        if(WallJumpUnlock)
        {
            GameManager.instance.CanWallJump = true;
        }

        Destroy(gameObject); //DESTROYING THIS GAME OBJECT AFTER PICKING IT UP
    }
}
