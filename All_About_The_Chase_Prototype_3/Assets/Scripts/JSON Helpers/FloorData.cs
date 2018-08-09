using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloorData{
    //position coordinates
    public int[] bottomLeft;
    public int[] topRight;

    //visual variables
    public int[] tint;
    public int layer;
}