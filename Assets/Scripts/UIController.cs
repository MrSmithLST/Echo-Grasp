using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance; //CREATING AN INSTANCE OF THIS SCRIPT SO THAT IT CAN BE EASILY ACCESSED FROM ANYWHERE
    private Image _fadeScreen; //BLACK SCREEN, WE FADE INTO IT TO MAKE THE TRANSITION BETWEEN LEVELS SMOOTHER

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

        _fadeScreen = GameObject.Find("Fade Screen").GetComponent<Image>();
    }

    public float fadeSpeed = 2f; //SPEED OF FADING INTO BACK AND BACK FROM IT
    //[SerializeField]
    private bool _fadingToBlack, _fadingFromBlack; //VARIABLES THAT TRACK WHEATHER WE ARE CURRENTLY FADING INTO OR FROM BLACK SCREEN

    // Update is called once per frame
    void Update()
    {
        if(_fadingToBlack) //IF START FADE TO BLACK WAS CALLED 
        {
            //CHANGE THE COLOR OF FADING SCREEN (BY DEFAULT IT'S ALPHA CHANNEL IS SET TO 0) BY MOVING TOWARDS THE MAXIMUM ALPHA VALUE WITH THE FADING SPEED
            _fadeScreen.color = new Color(_fadeScreen.color.r, _fadeScreen.color.g, _fadeScreen.color.b, Mathf.MoveTowards(_fadeScreen.color.a, 1f, fadeSpeed * Time.deltaTime));
            if(_fadeScreen.color.a == 1) //IF IT'S BLACK ALREADY STOP FADING TO BLACK
            {
                _fadingToBlack = false;
            }
        }
        else if(_fadingFromBlack) //IF START FADE FROM BLACK WAS CALLED
        {
            //CHANGE THE COLOR OF FADING SCREEN (IT SHOULD BE 1 BY NOW) BY MOVING TOWARDS THE MINIMUM ALPHA VALUE WITH THE FADING SPEED
            _fadeScreen.color = new Color(_fadeScreen.color.r, _fadeScreen.color.g, _fadeScreen.color.b, Mathf.MoveTowards(_fadeScreen.color.a, 0f, fadeSpeed * Time.deltaTime));
            if(_fadeScreen.color.a == 0f) //IF IT'S TRANSPARENT ALREADY STOP FADING FROM BLACK
            {
                _fadingFromBlack = false;
            }
        }
    }

    public void StartFadeToBlack() 
    {
        _fadingToBlack = true;
        _fadingFromBlack = false;
    }

    public void StartFadeFromBlack()
    {
        _fadingToBlack = false;
        _fadingFromBlack = true;
    }
}
