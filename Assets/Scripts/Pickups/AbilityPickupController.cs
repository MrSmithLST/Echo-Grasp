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
            PlayerPrefs.SetInt("CanDoubleJump", 1);
        }
        if(DashUnlock)
        {
            GameManager.instance.CanDash = true;
            PlayerPrefs.SetInt("DashJump", 1);
        }
        if(WallJumpUnlock)
        {
            GameManager.instance.CanWallJump = true;
            PlayerPrefs.SetInt("CanWallJump", 1);
        }

        Destroy(gameObject); //DESTROYING THIS GAME OBJECT AFTER PICKING IT UP
    }
}
