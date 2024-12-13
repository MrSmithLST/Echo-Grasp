using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private float _pushPower;
    [SerializeField] private float _duration = .5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player)
            player.Push(transform.up * _pushPower, _duration);
        
    }
}
