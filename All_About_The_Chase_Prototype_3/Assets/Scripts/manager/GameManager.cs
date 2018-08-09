using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

namespace game
{
    public enum endState { win, lossTime, lossCrash };
    public enum action { none, turn, dodge };

    public class tileMapping
    {
        public Dictionary<Vector2, action> tileActions;

        //---constructors

        public tileMapping()
        {
            tileActions = new Dictionary<Vector2, action>();
            tileActions.Add(Vector2.up, action.turn); //n
            tileActions.Add(Vector2.right, action.turn); //e
            tileActions.Add(Vector2.down, action.turn); //s
            tileActions.Add(Vector2.left, action.turn); //w
        }
    }

    public class GameManager : MonoBehaviour
    {
        public GameObject Timer;
        public float totalTime;
        public float timeLeft;

        private void Start()
        {
            totalTime = 1000f;
            timeLeft = totalTime;
        }

        private void Update()
        {
            runTimer();
        }
        void runTimer()
        {
            timeLeft -= Time.deltaTime;
            Timer.GetComponent<Text>().text = (timeLeft).ToString("0");
            if (timeLeft < 5.5)
                Timer.GetComponent<Text>().color = Color.red;

            if (timeLeft <= 0)
                EndGame(endState.lossTime);
        }

        public void EndGame(endState result)
        {
            if (result == endState.win)
                SceneManager.LoadScene("Win");
            else
            {
                if (result == endState.lossTime)
                    SceneManager.LoadScene("LossTime");
                else
                    SceneManager.LoadScene("LossCrash");
            }
        }
    }
}