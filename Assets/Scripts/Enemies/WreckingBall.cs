using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    [SerializeField] private Rigidbody2D ballRB;
    [SerializeField] private float pushForce;

    private void Start()
    {
        Vector2 pushVector = new Vector2(pushForce, 0);

        ballRB.AddForce(pushVector, ForceMode2D.Impulse);
    }

    private void Update()
    {
        Vector2 pushVector = new Vector2(pushForce, 0);
    }
}