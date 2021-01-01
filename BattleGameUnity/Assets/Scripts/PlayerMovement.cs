﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public int playerNumber = 1;
    public float gravity = -9.81f;

    public MarksController marksController;
    public Transform respawnPoint;
    private bool canMove = true;
    private bool death = false;
    private float resurectionTime = 0.0f;

    //for gravity and jumps
    private Vector3 velocity;
    private bool isGrounded = false;
    public Transform groundCheck;
    private float groundDistance = 0.4f;
    public LayerMask groundMask;
    public float jumpHeight = 1f;

    //Player stats
    public int maxHealth;
    public int health;
    public int attack;
    public int distantAttack;
    public GameObject distantAttackObject;

    private float horizontal = 0f;
    private float vertical = 0f;
    private bool isRunning = false;
    private float nextAttack = 0f;
    public float coolDownTime = 0.75f;
    public float coolDownMelee = 0.5f;

    //melee attacks
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public string tagName = "Player1";


    private void Start() {
        controller = this.GetComponent<CharacterController>();

        health = maxHealth;
        marksController.SetMaxHealth(maxHealth);

    }

    void Update()
    {
        if (canMove == true)
        {
            // checking if we are on the ground or not
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            //it calculates the horizontal and vertical number and it checks if it is running or not.
            PlayerControllers();

            //finally we calculate the direction where the player moves outputted as a Vector 3
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            //applying the rotation
            if (direction.magnitude >= 0.1f)
            {
                float turnSmoothTime = 0.1f;
                //this is the war angle where the player moves
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                //this is an smoother angle which progresively rotates his head
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothTime, 0.1f);

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                if (isRunning)
                { controller.Move(direction * runSpeed * Time.deltaTime); }
                else
                { controller.Move(direction * walkSpeed * Time.deltaTime); }
            }

            velocity.y += gravity * Time.deltaTime;

            //the equation for gravity is time squared and hence we need to multiply it twice
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            if (death == true)
            {
                this.transform.position = respawnPoint.position;
                resurectionTime = Time.time + 3f;
                death = false;
            }
            else
            {
                if (Time.time > resurectionTime)
                {
                    canMove = true;
                }
            }
            
        }
    }

    public void TakeDamage (int damage)
    {
        health -= damage;

        //after taking damage or recuperating life health should never be less than 0 or more than the maxHealth
        if (health > maxHealth)
        { health = maxHealth; }
        else if (health < 0)
        { health = 0; }

        marksController.SetHealth(health);
        if (health <= 0)
        {
            respawn();
        }
    }

    void respawn ()
    {
        canMove = false;
        death = true;
        health = maxHealth;
        marksController.resurrection();
    }

    void PlayerControllers ()
    {
        horizontal = 0f;
        vertical = 0f;
        isRunning = false;

        if (playerNumber == 1)
        {
            if (Input.GetKey(KeyCode.F))
            { horizontal = horizontal - 1f; }

            if (Input.GetKey(KeyCode.H))
            { horizontal = horizontal + 1f; }

            if (Input.GetKey(KeyCode.G))
            { vertical = vertical - 1f; }

            if (Input.GetKey(KeyCode.T))
            { vertical = vertical + 1f; }

            //Distant attack
            if (Input.GetKey(KeyCode.S))
            {
                // we can only make one attack for each 0.75 seconds
                if (nextAttack <= Time.time)
                {
                    Quaternion rotation = Quaternion.Euler(0, this.transform.eulerAngles.y, 0);
                    Instantiate(distantAttackObject, this.transform.position, rotation);
                    nextAttack = Time.time + coolDownTime;
                }
            }

            //melee attack
            if (Input.GetKey(KeyCode.D))
            {
                // we can only make one attack for each 0.5 seconds
                if (nextAttack <= Time.time)
                {
                    Debug.Log("Melee Attack");
                    nextAttack = Time.time + coolDownMelee;
                    
                    Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange);

                    foreach(Collider enemy in hitEnemies)
                    {
                        Debug.Log("you hit "+  enemy.name);
                        //checking if it hits the other players but not the current one
                        if (enemy.gameObject.layer == 9 && enemy.gameObject.tag != tagName)
                        {
                            enemy.gameObject.GetComponent<PlayerMovement>().TakeDamage(attack);
                        }
                    }
                }
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity);
                TakeDamage(3);
            }

            //run
            if (Input.GetKey(KeyCode.A))
            { isRunning = true; }
        }
        else if (playerNumber == 2)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            { horizontal = horizontal - 1f; }

            if (Input.GetKey(KeyCode.RightArrow))
            { horizontal = horizontal + 1f; }

            if (Input.GetKey(KeyCode.DownArrow))
            { vertical = vertical - 1f; }

            if (Input.GetKey(KeyCode.UpArrow))
            { vertical = vertical + 1f; }


            //running
            if (Input.GetKey(KeyCode.K))
            { isRunning = true; }

            //melee attack
            if (Input.GetKey(KeyCode.P))
            {
                // we can only make one attack for each 0.5 seconds
                if (nextAttack <= Time.time)
                {
                    Debug.Log("Melee Attack");
                    nextAttack = Time.time + coolDownMelee;

                    Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange);

                    foreach (Collider enemy in hitEnemies)
                    {
                        Debug.Log("you hit " + enemy.name);
                        //checking if it hits the other players but not the current one
                        if (enemy.gameObject.layer == 9 && enemy.gameObject.tag != tagName)
                        {
                            enemy.gameObject.GetComponent<PlayerMovement>().TakeDamage(attack);
                        }
                    }
                }
            }

            //Distant attack
            if (Input.GetKey(KeyCode.L))
            {
                // we can only make one attack for each 0.75 seconds
                if (nextAttack <= Time.time)
                {
                    Quaternion rotation = Quaternion.Euler(0, this.transform.eulerAngles.y, 0);
                    Instantiate(distantAttackObject, this.transform.position, rotation);
                    nextAttack = Time.time + coolDownTime;
                }
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.O) && isGrounded)
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity);
                TakeDamage(3);
            }
        }
    }

    /* Drawing the range for melee attack. Only used for developing purposes
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }*/
}


