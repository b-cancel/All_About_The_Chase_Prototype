using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using pairingKit;

namespace game
{
    //-------------------------ENUMERABLES

    //you can move on a tile that is not obstructed [MOVE]
    //you can move through a tile where you [TRIP] but you cannot execute actions while on the tile (and you get a minor speed kill)
    //you can't move through a tile where you [FAINT]
    //you can't move through a tile where you [FALL] a falling animation will play... 
    //  your colliders will collide with walls... 
    //  you will get smaller until you disapear or fall to your death below
    public enum tileAction : byte { move, trip, faint, fall, allwaysTurnTEST, allwaysDodgeTEST, allwaysNothingTEST };

    public enum tileEdgeAction : byte { none, dodge, turn }; //0 -> n

    //-------------------------TIlE MAPPING OBJECT

    public class tileMapping
    {
        public Dictionary<ushort, tileEdgeAction> tileActions;

        //constructor
        public tileMapping()
        {
            tileActions = new Dictionary<ushort, tileEdgeAction>();
            tileActions.Add(_2tuple.combine((sbyte)0, (sbyte)1), tileEdgeAction.none); //n
            tileActions.Add(_2tuple.combine((sbyte)1, (sbyte)0), tileEdgeAction.none); //e
            tileActions.Add(_2tuple.combine((sbyte)0, (sbyte)-1), tileEdgeAction.none); //s
            tileActions.Add(_2tuple.combine((sbyte)-1, (sbyte)0), tileEdgeAction.none); //w
        }
    }

    //-------------------------ENVIRONMENT MANAGER

    public class EnvironManager : MonoBehaviour
    {
        //-----where we store our json files for the TEST map

        string defaultJSON;

        //-----prefab references

        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject wallCornerPrefab;

        //----------public vars

        Dictionary<Vector2, GameObject> vector2ToWallCorner;

        //---primary vars

        //based on (int,int) -> ulong [tileID] --> something that will point to its tileMapping
        public Dictionary<ulong, tileAction> tileID_2_tileAction; //used in REGULAR cases
        public Dictionary<ulong, uint> tileID_2_TileEdgeActionsID; //used in SPECIAL cases

        //based on tileAction -or- tileEdgeActionsID (Actions as in plural) --> tileMapping
        public Dictionary<tileAction, tileMapping> tileAction_2_DefaultMapping; //used in REGULAR cases
        public Dictionary<uint, tileMapping> tileEdgeActionID_2_tileMapping; //used in SPECIAL cases

        //---for debugging

        //used to indicate a color for a particular edge action for debugging purposes
        public Dictionary<tileEdgeAction, Color> tileEdgeAction_2_Color;
        //used to make iteration through all directions simpler with a for loop
        public Dictionary<byte, sbyte[]> dirID_2_dir;

        // Use this for initialization
        public IEnumerator CreateMap()
        {
            //NOTE: secondary var then primary vars

            //---secondary vars

            tileEdgeAction_2_Color = new Dictionary<tileEdgeAction, Color>();
            tileEdgeAction_2_Color.Add(tileEdgeAction.none, Color.red);
            tileEdgeAction_2_Color.Add(tileEdgeAction.dodge, Color.black);
            tileEdgeAction_2_Color.Add(tileEdgeAction.turn, Color.green);

            dirID_2_dir = new Dictionary<byte, sbyte[]>();
            dirID_2_dir.Add(0, new sbyte[] { 0, 1 });
            dirID_2_dir.Add(1, new sbyte[] { 1, 0 });
            dirID_2_dir.Add(2, new sbyte[] { 0, -1 });
            dirID_2_dir.Add(3, new sbyte[] { -1, 0 });

            //---primary vars

            tileID_2_tileAction = new Dictionary<ulong, tileAction>();
            tileID_2_TileEdgeActionsID = new Dictionary<ulong, uint>();

            //NOTE: must be in this order (below)
            tileEdgeActionID_2_tileMapping = new Dictionary<uint, tileMapping>();
            setAllPossibleMappings();
            tileAction_2_DefaultMapping = new Dictionary<tileAction, tileMapping>();
            setDefaultMappings();
            //NOTE: must be in this order (above)

            vector2ToWallCorner = new Dictionary<Vector2, GameObject>();

            //---read in JSON files
            defaultJSON = Application.streamingAssetsPath;
            yield return readFloors();
            yield return readWalls();
        }

