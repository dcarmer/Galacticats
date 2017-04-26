using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    void Update()
    {
        if(Application.loadedLevel == 8 || Application.loadedLevel == 0)
        {
            Destroy(gameObject);
        }
    }
    }
