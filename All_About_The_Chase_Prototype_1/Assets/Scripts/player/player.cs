using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pairingKit;

namespace game
{
    //NOTE: we must be spawned in a tile with 2 ints
    public class player : MonoBehaviour
    {
        //---speed variables

        sbyte[] runDirection;

        //current and prev speed
        float speed;
        private float prevSpeed;

        //how animations match up with speed
        public float speedToAnimMultiplier;

        //speed levels
        int speedMinLevel;
        int speedMaxLevel;
        int speedLevel;
        float speedLevelSize;

        //---ruling tile variables

        int[] rulingTile;
        Dictionary<ushort, tileEdgeAction> dir_2_Action;

        //---death variables

        bool forwardMeansDeath;

        //---varaibles to adjust

        //NOTE: this value should be as short as possible so it doesnt feel unfair to lose but it doesnt look like the game is broken
        [Range(0,1f)] //NOTE: 1 is instantaneous, .5 will feel unfair, .25 should feel fair (with some buffer before the player dies)
        public float closestDistBeforeCrash;

        public bool alwaysSnapBoth;
        public bool turningChangesRulingtile;

        public bool allowTurningWhenSprinting;

        // Use this for initialization
        void Start() //THIS MUST BE START NOT AWAKE
        {
            //---variables to adjust

            closestDistBeforeCrash = .25f;

            alwaysSnapBoth = false;
            turningChangesRulingtile = false;

            allowTurningWhenSprinting = true;

            //---speed variables

            runDirection = new sbyte[] { 0, 1 };
            speed = 2.75f;
            prevSpeed = speed;
            speedToAnimMultiplier = .5f;

            speedMinLevel = 1;
            speedMaxLevel = 2;
            speedLevel = 1;
            speedLevelSize = 3.5f; //guarantees that we dont go below

            //---death variables

            forwardMeansDeath = false;

            //---ruling tile variables

            //DEFAULT tile (this will be immediately overwritten BUT is required to avoid NULL errors)
            dir_2_Action = Camera.main.GetComponent<GameManager>().map.GetComponent<EnvironManager>().tileAction_2_DefaultMapping[tileAction.allwaysTurnTEST].tileActions; 
            newRulingTile(vect_2_arr(gameObject.transform.position), true, true);
        }

        void FixedUpdate()
        {
            //---speed management

            speed = (speed == 0) ? prevSpeed : speed; //our speed can never be zero or we will cause errors
            gameObject.GetComponent<Rigidbody2D>().velocity = arr_2_vect(runDirection) * speed;
            gameObject.GetComponent<Animator>().speed = (speed * speedToAnimMultiplier);
            prevSpeed = speed;

            //---ruling tile management 

            checkForNewRulingTile();
        }

        // Update is called once per frame
        void Update()
        {
            keyHandling();
        }

        //--------------------Fixed Update Helpers--------------------

        //NOTE: since we are moving forward 
        //the only tile that could possibly be ahead of us be a potential ruler is the one ahead of us
        void checkForNewRulingTile()
        {
            Vector2 playerPos = gameObject.transform.position;
            Vector2 runDirVect = arr_2_vect(runDirection);
            Vector2 ruleTileVect = arr_2_vect(rulingTile);
            Vector2 potentialRulingTile = runDirVect + ruleTileVect;

            //---check for death

            //the direction we are moving in has a no move command (there is a wall in front of us and we know its position)
            Vector2 forwardEdge = arr_2_vect(rulingTile) + (runDirVect.normalized/2); //forward edge is half a tile from the tile's center in the direction the player is running
            if (Vector2.Distance(forwardEdge, playerPos) < closestDistBeforeCrash)
                if(forwardMeansDeath)
                    Camera.main.GetComponent<GameManager>().EndGame(endState.lossCrash); 

            //---check if we are in a new ruling tile

            if (Vector2.Distance(potentialRulingTile, playerPos) < Vector2.Distance(ruleTileVect, playerPos))
                newRulingTile(vect_2_arr(potentialRulingTile), false, false);
        }

        //--------------------Update Helpers--------------------

        void keyHandling()
        {
            //-----READ

            //---read speed changes
            bool faster = (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow));
            bool slower = (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow));

