using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
   
   private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player")
        {
            RespawnController.instance.SetSpawn(transform.position); //COMUNICATING WITH THE RESPAWN CONTROLLER AND SETTING THE NEW SPAWN POINT TO THE CURRENT POSITION
        }
   }
}
