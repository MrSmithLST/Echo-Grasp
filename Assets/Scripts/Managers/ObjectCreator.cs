using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectCreator : MonoBehaviour
{
    public static ObjectCreator instance; //CREATING A SINGLETON

    private void Awake() {
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

    #region Creating Objects
    public void CreateObject(GameObject prefab, Transform target, float delay = 0, bool shouldBeDestroyed = false)
    {
        StartCoroutine(CreateObjectRoutine(prefab, target, delay, shouldBeDestroyed));
    }

    //USED TO INSTANTIATE OBJECTS WITH A DELAY
    private IEnumerator CreateObjectRoutine(GameObject prefab, Transform target, float delay, bool shouldBeDestroyed)
    {
        Vector3 newPosition = target.position; //SETTING THE SPAWN POSITION OF THE NEW OBJECT

        yield return new WaitForSeconds(delay); //WAIT IF THERE IS A DELAY

        GameObject newObject = Instantiate(prefab, newPosition, Quaternion.identity); //INSTANTIATING THE OBJECT

        if(shouldBeDestroyed) Destroy(newObject, 15); //IF IT'S A TEMPORATY OBJECT, DESTROY IT AFTER 15 SECONDS
    }
    #endregion

    public void WakeMeUp(GameObject objectToWakeUp, float sleepDuration) => StartCoroutine(WakeUpRoutine(objectToWakeUp, sleepDuration));

    //USED TO DEACTIVATE AND REACTIVATE OBJECTS
    private IEnumerator WakeUpRoutine(GameObject objectToWakeUp, float sleepDuration)
    {
        objectToWakeUp.SetActive(false);

        yield return new WaitForSeconds(sleepDuration);

        objectToWakeUp.SetActive(true);
    }
}

