using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using lerpKit;

public class follow : MonoBehaviour {

    public GameObject objectToFollow;

    float rotationVelocity;

    private void Start()
    {
        float guideDistance = 360f;
        float guideTime = 1f;
        rotationVelocity = lerpHelper.calcLerpVelocity(guideDistance, guideTime, unitOfTime.seconds, updateLocation.update);
    }

    // Update is called once per frame
    void Update () {
        Vector3 objectPos = objectToFollow.transform.position;
        gameObject.transform.position = new Vector3(objectPos.x, objectPos.y, gameObject.transform.position.z);

        Vector3 objectRot = objectToFollow.transform.rotation.eulerAngles;
        Vector3 ourRot = gameObject.transform.rotation.eulerAngles;
        float newZ = Mathf.LerpAngle(ourRot.z, objectRot.z, lerpHelper.calcLerpValue(ourRot, objectRot, rotationVelocity));
        gameObject.transform.rotation = Quaternion.Euler(ourRot.x, ourRot.y, newZ);
	}
}
