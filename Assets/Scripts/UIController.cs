using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    private void Awake() 
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Image fadeScreen;
    public float fadeSpeed = 2f;
    private bool _fadingToBlack, _fadingFromBlack;

    // Update is called once per frame
    void Update()
    {
        if(_fadingToBlack)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 1f, fadeSpeed * Time.deltaTime));
            if(fadeScreen.color.a == 1)
            {
                _fadingToBlack = false;
            }
        }
        else if(_fadingFromBlack)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 0f, fadeSpeed * Time.deltaTime));
            if(fadeScreen.color.a == 0f)
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
