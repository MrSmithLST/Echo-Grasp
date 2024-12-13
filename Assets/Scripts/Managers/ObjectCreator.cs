using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectCreator : MonoBehaviour
{
    public static ObjectCreator instance;

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

    private IEnumerator CreateObjectRoutine(GameObject prefab, Transform target, float delay, bool shouldBeDestroyed)
    {
        Vector3 newPosition = target.position;

        yield return new WaitForSeconds(delay);

        GameObject newObject = Instantiate(prefab, newPosition, Quaternion.identity);

        if(shouldBeDestroyed) Destroy(newObject, 15);
    }
    #endregion

    public void WakeMeUp(GameObject objectToWakeUp, float sleepDuration) => StartCoroutine(WakeUpRoutine(objectToWakeUp, sleepDuration));

    private IEnumerator WakeUpRoutine(GameObject objectToWakeUp, float sleepDuration)
    {
        objectToWakeUp.SetActive(false);

        yield return new WaitForSeconds(sleepDuration);

        objectToWakeUp.SetActive(true);
    }
}

