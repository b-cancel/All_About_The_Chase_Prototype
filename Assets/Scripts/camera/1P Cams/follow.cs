using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lerpKit;

public class follow : MonoBehaviour {

    public GameObject player;

    [Range(0,1)]
    public float timeFor90DegRot;
    float lerpVelocity;

    void Awake()
    {
        timeFor90DegRot = 1f; //in seconds
    }
	
	// Update is called once per frame
	void Update () {
        lerpVelocity = lerpHelper.calcLerpVelocity(90, timeFor90DegRot, unitOfTime.seconds, updateLocation.Update); //TODO... this should need to be adjusted after variable is established

        followPosition();

        smoothFollowRotation();
    }

    void followPosition()
    {
        Vector3 newPos = player.transform.position; ;
        newPos.z = this.transform.position.z;
        this.transform.position = newPos;
    }

    void smoothFollowRotation()
    {
        Vector3 playerRot = (player.transform.rotation).eulerAngles;
        Vector3 camRot = (this.transform.rotation).eulerAngles;  

        if (Mathf.Approximately(camRot.z, playerRot.z) == false)
        {
            //---cover edge cases
            float difference = Mathf.Abs(playerRot.z - camRot.z);
            if(difference > 180)
            {
                if (playerRot.z < camRot.z)
                    playerRot.z += 360;
                else
                    camRot.z += 360;
            }

            //extra code so that linear interpolation take X ammount of time for every 90 degrees
            float lerpValue = lerpHelper.calcLerpValue(camRot.z, playerRot.z, lerpVelocity);

            //lerp z value
            float zValue = Mathf.Lerp(camRot.z, playerRot.z, lerpValue);

            //apply rotation
            this.transform.rotation = Quaternion.Euler(playerRot.x, playerRot.y, zValue);
        }
    }
}
