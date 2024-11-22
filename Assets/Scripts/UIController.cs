using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance; //CREATING AN INSTANCE OF THIS SCRIPT SO THAT IT CAN BE EASILY ACCESSED FROM ANYWHERE
    private Image fadeScreen; //BLACK SCREEN, WE FADE INTO IT TO MAKE THE TRANSITION BETWEEN LEVELS SMOOTHER

    private void Awake() 
    {
        if(!instance) //IF THERE IS NO INSTANCE YET
        {
            instance = this; //SET THIS AS AN INSTANCE
            DontDestroyOnLoad(gameObject); //AND DON'T DESTROY THIS OBJECT ON LOADING TO ANOTHER SCENE
        }
        else
        {
            Destroy(gameObject); //DESTROY THIS OBJECT OTHERWISE TO NOT HAVE UNNECESSARY COPIES
        }

        fadeScreen = GameObject.Find("Fade Screen").GetComponent<Image>(); //FIND THE FADE SCREEN OBJECT
    }

    [SerializeField] private float fadeSpeed = 2f; //SPEED OF FADING INTO BACK AND BACK FROM IT
    private bool fadingToBlack, fadingFromBlack; //VARIABLES THAT TRACK WHEATHER WE ARE CURRENTLY FADING INTO OR FROM BLACK SCREEN

    // Update is called once per frame
    void Update()
    {
        if(fadingToBlack) //IF START FADE TO BLACK WAS CALLED 
        {
            //CHANGE THE COLOR OF FADING SCREEN (BY DEFAULT IT'S ALPHA CHANNEL IS SET TO 0) BY MOVING TOWARDS THE MAXIMUM ALPHA VALUE WITH THE FADING SPEED
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 1f, fadeSpeed * Time.deltaTime));
            if(fadeScreen.color.a == 1) //IF IT'S BLACK ALREADY STOP FADING TO BLACK
            {
                fadingToBlack = false;
            }
        }
        else if(fadingFromBlack) //IF START FADE FROM BLACK WAS CALLED
        {
            //CHANGE THE COLOR OF FADING SCREEN (IT SHOULD BE 1 BY NOW) BY MOVING TOWARDS THE MINIMUM ALPHA VALUE WITH THE FADING SPEED
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 0f, fadeSpeed * Time.deltaTime));
            if(fadeScreen.color.a == 0f) //IF IT'S TRANSPARENT ALREADY STOP FADING FROM BLACK
            {
                fadingFromBlack = false;
            }
        }
    }

    public void StartFadeToBlack() 
    {
        fadingToBlack = true;
        fadingFromBlack = false;
    }

    public void StartFadeFromBlack()
    {
        fadingToBlack = false;
        fadingFromBlack = true;
    }
}
