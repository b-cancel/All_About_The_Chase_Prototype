using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallData{
    //position coordinates
    public float[] point1;
    public float[] point2;

    //visual variables
    public int[] tint;
    public int layer;

    //extra wall details
    public bool flip;
}
