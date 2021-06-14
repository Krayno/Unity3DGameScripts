using System;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    private CharacterController Controller;
    private float VerticalSpeed;
    private int JumpCount;
    private Vector3 Direction;

    private float Horizontal;
    private float Vertical;

    private KeybindManager KeybindManager;
    public Animator Animator;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        JumpCount = 0;
        KeybindManager = GameObject.Find("KeybindManager").GetComponent<KeybindManager>();
    }

    void Update()
    {
        Horizontal = 0;
        Vertical = 0;

        if (KeybindManager.Keybinds["MoveForward"].IsHeld())
        {
            Vertical = 3;
            Animator.SetBool("isRunning", true);
        }
        if (KeybindManager.Keybinds["MoveBackward"].IsHeld())
        {
            Vertical = -3;
            Animator.SetBool("isRunning", true);
        }

        if (KeybindManager.Keybinds["MoveRight"].IsHeld())
        {
            Horizontal = 3;
            Animator.SetBool("isRunning", true);
        }
        if (KeybindManager.Keybinds["MoveLeft"].IsHeld())
        {
            Horizontal = -3;
            Animator.SetBool("isRunning", true);
        }

        Direction = new Vector3(Horizontal, 0f, Vertical).normalized;

        if (KeybindManager.Keybinds["Jump"].IsPressed() && JumpCount != 1)
        {
            VerticalSpeed = ClientGlobals.JumpSpeed;
            JumpCount += 1;
        }

        if (VerticalSpeed > ClientGlobals.Gravity)
        {
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }

        if (Controller.isGrounded) //Check if the player is on a slope.
        {
            // Animator.SetBool("isFalling", false); // -- FALLING ANIMATION CODE --

            RaycastHit Hit;
            if (Physics.Raycast(Controller.transform.position, Vector3.down, out Hit, 2.25f))
            {
                if (Hit.normal != Vector3.up)
                {
                    if (JumpCount == 0)
                    {
                        VerticalSpeed = -10; //Apply a lot of gravity while on a slope.
                    }
                }
            }
        }
        // -- FALLING ANIMATION CODE --
        //else if (!Physics.Raycast(Controller.transform.position, Vector3.down, 2.25f)) //If the raycast doesn't hit anything fall.
        //{
        //    Animator.SetBool("isFalling", true);
        //}

        if (Direction.x > 0 && Direction.z > 0) //Up-Right
        {
            Direction = transform.forward + transform.right;
            Direction = Direction.normalized;

            Direction.y = VerticalSpeed;
            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x > 0 && Direction.z < 0) //Down-Right
        {
            Direction = -transform.forward + transform.right;
            Direction = Direction.normalized;

            Direction.y = VerticalSpeed;
            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x < 0 && Direction.z < 0) //Down-Left
        {
            Direction = -transform.forward + -transform.right;
            Direction = Direction.normalized;

            Direction.y = VerticalSpeed;
            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x < 0 && Direction.z > 0) //Up-Left
        {
            Direction = transform.forward + -transform.right;
            Direction = Direction.normalized;

            Direction.y = VerticalSpeed;
            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.z < 0) //Down
        {
            Direction = -transform.forward;
            Direction.y = VerticalSpeed;

            Controller.Move(Direction * (ClientGlobals.PlayerSpeed) * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.z > 0) //Up
        {
            Direction = transform.forward;
            Direction.y = VerticalSpeed;

            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x > 0) //Right
        {
            Direction = transform.right;
            Direction.y = VerticalSpeed;

            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x < 0) //Left
        {
            Direction = -transform.right;
            Direction.y = VerticalSpeed;

            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }
        else if (Direction.x == 0 && Direction.y == 0) //Not moving
        {
            Animator.SetBool("isRunning", false);
            Direction.y = VerticalSpeed;
            Controller.Move(Direction * ClientGlobals.PlayerSpeed * Time.deltaTime);
            VerticalSpeed += ClientGlobals.Gravity / 2 * Time.deltaTime; //Apply half gravity every frame.
        }

        if (Controller.isGrounded)
        {
            VerticalSpeed = 0;
            JumpCount = 0;
        }
    }
}
