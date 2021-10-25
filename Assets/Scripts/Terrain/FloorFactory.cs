using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorFactory : MonoBehaviour
{


    [SerializeField]
    private GameObject floor;
    private static GameObject floorObject;

    [SerializeField]
    private Material red;
    private static Material mat1;
    [SerializeField]
    private Material orange;
    private static Material mat2;
    [SerializeField]
    private Material yellow;
    private static Material mat3;
    [SerializeField]
    private Material green;
    private static Material mat4;
    [SerializeField]
    private Material blue;
    private static Material mat5;
    [SerializeField]
    private Material purple;
    private static Material mat6;
    public enum floorColour
    {
        red,
        orange,
        yellow,
        green,
        blue,
        purple
    }
    public static GameObject createFloor(floorColour colour)
    {
        GameObject temp = GameObject.Instantiate(floorObject);
        switch (colour)
        {
            case floorColour.red:
                temp.GetComponent<MeshRenderer>().material = mat1;
                break;
            case floorColour.orange:
                temp.GetComponent<MeshRenderer>().material = mat2;
                break;
            case floorColour.yellow:
                temp.GetComponent<MeshRenderer>().material = mat3;
                break;
            case floorColour.green:
                temp.GetComponent<MeshRenderer>().material = mat4;
                break;
            case floorColour.blue:
                temp.GetComponent<MeshRenderer>().material = mat5;
                break;
            case floorColour.purple:
                temp.GetComponent<MeshRenderer>().material = mat6;
                break;
            default:
                break;
        }
        return temp;
    }

    private void Awake()
    {
        floorObject = floor;
        mat1 = red;
        mat2 = orange;
        mat3 = yellow;
        mat4 = green;
        mat5 = blue;
        mat6 = purple;
    }
}
