using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace game
{
    //NOTE: has current unimplemented boost code

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {

        public float startingSpeed;
        public float speed;
        public float speedMultiplier;
        Rigidbody2D rb;
        public int playerNum;
        string player;
        GameManager gameManager;
        public float stamina;
        float maxStamina;
        bool wastedStamina = false;

        private void Start()
        {
            speedMultiplier = 1.5f;
            maxStamina = 5f;
            stamina = maxStamina;
            gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            startingSpeed = 500f;
            speed = startingSpeed;
            rb = this.GetComponent<Rigidbody2D>();
            player = " Player " + playerNum;
            if (playerNum == 2)
            {
                this.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;
            }
        }

        private void Update()
        {
            float movementX = Input.GetAxisRaw("Horizontal" + player);
            float movementY = Input.GetAxisRaw("Vertical" + player);

            if (Input.GetButton("Sprint" + player) && stamina > 0)
            {
                Debug.Log("Sprinting");
                speed = speedMultiplier * startingSpeed;
                stamina -= Time.deltaTime;
                if (stamina <= 0)
                {
                    wastedStamina = true;
                    stamina = -5f;
                }
            }
            else
            {
                speed = startingSpeed;
                stamina += Time.deltaTime;
            }
            stamina = Mathf.Clamp(stamina, Mathf.NegativeInfinity, maxStamina);
            Vector3 velocity = new Vector2(movementX, movementY);
            velocity *= speed * Time.deltaTime;

            this.rb.velocity = velocity;
        }
    }
}