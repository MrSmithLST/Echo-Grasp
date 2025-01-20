using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    [SerializeField] private Rigidbody2D ballRB; //RIGID BODY OF THE BALL ITSELF
    [SerializeField] private float pushForce; //PUSH FORCE WITH WHICH THE BALL WILL START MOVING

    private void Start()
    {
        Vector2 pushVector = new Vector2(pushForce, 0); //BUILD THE PUSH VECTOR

        ballRB.AddForce(pushVector, ForceMode2D.Impulse); //PUSH THE BALL ON START
    }

    private void Update()
    {
        Vector2 pushVector = new Vector2(pushForce, 0); //KEEP PUSHING THE BALL
    }
}