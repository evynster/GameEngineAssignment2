using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MultiTag : MonoBehaviour
{
    public List<string> tags;
    public bool searchTags(string tag) {
        foreach (string i in tags)
        {
            if (i == tag)
                return true;
        }
        return false;
    }

    public List<GameObject> searchGameObjectsWithTag(string tag)
    {
        GameObject[] tempList;
        tempList = GameObject.FindGameObjectsWithTag("MultiTag");
        List<GameObject> myList = new List<GameObject>();
        foreach (GameObject i in tempList)
        {
            MultiTag temp = i.GetComponent<MultiTag>();
            if (temp.searchTags(tag))
                myList.Add(i);
        }
        return myList;
    }
}
