using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AverageAndZoom : MonoBehaviour {

    public GameObject player1;
    public GameObject player2;

    private void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Chased");
        player2 = GameObject.FindGameObjectWithTag("Chaser");
    }

    public void LateUpdate()
    {
        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;

        float playerDistance = Vector3.Distance(player1Pos, player2Pos);

        this.transform.position = (player1Pos + player2Pos) / 2;
        playerDistance = Mathf.Clamp(playerDistance, 10, Mathf.Infinity);
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -playerDistance);
    }
}