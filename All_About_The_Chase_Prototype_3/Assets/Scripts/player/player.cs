using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace game
{
    //NOTE: we must be spawned in a tile with 2 ints
    public class player : MonoBehaviour
    {
        //---speed variables

        Vector2 runDirection;

        public float speed;
        private float prevSpeed;

        public float speedToAnimMultiplier;

        public int speedLevel;
        public float speedLevelSize;

        //---ruling tile variables

        Vector2 rulingTile;
        Vector2[] tileCorners;
        Dictionary<Vector2, action> actions;

        // Use this for initialization
        void Start()
        {
            //---speed variables

            runDirection = new Vector2(0, 1);
            speed = 2f;
            prevSpeed = speed;
            speedToAnimMultiplier = 1;

            speedLevel = 3;
            speedLevelSize = .75f; //guarantees that we dont go below

            //level 1 -> .75
            //level 2 -> 1.25
            //level 3 -> 2
            //level 4 -> 2.75
            //level 5 -> 3.5

            //---ruling tile variables

            actions = (new tileMapping()).tileActions; //DEFAULT tile (this will be immediately overwritten)
            newRulingTile(gameObject.transform.position, true);
        }

        void FixedUpdate()
        {
            //---speed management

            speed = (speed == 0) ? prevSpeed : speed; //our speed can never be zero or we will cause errors
            runDirection = (runDirection.normalized * speed);
            gameObject.GetComponent<Rigidbody2D>().velocity = runDirection;
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

            Vector2 potentialRulingTile = (rulingTile + runDirection.normalized);

            if (Vector2.Distance(potentialRulingTile, playerPos) < Vector2.Distance(rulingTile, playerPos))
                newRulingTile(potentialRulingTile, false);
        }

        void newRulingTile(Vector2 newTilePos, bool align)
        {
            //IF I switch out of a tile that has none in front... this means i hit a wall and should lose
            if (actions[runDirection.normalized] == action.none)
                Camera.main.GetComponent<GameManager>().EndGame(endState.lossCrash);

            rulingTile = floatVect_2_intVect(newTilePos);
            tileCorners = getTileCorners(rulingTile);
            //TODO... trigger actions depending on the type of floor this is (for now: [1] floor [2] air)
            //TODO... use this tiles coordinates to reassign the move coordinates in our "actions" dictionary

            if(align)
                playerToRulingTile(); //align the player to the new tile
        }

        Vector2[] getTileCorners(Vector2 center)
        {
            Vector2[] corners = new Vector2[4];
            //NOTE: given that each tile is 1 by 1
            corners[0] = new Vector2(center.x + .5f, center.y + .5f);
            corners[1] = new Vector2(center.x + .5f, center.y - .5f);
            corners[2] = new Vector2(center.x - .5f, center.y - .5f);
            corners[3] = new Vector2(center.x - .5f, center.y + .5f);
            return corners;
        }

        Vector2 floatVect_2_intVect(Vector2 vect)
        {
            vect.x = Mathf.RoundToInt(vect.x);
            vect.y = Mathf.RoundToInt(vect.y);
            return vect;
        }

        //TODO... only lock on one of the players position and not both
        void playerToRulingTile()
        {
            gameObject.transform.position = new Vector2(rulingTile.x, rulingTile.y);
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
                        speedUp(true);
                    else
                        speedUp(false);
                }
                //ELSE... no command triggered
            }
            //ELSE... both are pressed... we can't do both... ignore command

            //---handle directional changes
            if ((left && right) == false)
            {
                if (left || right)
                {
                    if (left)
                    {
                        Vector2 correctedVect = floatVect_2_intVect((Quaternion.AngleAxis(90, Vector3.forward) * runDirection).normalized); //positive is to the left
                        //TODO... remove this test code
                        if (Input.GetKey(KeyCode.Space))
                            executeAction(action.dodge, true);
                        else
                            executeAction(action.turn, true);
                        //executeAction(actions[correctedVect], true); //TODO... actually use this ACTUAL code
                    }
                    else
                    {
                        Vector2 correctedVect = floatVect_2_intVect((Quaternion.AngleAxis(-90, Vector3.forward) * runDirection).normalized); //positive is to the left
                        //TODO... remove this test code
                        if (Input.GetKey(KeyCode.Space))
                            executeAction(action.dodge, false);
                        else
                            executeAction(action.turn, false);
                        //executeAction(actions[correctedVect], false); //TODO... actually use this ACTUAL code
                    }
                }
                //ELSE... no command triggered
            }
            //ELSE... both are pressed... we can't do both... ignore command
        }

        void executeAction(action act, bool left)
        {
            if(act == action.turn || act == action.dodge)
            {
                if (act == action.turn)
                {
                    Quaternion rotation;
                    if (left)
                        rotation = Quaternion.AngleAxis(90, Vector3.forward); //positive is left
                    else
                        rotation = Quaternion.AngleAxis(-90, Vector3.forward);

                    gameObject.transform.rotation = Quaternion.AngleAxis(gameObject.transform.rotation.eulerAngles.z + rotation.eulerAngles.z,Vector3.forward); //rotate our object so the camera rotates with us
                    runDirection = floatVect_2_intVect(rotation * runDirection);//set new running direction
                    newRulingTile(rulingTile + runDirection.normalized, true); //set new ruling tile
                }
                else //TODO... requires repairs
                {
                    //TODO... automatically switch our ruling tile to the one to our left or right
                    print("dodge");
                    Quaternion rotation;
                    if (left)
                        rotation = Quaternion.AngleAxis(90, Vector3.forward); //positive is left
                    else
                        rotation = Quaternion.AngleAxis(-90, Vector3.forward);

                    //camera doesnt need to rotate with us (but it does need to change positions)
                    //running direction stays the same
                    newRulingTile(rulingTile + (Vector2)(rotation * runDirection.normalized), true); //set ruling tile
                }
            }
            else
                print("none");
            //ELSE... the action was none
        }

        void speedUp(bool speedUp)
        {
            if (speedUp)
            {
                if (speedLevel < 5)
                {
                    speed += speedLevelSize;
                    speedLevel++;
                }
            }
            else
            {
                if (1 < speedLevel)
                {
                    speed -= speedLevelSize;
                    speedLevel--;
                }
            }
        }

        //--------------------GUI (for debugging)--------------------

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(gameObject.transform.position, (rulingTile + runDirection.normalized));

            Gizmos.color = Color.cyan;
            drawSquare();

            Gizmos.color = Color.magenta;
            drawX();
        }

        void drawSquare()
        {
            if(tileCorners != null)
            {
                Gizmos.DrawLine(tileCorners[0], tileCorners[1]);
                Gizmos.DrawLine(tileCorners[1], tileCorners[2]);
                Gizmos.DrawLine(tileCorners[2], tileCorners[3]);
                Gizmos.DrawLine(tileCorners[3], tileCorners[0]);
            }
        }

        void drawX()
        {
            if(tileCorners != null)
            {
                Gizmos.DrawLine(tileCorners[0], tileCorners[2]);
                Gizmos.DrawLine(tileCorners[1], tileCorners[3]);
            }
        }
    }
}