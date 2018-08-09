using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace game
{
    public class Objective : MonoBehaviour
    {

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Camera.main.GetComponent<GameManager>().EndGame(endState.win);
        }
    }
}