        string getJsonStringFromFile(string filePath)
        {
            StreamReader file = new StreamReader(filePath);
            string jsonString = file.ReadToEnd();
            return jsonString;
        }

        //-------------------------Preperation Step

        //NOTE: if you have 3 actions per edge (none, turn, dodge) and you have 4 edges...
        //you have a total of 3*3*3*3 = 81 possible mappings of tiles
        //this assumes the orientation of the tile MATTERS EX: (none, turn, none, turn) != (turn, none, turn, none)
        //this assumes the orientation of the player to the tile also DOES NOT MATTER
        //IF it did... then 81 mapping * 4 orientation the player can come in would creates 324 possible mappings of tiles
        //NOTE: in either case it makes sense to use this system because there are bound to be alot more than 324 tiles in the game
        void setAllPossibleMappings()
        {
            //GIVEN...
            //tileEdgeAction : byte { none, dodge, turn }; //0 -> 2
            //orientation of player DOES NOT MATTER
            //orientation of tile DOES and is static

            Dictionary<byte, tileEdgeAction> byte_2_TileEdgeAction = new Dictionary<byte, tileEdgeAction>();
            //NOTE: this has to match up with our tileEdgeAction enum
            byte_2_TileEdgeAction.Add(0, tileEdgeAction.none);
            byte_2_TileEdgeAction.Add(1, tileEdgeAction.dodge);
            byte_2_TileEdgeAction.Add(2, tileEdgeAction.turn);

            for (byte n=0; n < 3; n++)
            {
                for (byte e = 0; e < 3; e++)
                {
                    for (byte s = 0; s < 3; s++)
                    {
                        for (byte w = 0; w < 3; w++)
                        {
                            tileMapping tempTileMapping = new tileMapping();
                            sbyte[] zero = dirID_2_dir[0];
                            tempTileMapping.tileActions[_2tuple.combine(zero[0], zero[1])] = byte_2_TileEdgeAction[n]; //N
                            sbyte[] one = dirID_2_dir[1];
                            tempTileMapping.tileActions[_2tuple.combine(one[0], one[1])] = byte_2_TileEdgeAction[e]; //E
                            sbyte[] two = dirID_2_dir[2];
                            tempTileMapping.tileActions[_2tuple.combine(two[0], two[1])] = byte_2_TileEdgeAction[s]; //S
                            sbyte[] three = dirID_2_dir[3];
                            tempTileMapping.tileActions[_2tuple.combine(three[0], three[1])] = byte_2_TileEdgeAction[w]; //W

                            //we have the tileMapping set... now we simply map the 4 number to it
                            tileEdgeActionID_2_tileMapping.Add(_4tuple.combine(n, e, s, w),tempTileMapping); //I assume this passes by REFERENCE
                        }
                    }
                }
            }
        }

        void setDefaultMappings()
        {
            //GIVEN...
            //tileAction:byte {move, trip, faint, fall };
            uint alwaysDodge = _4tuple.combine((byte)tileEdgeAction.dodge, (byte)tileEdgeAction.dodge, (byte)tileEdgeAction.dodge, (byte)tileEdgeAction.dodge);
            uint alwaysTurn = _4tuple.combine((byte)tileEdgeAction.turn, (byte)tileEdgeAction.turn, (byte)tileEdgeAction.turn, (byte)tileEdgeAction.turn);
            uint alwaysNothing = _4tuple.combine((byte)tileEdgeAction.none, (byte)tileEdgeAction.none, (byte)tileEdgeAction.none, (byte)tileEdgeAction.none);

            //---TEST mappings

            tileAction_2_DefaultMapping.Add(tileAction.allwaysDodgeTEST, tileEdgeActionID_2_tileMapping[alwaysDodge]);
            tileAction_2_DefaultMapping.Add(tileAction.allwaysTurnTEST, tileEdgeActionID_2_tileMapping[alwaysTurn]);
            tileAction_2_DefaultMapping.Add(tileAction.allwaysNothingTEST, tileEdgeActionID_2_tileMapping[alwaysNothing]);

            //---actual mappings

            tileAction_2_DefaultMapping.Add(tileAction.move, tileEdgeActionID_2_tileMapping[alwaysDodge]);
            tileAction_2_DefaultMapping.Add(tileAction.trip, tileEdgeActionID_2_tileMapping[alwaysNothing]);
            tileAction_2_DefaultMapping.Add(tileAction.faint, tileEdgeActionID_2_tileMapping[alwaysNothing]);
            tileAction_2_DefaultMapping.Add(tileAction.fall, tileEdgeActionID_2_tileMapping[alwaysNothing]);
        }

