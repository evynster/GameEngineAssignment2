using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonGeneration : MonoBehaviour
{
    private static SingletonGeneration _instance;
    public static SingletonGeneration Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SingletonGeneration>();
                if (_instance == null)
                {
                    GameObject temp = new GameObject();
                    temp.name = "Generation Singleton";
                    _instance = temp.AddComponent<SingletonGeneration>();
                    DontDestroyOnLoad(temp);
                }
            }
            return _instance;
        }
        private set { }
    }
    // Start is called before the first frame update

    public int generations = 0;

    void Awake()
    {
        if (_instance = null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
