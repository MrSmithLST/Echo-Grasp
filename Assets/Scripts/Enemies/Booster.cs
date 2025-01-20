using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private float pushPower; //POWER OF THE BOOSTER
    [SerializeField] private float duration = .5f; //DURATION OF BLOCKING THE PLAYER'S MOVEMENT

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>(); //IF THE PLAYER GET'S IN CONTACT WITH THE BOOSTER

        if(player)
            player.Push(transform.up * pushPower, duration); //BOOST HIM IN THE DIRECTION OF THE BOOSTER
        
    }
}
