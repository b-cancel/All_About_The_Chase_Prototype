using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironManager : MonoBehaviour {

    string defaultJSON = "./Assets/Environment Spawner/StreamingAssets";
    public GameObject mapCreatorPrefab;
    public MapCreator mapCreatorScript;

    // Use this for initialization
    void Awake()
    {
        //Instantiate map, begin map creation
        GameObject mapCreator = Instantiate(mapCreatorPrefab, Vector3.zero, Quaternion.identity);
        mapCreatorScript = mapCreator.GetComponent<MapCreator>();
        mapCreatorScript.CreateMap();

        //read in the floors from our json file (from prefabs our floors should be set)
        for (int i = 0; i < 10; i++)
        {
            //for each floor tile...

            //spawn in the floor

            //go through all the programmatic tile this tile includes and mark those as a floor inside of a dictionary
        }

        //read in the walls from our json file (from prefabs our walls should be set)
        for (int i = 0; i < 10; i++)
        {
            //for each wall tile...

            //spawn in the wall

            //find the 2 tile it affects

            for (int s = 0; s < 10; s++)
            {
                //for each 1 unit segment between the two points...

                //point on one end of the segment, and point at the other end of the segment are the edge of some tile
                //the point inbetween +-.5 to both sides prependicular to the wall will tell you the coordinates of all the tiles affected by this wall

                for (int f = 0; f < 2; f++)
                {
                    //for each of the 2 tiles affected per segment

                    //if the tile exists... mark its special casses (that a command is not triggered here because only idiots run into walls)
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
