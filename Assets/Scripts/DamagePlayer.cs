using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{

    [SerializeField] private int _damageAmount = 1; //AMOUNT OF DAMAGE THAT THIS ENEMY/OBSTACLE IS DEALING TO THE PLAYER
    [SerializeField] private bool _destroyOnDamage; //VARIABLE THAT DETERMINES IF THIS OBJECT IS MENT TO BE DESTROYED AFTER DEALING DAMAGE TO THE PLAYER
    [SerializeField] private GameObject _destroyEffect; //EFFECT THAT IS BEING CREATED IF THIS OBJECT IS DESTROYED ON COLLISION

    private void OnCollisionEnter2D(Collision2D other) //IN CASE THIS SCRIPT IS ATTACHED TO A COLLIDER
    {
        if(other.gameObject.tag == "Player")
        {
            DealDamage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) //IN CASE THIS SCRIPT IS ATTACHED TO A TRIGGER
    {
        if(other.gameObject.tag == "Player")
        {
            DealDamage();
        }
    }

    void DealDamage() 
    {
       PlayerHealthController.instance.DamagePlayer(_damageAmount); //COMMUNICATING WITH THE PLAYER HEALTH CONTROLLER AND SENDING THE DAMAGE AMOUNT
       Player.instance.Knockback();

       if(_destroyOnDamage) //IF THIS GAME OBJECT IS MENT TO BE DESTROYED
       {
            if(_destroyEffect)
            {
                Instantiate(_destroyEffect, transform.position, Quaternion.identity); //CREATE THIS EFFECT IF IT EXISTS
            }

            Destroy(gameObject); //AND DESTROY YOURSELF
       }
    }
}
