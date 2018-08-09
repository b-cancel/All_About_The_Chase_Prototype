using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour {
    
    string defaultPath = "./Assets/Environment Spawner/StreamingAssets";
    public AllFloorsTypes allFloorTiles;
    public AllWallTypes allWallTiles;
    public GameObject tilePrefab;

    public Tile[,] map = new Tile[33,33]; //map needs to be odd length and odd width so there is a center

    //this function is called once the game starts
    public void CreateMap()
    {
        //this function begins by placing all the floors first, and then obstacles, and then props, and finally walls
        //If tiles overlap each other, they are overwritten in precedence floors < obstacles < props < walls
        string filepath = "./Assets/Environment Spawner/StreamingAssets/";
        ReadMapFiles(filepath);
        VisualizeMap();
    }

    void ReadMapFiles(string filepath)
    {
        //starts by reading the floor files
        string currentFile = filepath + "floors.json";
        System.IO.StreamReader file = new System.IO.StreamReader(currentFile);
        string jsonString = file.ReadToEnd();
        allFloorTiles = JsonUtility.FromJson<AllFloorsTypes>(jsonString);//this utility reads the Json files and automatically inputs them into the AllFloorTiles class
        FillFloorTiles();

        currentFile = filepath + "walls.json";
        file = new System.IO.StreamReader(currentFile);
        jsonString = file.ReadToEnd();
        allWallTiles = JsonUtility.FromJson<AllWallTypes>(jsonString);
        FillWallTiles();
    }

    void VisualizeMap()
    {
        for(int count = 0; count<map.GetLength(0);count++)
        {
            for(int count2 = 0; count2<map.GetLength(1);count2++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(count, -count2), Quaternion.identity);
                tile.GetComponent<TileData>().data = map[count, count2];
            }
        }
    }

    void FillFloorTiles()
    {
        foreach(FloorTypes tile in allFloorTiles.floorTypes)
        {
            string tileType = tile.typeName;
            //Debug.Log(tileType);
            foreach(FloorData location in tile.floorData)
            {
                int leftmostX = location.bottomLeft[0];
                int rightmostX = location.topRight[0];
                int upperY = location.topRight[1];
                int lowerY = location.bottomLeft[1];

                int width = rightmostX - leftmostX;
                int height = lowerY - upperY;

                //these variables give us the top left corner of the tile
                int startingX = leftmostX;
                int startingY = upperY;

                for (int count = 0; count <= width; count++)
                {
                    for (int count2 = 0; count2 <= height; count2++)
                    {
                        int[] position = { startingX + count, startingY + count2 };
                        //this if statement, makes sure the coordinates are within the boundaries of the map
                        if (position[0] >= 0 && position[0] < map.GetLength(0) && position[1] >= 0 && position[1] < map.GetLength(1))
                        {
                            //Debug.Log("Creating " + tileType + " in location " + count +  " " + count2);
                            map[position[0], position[1]] = new Tile();
                            map[position[0], position[1]].floorData = location;
                            map[position[0], position[1]].type = tileType;
                        }
                    }
                }
            }
        }

    }

    void FillWallTiles()
    {
        foreach (WallTypes tile in allWallTiles.wallTypes)
        {
            string tileType = tile.typeName;
            //Debug.Log(tileType);
            foreach (WallData location in tile.wallData)
            {
                int leftmostX = location.bottomLeft[0];
                int rightmostX = location.topRight[0];
                int upperY = location.topRight[1];
                int lowerY = location.bottomLeft[1];

                int width = rightmostX - leftmostX;
                int height = lowerY - upperY;

                //these variables give us the top left corner of the tile
                int startingX = leftmostX;
                int startingY = upperY;

                for (int count = 0; count <= width; count++)
                {
                    for (int count2 = 0; count2 <= height; count2++)
                    {
                        int[] position = { startingX + count, startingY + count2 };
                        //this if statement, makes sure the coordinates are within the boundaries of the map
                        if (position[0] >= 0 && position[0] < map.GetLength(0) && position[1] >= 0 && position[1] < map.GetLength(1))
                        {
                            //Debug.Log("Creating " + tileType + " in location " + count + " " + count2);
                            map[position[0], position[1]] = new Tile();
                            map[position[0], position[1]].wallData = location;
                            map[position[0], position[1]].type = "WALL";
                        }
                    }
                }
            }
        }
    }
}