        //-------------------------Reading in Data

        IEnumerator readFloors()
        {
            //mapping to make reading in effect from JSON easier
            Dictionary<string, tileAction> string_2_TileAction = new Dictionary<string, tileAction>();
            //GIVEN tileAction:byte {move, trip, faint, fall };
            string_2_TileAction.Add("move", tileAction.move);
            string_2_TileAction.Add("trip", tileAction.trip);
            string_2_TileAction.Add("faint", tileAction.faint);
            string_2_TileAction.Add("fall", tileAction.fall);

            //test
            string_2_TileAction.Add("TURN", tileAction.allwaysTurnTEST);
            string_2_TileAction.Add("DODGE", tileAction.allwaysDodgeTEST);
            string_2_TileAction.Add("NONE", tileAction.allwaysNothingTEST);

            GameObject floorFolder = new GameObject("Floors");
            floorFolder.transform.parent = this.transform;

            //read in the json file
            string floorFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, "floors.json");
            string floorFile;

            if (floorFilePath.Contains("://"))
            {
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(floorFilePath);
                yield return www.SendWebRequest();
                floorFile = www.downloadHandler.text;
            }
            else
                floorFile = getJsonStringFromFile(floorFilePath);

            AllFloorTypes allFloorTiles = JsonUtility.FromJson<AllFloorTypes>(floorFile);

            foreach (FloorTypes aType in allFloorTiles.floorTypes) //loop all different floor types
            {
                string objName = aType.floorObjectName;

                //TODO... actually do something different depending on the objName 
                //FOR NOW... we spawn the exact same object (floorPrefab)

                //print("name: " + objName + " " + aType.floorList.Length);
            
                foreach (FloorData aFloor in (aType.floorList)) //loop through all tile with the floor type
                {
                    //---Read In Data From JSON (understand the readMe.txt first)

                    Vector2 cornerTile1 = new Vector2(aFloor.cornerTile1[0], aFloor.cornerTile1[1]);
                    Vector2 cornerTile2 = new Vector2(aFloor.cornerTile2[0], aFloor.cornerTile2[1]);
                    //TODO... read in tint
                    //TODO... read in layer
                    tileAction thisTileAction = string_2_TileAction[aFloor.effect];

                    //---Create some variables from Json

                    float width = Mathf.Abs(cornerTile1[0] - cornerTile2[0]) + 1;
                    float height = Mathf.Abs(cornerTile1[1] - cornerTile2[1]) + 1;

                    //---Spawn in the Floor 

                    GameObject floorTile = Instantiate(floorPrefab, floorFolder.transform); //spawn and set under folder
                    floorTile.GetComponent<SpriteRenderer>().size = new Vector2(width, height); //set size
                    floorTile.transform.position = Vector2.Lerp(cornerTile1, cornerTile2, .5f); //set center
                    //TODO... set tint
                    //TODO... set layer

                    //---Go through all the units this floor covers and mark them as a floor in the dictionary

                    int leftX = (int)Mathf.RoundToInt(Mathf.Min(cornerTile1[0], cornerTile2[0]));
                    int rightX = (int)Mathf.RoundToInt(Mathf.Max(cornerTile1[0], cornerTile2[0]));
                    int lowerY = (int)Mathf.RoundToInt(Mathf.Min(cornerTile1[1], cornerTile2[1]));
                    int upperY = (int)Mathf.RoundToInt(Mathf.Max(cornerTile1[1], cornerTile2[1]));

                    for (int xCoverage = leftX; xCoverage <= rightX; xCoverage++)
                    {
                        for (int yCoverage = lowerY; yCoverage <= upperY; yCoverage++)
                        {
                            ulong z = _2tuple.combine(xCoverage, yCoverage);
                            if (tileID_2_tileAction.ContainsKey(z)) //NOTE: this lets us overwrite the previous commands we had here by default
                                tileID_2_tileAction[z] = thisTileAction;
                            else
                                tileID_2_tileAction.Add(z, thisTileAction);
                        }
                    }  
                }
            }
        }

