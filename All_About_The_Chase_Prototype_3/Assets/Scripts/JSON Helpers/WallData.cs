using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallData{
    //position coordinates
    public int[] bottomLeft;
    public int[] topRight;

    //extra wall details
    public bool flip;

    //visual variables
    public int[] tint;
    public int layer;
}
