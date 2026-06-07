using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            Init();
            return instance;
        }
    }

    private static void Init()
    {
        if(instance != null)
        {
            return;
        }
        else
        {
            instance = FindAnyObjectByType<T>();
            
            if(instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                instance = obj.AddComponent<T>();    
            }
        }
    }

    protected virtual void Awake()
    {
        if(gameObject != Instance.gameObject)
        {
            Destroy(gameObject);
        }
    }
}