        IEnumerator readWalls()
        {
            GameObject wallFolder = new GameObject("Walls");
            wallFolder.transform.parent = this.transform;

            GameObject cornerFolder = new GameObject("Corners");
            cornerFolder.transform.parent = this.transform;

            string wallFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, "walls.json");
            string wallFile;

            if (wallFilePath.Contains("://"))
            {
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(wallFilePath);
                yield return www.SendWebRequest();
                wallFile = www.downloadHandler.text;
            }
            else
                wallFile = getJsonStringFromFile(wallFilePath);

            AllWallTypes allWallTiles = JsonUtility.FromJson<AllWallTypes>(wallFile);

            //read in the walls from our json file (from prefabs our walls should be set)
            foreach (WallTypes aType in allWallTiles.wallTypes) //for each type of wall
            {
                string objName = aType.wallObjectName;

                //TODO... actually do something different depending on the objName 
                //FOR NOW... we spawn the exact same object (wallPrefab)

                foreach (WallData aWall in aType.wallList)
                {
                    //---Read In Data From JSON (understand the readMe.txt first)

                    Vector2 point1 = new Vector2(aWall.point1[0], aWall.point1[1]);
                    Vector2 point2 = new Vector2(aWall.point2[0], aWall.point2[1]);
                    //TODO... read in tint
                    //TODO... read in layer
                    //TODO... read in flip

                    //---Create some vars from JSON

                    //NOTE: one of these will be 0... the other will be some number
                    int xDist = (int)Mathf.Abs(point1[0] - point2[0]);
                    int yDist = (int)Mathf.Abs(point1[1] - point2[1]);
                    float maxDist = Mathf.Max(xDist, yDist);

                    List<float[]> pointsBetweenTiles = new List<float[]>(); //all the points between 2 tiles that this wall effects

                    bool horizontalWall = (xDist > yDist);

                    //NOTE: our height is always the largest because we need the sprite to spread out in a particular direction
                    float height = maxDist;
                    float width = (1 / 3f);

                    if (horizontalWall) //horizontal wall
                    {
                        float sameY = point1.y;

                        //given this horizontal wall what are all the points between tiles that we will be effecting
                        float leftMostX = Mathf.Min(point1.x, point2.x);
                        leftMostX += .5f; //this is because we start at the very edge of the wall... and this should really be the x center of the leftmost tile

                        for(int extra = 0; extra < maxDist; extra++) //2nd tile because first was taken care off above
                            pointsBetweenTiles.Add(new float[] { leftMostX + extra, sameY });
                    }
                    else
                    {
                        float sameX = point1.x;

                        //given this vertical wall what are all the points between tiles that we will be effecting
                        float bottomMostY = Mathf.Min(point1.y, point2.y);
                        bottomMostY += .5f; //this is because we start at the very edge of the wall... and this should really be the x center of the leftmost tile

                        for (int extra = 0; extra < maxDist; extra++)
                            pointsBetweenTiles.Add(new float[] { sameX, bottomMostY + extra });
                    }

                    //---Spawn in the Wall

                    GameObject wallTile = Instantiate(wallPrefab, wallFolder.transform); //spawn and set under folder
                    wallTile.GetComponent<SpriteRenderer>().size = new Vector2(width, height); //set size
                    wallTile.transform.position = Vector2.Lerp(point1, point2, .5f); //set center
                    if (horizontalWall)
                        wallTile.transform.rotation = Quaternion.Euler(0, 0, 90);
                    //TODO... set tint
                    //TODO... set layer
                    //TODO... set flip

                    //---Spawn in the Wall Corners (for both corners)
                    if(vector2ToWallCorner.ContainsKey(point1) == false)
                    {
                        GameObject newCorner = Instantiate(wallCornerPrefab, cornerFolder.transform);
                        newCorner.transform.position = point1;
                    }
                    if(vector2ToWallCorner.ContainsKey(point2) == false)
                    {
                        GameObject newCorner = Instantiate(wallCornerPrefab, cornerFolder.transform);
                        newCorner.transform.position = point2;
                    }

                    //---Go through all the units this wall covers and have them effect the 2 floors to the left and right

                    foreach (float[] arr in pointsBetweenTiles) //for each segment (horizontal or vertical)
                    {
                        for (float f = -.05f, iteration = 1; f < 1; f++, iteration++) //for each of the 2 walls affected per segment
                        {
                            float tileX = 0;
                            float tileY = 0;
                            if (horizontalWall)
                            {
                                tileX = Mathf.RoundToInt(arr[0]);
                                tileY = Mathf.RoundToInt(arr[1] + f);
                            }
                            else
                            {
                                tileX = Mathf.RoundToInt(arr[0] + f);
                                tileY = Mathf.RoundToInt(arr[1]);
                            }

                            int tileXInt = Mathf.RoundToInt(tileX);
                            int tileYInt = Mathf.RoundToInt(tileY);

                            ulong z = _2tuple.combine(tileXInt, tileYInt);
                            if (tileID_2_tileAction.ContainsKey(z))
                            {
                                tileAction regularAction = tileID_2_tileAction[z];
                                tileMapping regularMaping = tileAction_2_DefaultMapping[regularAction];
                                tileMapping regularMapingCopy = new tileMapping();

                                ushort n = _2tuple.combine((sbyte)0, (sbyte)1);
                                ushort e = _2tuple.combine((sbyte)1, (sbyte)0);
                                ushort s = _2tuple.combine((sbyte)0, (sbyte)-1);
                                ushort w = _2tuple.combine((sbyte)-1, (sbyte)0);

                                regularMapingCopy.tileActions[n] = regularMaping.tileActions[n];
                                regularMapingCopy.tileActions[e] = regularMaping.tileActions[e];
                                regularMapingCopy.tileActions[s] = regularMaping.tileActions[s];
                                regularMapingCopy.tileActions[w] = regularMaping.tileActions[w];

                                ushort dir = 0;
                                if (horizontalWall)
                                {
                                    //first bottom THEN top
                                    if (iteration == 1)
                                        dir = _2tuple.combine((sbyte)0, (sbyte)1);
                                    else
                                        dir = _2tuple.combine((sbyte)0, (sbyte)-1);
                                }
                                else
                                {
                                    //first left THEN right (so towards right and then left)
                                    if (iteration == 1)
                                        dir = _2tuple.combine((sbyte)1, (sbyte)0);
                                    else
                                        dir = _2tuple.combine((sbyte)-1, (sbyte)0);
                                }

                                //---other code

                                regularMapingCopy.tileActions[dir] = tileEdgeAction.none;

                                byte north = (byte)regularMapingCopy.tileActions[n];
                                byte east = (byte)regularMapingCopy.tileActions[e];
                                byte south = (byte)regularMapingCopy.tileActions[s];
                                byte west = (byte)regularMapingCopy.tileActions[w];

                                //map this new special tile to a number
                                uint newTileEdgeActionsID = _4tuple.combine(north, east, south, west);

                                if (tileID_2_TileEdgeActionsID.ContainsKey(z))
                                    tileID_2_TileEdgeActionsID[z] = newTileEdgeActionsID;
                                else
                                    tileID_2_TileEdgeActionsID.Add(z, newTileEdgeActionsID);
                            }
                            //ELSE... this tile is out of bounds we cant assign instructions to it
                        }
                    }
                }
            }
        }
    }
}