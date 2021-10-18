using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    // Start is called before the first frame update
    public float zipTime = 0f;
    [HideInInspector]
    public GameObject zipEnd = null;
    void Start()
    {
        zipEnd = transform.parent.Find("ZiplineEnd").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
