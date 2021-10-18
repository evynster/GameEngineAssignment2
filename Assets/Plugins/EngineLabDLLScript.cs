using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class EngineLabDLLScript : MonoBehaviour
{

    [DllImport("EnginesLabDLL")]
    private static extern int AddTwoIntegers(int i1, int i2);

    [DllImport("EnginesLabDLL")]
    private static extern int SubtractTwoIntegers(int i1, int i2);
    [DllImport("EnginesLabDLL")]
    private static extern int MultiplyTwoIntegers(int i1, int i2);
    [DllImport("EnginesLabDLL")]
    private static extern int DivideTwoIntegers(int i1, int i2);


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(AddTwoIntegers(3, 2));
        Debug.Log(SubtractTwoIntegers(3, 2));
        Debug.Log(MultiplyTwoIntegers(3, 2));
        Debug.Log(DivideTwoIntegers(3, 2));
    }
}