            //---read directional changes
            bool left = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow));
            bool right = (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow));

            //-----HANDLE

            //---handle speed changes
            if ((faster && slower) == false)
            {
                if (faster || slower)
                {
                    if (faster)
                    {
                        if (speedLevel < speedMaxLevel)
                        {
                            speed += speedLevelSize;
                            speedLevel++;
                        }
                    }
                    else
                    {
                        if (speedMinLevel < speedLevel)
                        {
                            speed -= speedLevelSize;
                            speedLevel--;
                        }
                    }
                }
                //ELSE... no command triggered
            }
            //ELSE... both are pressed... we can't do both... ignore command

            //---handle directional changes
            if ((left && right) == false)
            {
                if (left || right)
                {
                    sbyte[] correctedVect = vect_2_byteArr((Quaternion.AngleAxis((90 * ((left) ? 1 : -1)), Vector3.forward) * arr_2_vect(runDirection)).normalized); //positive is to the left
                    ushort key = _2tuple.combine(correctedVect[0], correctedVect[1]);
                    executeAction(dir_2_Action[key], left);
                }
                //ELSE... no command triggered
            }
            //ELSE... both are pressed... we can't do both... ignore command
        }

        void executeAction(tileEdgeAction act, bool left)
        {
            if(act == tileEdgeAction.turn || act == tileEdgeAction.dodge)
            {
                Quaternion rotation = Quaternion.AngleAxis((90 * ((left) ? 1 : -1)), Vector3.forward); //positive is left

                bool snapX = true;
                bool snapY = true;

                if (act == tileEdgeAction.turn)
                {
                    bool notSprinting = (speedLevel != speedMaxLevel) ? true : false;
                    bool sprinting = (speedLevel == speedMaxLevel) ? true : false;
                    if( notSprinting || (sprinting && allowTurningWhenSprinting) )
                    {
                        gameObject.transform.rotation = Quaternion.AngleAxis(gameObject.transform.rotation.eulerAngles.z + rotation.eulerAngles.z, Vector3.forward); //rotate our object so the camera rotates with us
                        runDirection = vect_2_byteArr(rotation * arr_2_vect(runDirection));

                        //from rundirection determine which axis we should snap to
                        if (alwaysSnapBoth == false)
                        {
                            bool runningUpOrDown = (Mathf.Approximately(runDirection[0], 0)) ? true : false;
                            snapX = (runningUpOrDown) ? true : false;
                            snapY = (runningUpOrDown) ? false : true;
                        }

                        if (turningChangesRulingtile)
                            newRulingTile(vect_2_arr(arr_2_vect(rulingTile) + arr_2_vect(runDirection)), snapX, snapY);
                        else
                            newRulingTile(rulingTile, snapX, snapY); //we are rotating on the same square... there is no need to switch ruling tile manually
                    }
                }
                else
                {
                    //from rundirection determine which axis we should snap to
                    if (alwaysSnapBoth == false)
                    {
                        bool runningUpOrDown = (Mathf.Approximately(runDirection[0], 0)) ? true : false;
                        snapX = (runningUpOrDown) ? true : false;
                        snapY = (runningUpOrDown) ? false : true;
                    }

                    newRulingTile(vect_2_arr(arr_2_vect(rulingTile) + (Vector2)(rotation * arr_2_vect(runDirection))), snapX, snapY); //set new ruling tile
                }
            }
        }

        //--------------------For Update and Fixed Update--------------------

        void newRulingTile(int[] newTilePos, bool alignX, bool alignY)
        {
            rulingTile = newTilePos;

            dir_2_Action = tileToMapping(rulingTile);
            ushort forwardKey = _2tuple.combine(runDirection[0], runDirection[1]);
            forwardMeansDeath = (dir_2_Action[forwardKey] == tileEdgeAction.none) ? true : false;

            //NOTE: we could add the ability to lock onto only one or both of these coordinates... 
            //but given current implementation this works fine...
            if (alignX) 
                gameObject.transform.position = new Vector2(rulingTile[0], gameObject.transform.position.y);
            if (alignY)
                gameObject.transform.position = new Vector2(gameObject.transform.position.x, rulingTile[1]);
        }

        //--------------------Helper Functions--------------------

        Dictionary<ushort, tileEdgeAction> tileToMapping(int[] center)
        {
            GameObject map = Camera.main.GetComponent<GameManager>().map;

            ulong tileKey = _2tuple.combine(center[0], center[1]);

            tileAction tileAct;
            Dictionary<ushort, tileEdgeAction> tileActions;

            if (map.GetComponent<EnvironManager>().tileID_2_tileAction.ContainsKey(tileKey)) //we are in bounds
            {
                if (map.GetComponent<EnvironManager>().tileID_2_TileEdgeActionsID.ContainsKey(tileKey)) //tile is SPECIAL
                {
                    uint tileActs = map.GetComponent<EnvironManager>().tileID_2_TileEdgeActionsID[tileKey];
                    tileActions = map.GetComponent<EnvironManager>().tileEdgeActionID_2_tileMapping[tileActs].tileActions;
                }
                else //tile is NOT special
                {
                    tileAct = map.GetComponent<EnvironManager>().tileID_2_tileAction[tileKey];
                    tileActions = map.GetComponent<EnvironManager>().tileAction_2_DefaultMapping[tileAct].tileActions; //this tile isnt special... use defulat options
                }
            }
            else //we are OUT OF BOUNDS
            {
                //TODO... this should never happen in the actual game
                tileAct = tileAction.allwaysTurnTEST;
                tileActions = map.GetComponent<EnvironManager>().tileAction_2_DefaultMapping[tileAct].tileActions;
            }

            //TODO... trigger actions depending go the type of floor this is (ONLY IF YOU WANT TO... because our onGUI uses this function too)

            return tileActions;
        }

        //--------------------REQUIRED because our pairing functions require 2 ints exactly... but we also need the vector counter part to do math with in other functions

        int[] vect_2_arr(Vector2 vect)
        {
            return new int[] {Mathf.RoundToInt(vect.x), Mathf.RoundToInt(vect.y)};
        }

        sbyte[] vect_2_byteArr(Vector2 vect)
        {
            return new sbyte[] { (sbyte)Mathf.RoundToInt(vect.x), (sbyte)Mathf.RoundToInt(vect.y) };
        }

        Vector2 arr_2_vect(int[] arr)
        {
            return new Vector2(arr[0], arr[1]);
        }

        Vector2 arr_2_vect(sbyte[] arr)
        {
            return new Vector2(arr[0], arr[1]);
        }

        //--------------------GUI (for debugging)--------------------

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Color yourTileColor = Color.red;
                Color otherTileColors = Color.blue;
                int tileRad = 3;

                //---For NOT our own tile

                drawInstructs(tileRad, rulingTile, otherTileColors);

                //---For our OWN tile

                //draw our distance from our new square
                Gizmos.color = Color.red;
                Gizmos.DrawLine(gameObject.transform.position, (arr_2_vect(rulingTile) + arr_2_vect(runDirection)));

                //draw a square and X on the square we are on
                Vector2[] tileCorners = getTileCorners(rulingTile);
                drawSquare(tileCorners, yourTileColor);
                drawX(tileCorners, yourTileColor);
            }
        }

        void drawInstructs(int tileRadius, int[] center, Color tileColor)
        {
            int[] start = new int[] { center[0] - tileRadius, center[1] - tileRadius }; //bottom left
            int[] end = new int[] { center[0] + tileRadius, center[1] + tileRadius }; //top right

            for(int row = start[0]; row <= end[0]; row++)
                for(int col = start[1]; col <= end[1]; col++)
                        drawTileInstructs(new int[] { row, col }, tileColor);
        }

        void drawTileInstructs(int[] center, Color tileColor)
        {
            //basic outline for each tile
            Vector2[] tileCorners = getTileCorners(center);
            drawSquare(tileCorners, tileColor);
            drawX(tileCorners, tileColor);

            Dictionary<ushort, tileEdgeAction> thisEdgeDir_2_edgeAction = tileToMapping(center);

            //instruction based renderings
            for (byte i=0; i<4; i++)
            {
                sbyte[] thisDir = Camera.main.GetComponent<GameManager>().map.GetComponent<EnvironManager>().dirID_2_dir[i];
                tileEdgeAction thisDirAction = thisEdgeDir_2_edgeAction[_2tuple.combine(thisDir[0],thisDir[1])];
                Color thisDirActionColor = Camera.main.GetComponent<GameManager>().map.GetComponent<EnvironManager>().tileEdgeAction_2_Color[thisDirAction];
                float[] starCenter = new float[] { (float)(center[0] + (thisDir[0] / 4.0)), (float)(center[1] + (thisDir[1] / 4.0)) };
                drawStar(starCenter, .1f, thisDirActionColor);
            }
        }

        void drawStar(float[] center, float size, Color starColor)
        {
            Gizmos.color = starColor;
            Gizmos.DrawLine(new Vector2(center[0] - size, center[1]), new Vector2(center[0] + size, center[1]));
            Gizmos.DrawLine(new Vector2(center[0], center[1] - size), new Vector2(center[0], center[1] + size));
        }

        Vector2[] getTileCorners(int[] center)
        {
            Vector2[] corners = new Vector2[4];
            //NOTE: given that each tile is 1 by 1
            corners[0] = new Vector2(center[0] + .5f, center[1] + .5f);
            corners[1] = new Vector2(center[0] + .5f, center[1] - .5f);
            corners[2] = new Vector2(center[0] - .5f, center[1] - .5f);
            corners[3] = new Vector2(center[0] - .5f, center[1] + .5f);
            return corners;
        }

        void drawSquare(Vector2[] tileCorners, Color squareColor)
        {
            Gizmos.color = squareColor;
            Gizmos.DrawLine(tileCorners[0], tileCorners[1]);
            Gizmos.DrawLine(tileCorners[1], tileCorners[2]);
            Gizmos.DrawLine(tileCorners[2], tileCorners[3]);
            Gizmos.DrawLine(tileCorners[3], tileCorners[0]);
        }

        void drawX(Vector2[] tileCorners, Color xColor)
        {
            Gizmos.color = xColor;
            Gizmos.DrawLine(tileCorners[0], tileCorners[2]);
            Gizmos.DrawLine(tileCorners[1], tileCorners[3]);
        }
    }
}