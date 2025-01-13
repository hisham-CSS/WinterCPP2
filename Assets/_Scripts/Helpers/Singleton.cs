using System;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    static T instance;
    public static T Instance
    {
        get
        {
            try
            {
                instance = FindAnyObjectByType<T>();
                if (instance == null) throw new NullReferenceException();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e.ToString());
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
            finally
            {
                //this code always runs whether or not the exception was hit
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (!instance)
        {
            instance = this as T;
            DontDestroyOnLoad(instance);
            return;
        }

        Destroy(gameObject);
    }
}
