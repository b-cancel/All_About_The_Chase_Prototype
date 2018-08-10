using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

namespace game
{
    public enum endState { win, lossTime, lossCrash };

    public class GameManager : MonoBehaviour
    {
        public GameObject map;

        public GameObject Timer;
        public float totalTime;
        public float timeLeft;

        void Awake()
        {
            totalTime = 100000; //TODO... change this to something less ridiculous
            timeLeft = totalTime;

            StartCoroutine(map.GetComponent<EnvironManager>().CreateMap());
        }

        void Update()
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