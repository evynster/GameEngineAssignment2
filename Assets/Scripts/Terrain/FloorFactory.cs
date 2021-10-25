using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorFactory : MonoBehaviour
{


    [SerializeField]
    private GameObject floor = null;
    private static GameObject floorObject;

    [SerializeField]
    private GameObject lvlStart = null;
    private static GameObject levelStart;

    [SerializeField]
    private Material white = null;
    private static Material mat0;
    [SerializeField]
    private Material red = null;
    private static Material mat1;
    [SerializeField]
    private Material orange = null;
    private static Material mat2;
    [SerializeField]
    private Material yellow = null;
    private static Material mat3;
    [SerializeField]
    private Material green = null;
    private static Material mat4;
    [SerializeField]
    private Material blue = null;
    private static Material mat5;
    [SerializeField]
    private Material purple = null;
    private static Material mat6;
    public enum floorColour
    {
        none,
        red,
        orange,
        yellow,
        green,
        blue,
        purple
    }
    private static List<GameObject> floors;
    public static GameObject createFloor(floorColour colour)
    {
        GameObject tempFloor = null;
        for (int i = 0; i < floors.Count; i++)
        {
            if (!floors[i].activeSelf)
            {
                floors[i].SetActive(true);
                 tempFloor = floors[i];
                break;
            }
        }
        
        tempFloor.transform.localScale = new Vector3(200, 200, 200);
        tempFloor.layer = LayerMask.NameToLayer("Ground");

        switch (colour)
        {
            case floorColour.none:
                tempFloor.GetComponent<MeshRenderer>().material = mat0;
                break;
            case floorColour.red:
                tempFloor.GetComponent<MeshRenderer>().material = mat1;
                break;
            case floorColour.orange:
                tempFloor.GetComponent<MeshRenderer>().material = mat2;
                break;
            case floorColour.yellow:
                tempFloor.GetComponent<MeshRenderer>().material = mat3;
                break;
            case floorColour.green:
                tempFloor.GetComponent<MeshRenderer>().material = mat4;
                break;
            case floorColour.blue:
                tempFloor.GetComponent<MeshRenderer>().material = mat5;
                break;
            case floorColour.purple:
                tempFloor.GetComponent<MeshRenderer>().material = mat6;
                break;
            default:
                break;
        }
        return tempFloor;
    }

    private void Awake()
    {
        levelStart = lvlStart;
        floorObject = floor;
        mat0 = white;
        mat1 = red;
        mat2 = orange;
        mat3 = yellow;
        mat4 = green;
        mat5 = blue;
        mat6 = purple;
        floors = new List<GameObject>();

        int floorMax = GetComponent<CreateLevel>().maxSizeX * GetComponent<CreateLevel>().maxSizeY;
        for (int i = 0; i < floorMax; i++)
        {
            floors.Add(Instantiate(floorObject));
            floors[i].transform.SetParent(levelStart.transform);
            floors[i].SetActive(false);
        }


    }
}
