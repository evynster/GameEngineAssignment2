using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    /*
     * we only want a single instance of input for this game at any given time
     * so I made a in singleton
     */
    private static InputManager _instance;

    public static InputManager Instance{
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();
                if(_instance == null)
                {
                    GameObject temp = new GameObject();
                    temp.name = "InputManager";
                    _instance = temp.AddComponent<InputManager>();
                    DontDestroyOnLoad(temp);
                }
            }
            return _instance;
        }
    }
    private PlayerControls playerControls;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }



    private void OnDisable()
    {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.PlayerMovement.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return playerControls.PlayerMovement.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame()
    {
        return playerControls.PlayerMovement.Jump.triggered;
    }

    public bool PlayerRunning()
    {
        bool result = playerControls.PlayerMovement.Sprint.ReadValue<float>() == 1f ? true : false;
        return result;
    }
    public bool PlayerCrouching()
    {
        bool result = playerControls.PlayerMovement.Crouch.ReadValue<float>() == 1f ? true : false;
        return result;
    }
    public bool PlayerInteractThisFrame()
    {
       return playerControls.PlayerMovement.Interact.triggered;
    }
    public bool PlayerInteracting()
    {
        bool result = playerControls.PlayerMovement.InteractHold.ReadValue<float>() == 1f ? true : false;
        return result;
    }
    public bool PlayerMenuThisFrame()
    {
        return playerControls.PlayerMovement.Menu.triggered;
    }
}
