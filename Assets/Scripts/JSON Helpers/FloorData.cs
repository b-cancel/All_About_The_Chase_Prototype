using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloorData{
    //position coordinates (the coordinates of the 2 tiles in the corners)
    public int[] cornerTile1;
    public int[] cornerTile2;

    //visual variables
    public float[] tint; //4 values from (0->1)
    public int layer;

    //effect variable
    public string effect;
}