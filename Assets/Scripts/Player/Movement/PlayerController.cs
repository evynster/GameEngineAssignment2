using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    public static event System.Action zipLineAction;

    /*
     * menu data
     */
    public bool menu = false;
    private float menuTimer = 0f;

    /*
     * UI Elements data
     */
    [SerializeField]
    private Text hintText=null;

    /*
     * Player movement stuff is here
     */
    public Vector3 playerVelocity;
    [SerializeField]
    public float playerBaseSpeed = 2.0f;
    [SerializeField]
    public float sprintSpeedMult = 2.0f;
    [SerializeField]
    public float crouchSpeedMult = 0.5f;
    private float playerSpeed = 0f;


    [SerializeField]
    public float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.8f;
    public float currentGravityValue = 0;
    public bool playerAffectedByGravity = true;

    private InputManager inputManager;
    
    /*
     * Movement booleans are placed here
     */
    [SerializeField]
    private Transform groundCheck = null;
    [SerializeField]
    private float groundHeight = 0.1f;
    public LayerMask groundMask;
    private bool isGrounded = true;
    [SerializeField]
    private bool SpecialMovement = false;

    /*
     * Crouching Data
     */
    private bool isCrouching = false;
    /*
     * Scaling Data
     */
    private bool canScale = false;
    private GameObject ScaleWallObject;
    private GameObject ScallWallEndObject;
    /*
     *Ladder Data
     */
    private bool canClimb = false;
    private bool isClimbing = false;
    private GameObject ladderBox;
    private GameObject ladderObject;
    [SerializeField]
    private float climbSpeed = 3f;
    [SerializeField]
    private float ladderClampAmount = 115f;
    /*
     * Zipline Data
     */
    private bool isZipping = false;
    private GameObject zipStart;
    private GameObject zipEnd;
    /*
     *  Camera Stuff is placed here
     */

    [SerializeField]
    private Transform cameraTransform = null;
    [SerializeField] 
    private float baseMouseSensitivity = 1.0f;
    private float actualMouseSensitivity = 1.0f;
    private float characterHeight = 2f;

    [SerializeField]
    private float maxPitch = 70f;
    [SerializeField]
    private float minPitch = -70f;
    [SerializeField]
    private float currentPitch = 0f;
    [SerializeField]
    [Range(0f, 0.5f)] float mouseSmoothTime = 0.03f;

    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        Cursor.lockState = CursorLockMode.Locked;
        currentGravityValue = gravityValue;
    }

    void Update()
    {
        menuCheck();//menu is always checked ahead of movement
        if (!menu)
            movement();
        else
            menuFunction();
    }

    /*
     * this function checks to see of we have tried to pause the game while also stopping us from holding the pause key to pause and unpause all the time
     */
    private void menuCheck()
    {
        if (menuTimer>=0f)
            menuTimer -= Time.deltaTime;
        if (inputManager.PlayerMenuThisFrame() && menuTimer < 0f)
        {
            menu = !menu;
            if (menu)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;
            menuTimer = 0.2f;
        }
    }
    private void menuFunction()
    {
        //currently nothing for the menu besides a button
    }
    /*
     * This function handles player movement. Basic movement is put on hold while coroutines handle the special movements
     */
    private void movement()
    {
        if (!SpecialMovement)
        {
            UpdateMouse();
            isGrounded = Physics.CheckSphere(groundCheck.position, groundHeight, groundMask);
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
                gameObject.GetComponent<CharacterController>().stepOffset = 0.25f;
            }
            Vector2 movement = inputManager.GetPlayerMovement();
            Vector3 move = new Vector3(movement.x, 0, movement.y);
            Vector3 cameraMove = cameraTransform.forward;
            cameraMove.y = 0f;
            cameraMove = cameraMove.normalized;
            move = cameraMove * move.z + cameraTransform.right * move.x;
            move.y = 0f;
            //this is checking how fast the player can move, it stops the player from starting a sprint mid air, etc
            if (isGrounded)
            {
                if (isCrouching)
                {
                    playerSpeed = playerBaseSpeed * crouchSpeedMult;
                }
                else if (inputManager.PlayerRunning())
                {
                    Vector2 Move2d = new Vector2(move.x, move.z);
                    Vector2 CamereaDirection2d = new Vector2(controller.transform.forward.x, controller.transform.forward.z);
                    if (Vector2.Angle(Move2d, CamereaDirection2d) == 0f)
                    {
                        playerSpeed = playerBaseSpeed * sprintSpeedMult;
                    }
                    else
                    {
                        playerSpeed = playerBaseSpeed;
                    }
                }
                else
                {
                    playerSpeed = playerBaseSpeed;
                }
            }

            controller.Move(move * Time.deltaTime * playerSpeed);

            // Changes the height position of the player..
            if (inputManager.PlayerJumpedThisFrame() && isGrounded)
            {
                playerVelocity.y += Mathf.Sqrt(-jumpHeight * gravityValue);
                gameObject.GetComponent<CharacterController>().stepOffset = 0.001f;
            }
            if (playerAffectedByGravity)//if we can fall this functions handles that and also limits are downwards velocity
            {
                playerVelocity.y += currentGravityValue * Time.deltaTime;
                if (playerVelocity.y < -10f)
                    playerVelocity.y = -10f;
            }
            else
            {
                playerVelocity.y = 0f;
            }

            controller.Move(playerVelocity * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, cameraTransform.eulerAngles.y, 0);//player rotation should be where they are looking

            Crouching();
            AttemptSpecialMovement();

        }
    }
    
    /*
     * this function handles mouse movement and also smooths out the speed of the movement
     */
    private void UpdateMouse()
    {
        Vector2 targetMouseDelta = inputManager.GetMouseDelta();

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        actualMouseSensitivity = baseMouseSensitivity;

        currentPitch -= currentMouseDelta.y;
        
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        cameraTransform.localEulerAngles = Vector3.right * currentPitch * actualMouseSensitivity;

        transform.Rotate(Vector3.up * currentMouseDelta.x * actualMouseSensitivity);
    }

    /*
     * This function handles crouching and such, it stops you from uncrouching and sticking your head into a ceiling
     */
    private void Crouching()
    {
        if (!isCrouching && inputManager.PlayerCrouching())
        {
            isCrouching = true;
            StartCoroutine(StartCrouching());
        }
        else if (isCrouching && !inputManager.PlayerCrouching()) 
        {
            Ray crouchChecker = new Ray(transform.position,Vector3.up);
            //TODO change to cylinder up check instead of raycast they aren't cheap
            if (!Physics.Raycast(crouchChecker, out RaycastHit hitInfo, 2f, LayerMask.GetMask("Ground")))
            {
                isCrouching = false;
                StartCoroutine(EndCrouching());
            }            
        }
    }

    private IEnumerator StartCrouching()
    {//change to for loop with value based on character height
        for(int i = 0; i < 10; i++)
        {
            controller.height -= 0.1f;
            controller.center -= new Vector3(0f, 0.05f, 0f);
            cameraTransform.position -= new Vector3(0, 0.1f, 0);
            if (!isGrounded)
            {
                transform.Translate(new Vector3(0, 0.15f, 0));
            }
            yield return null;
        }
    }

    private IEnumerator EndCrouching()
    {
        for (int i = 0; i < 10; i++)
        {
            controller.height += 0.1f;
            controller.center += new Vector3(0f, 0.05f, 0f);
            cameraTransform.position += new Vector3(0, 0.1f, 0);
            if (!isGrounded)
            {
                transform.Translate(new Vector3(0, -0.15f, 0));
            }
            yield return null;
        }
    }
    public void AttemptSpecialMovement()
    {
        if (!SpecialMovement)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, 2f))
            {
                switch (hitInfo.collider.gameObject.name)//this is the hint text
                {
                    case "End":
                        hintText.text = "Press Space\nto scale wall";
                        break;
                    case "LadderClimber":
                        hintText.text = "Press Space\nto climb ladder";
                        break;
                    case "ZiplineStart":
                        hintText.text = "Press F\nto zipline";
                        break;
                    case "Goal":
                        hintText.text = "Press F\nto win?";
                        break;
                    default:
                        hintText.text = "";
                        break;
                }
                //Debug.Log(hitInfo.collider.gameObject.name);
                if(inputManager.PlayerJumpedThisFrame())
                    if (hitInfo.collider.gameObject.name == ("End") && canScale)
                    {
                        SpecialMovement = true;
                        hintText.text = "";
                        StartCoroutine(ScalingWall(ScaleWallObject, ScallWallEndObject));
                    }
                    else if (hitInfo.collider.gameObject.name == ("LadderClimber") && canClimb)
                    {
                        isClimbing = true;
                        SpecialMovement = true;
                        hintText.text = "";
                        StartCoroutine(ClimbingLadder(ladderBox));
                    }
                if (inputManager.PlayerInteractThisFrame())
                {
                    if (hitInfo.collider.gameObject.name == ("ZiplineStart"))
                    {
                        isZipping = true;
                        SpecialMovement = true;
                        hintText.text = "";
                        zipStart = hitInfo.collider.gameObject;
                        zipEnd = zipStart.GetComponent<Zipline>().zipEnd;
                        StartCoroutine(Zipline(zipStart,zipEnd));
                    }
                    if(hitInfo.collider.gameObject.name == ("Goal"))
                    {
                        hitInfo.collider.gameObject.SetActive(false);
                        Debug.Log("It took you " + GameObject.FindGameObjectWithTag("Level").GetComponent<CreateLevel>().generateAmount.generations + " level generations to get the goal!");
                    }
                }
                
            }
            else
                hintText.text = "";
        }            
    }

    public void ScaleWall(GameObject Wall, GameObject Ending)
    {
        canScale = true;
        ScaleWallObject = Wall;
        ScallWallEndObject = Ending;   
    }
    public void UnScaleWall()
    {
        canScale = false;
        ScaleWallObject = null;
        ScallWallEndObject = null;
    }


    //TODO : Camera look straight and smooth to that angle
    private IEnumerator ScalingWall(GameObject wall,GameObject ending)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = ending.transform.position;
        Vector3 difference = endPosition - startPosition;

        Vector3 testDirection = ending.transform.position - wall.transform.position;

        testDirection.y = 0f;
        testDirection.Normalize();
        testDirection.y = difference.y;

        while (transform.position.y <= endPosition.y)
        {
            yield return null;//continue
            if (menu)
                continue;
            controller.Move(testDirection * Time.deltaTime * 2f);
            
            yield return null;
        }
        SpecialMovement = false;
    }

    public void ClimbLadder(GameObject _ladderBox, GameObject _ladderObject)
    {
        ladderBox = _ladderBox;
        ladderObject = _ladderObject;
        canClimb = true;
    }

    public void UnClimbLadder()
    {
        canClimb = false;
        ladderBox = null;
        ladderObject = null;
        isClimbing = false;
    }
    /*
     * a seperate mouse function that limits the direction we can turn around based on the ladders direction
     */
    private void UpdateClimbingMouse()
    {
        if (!isClimbing)
            return;
        Vector3 ladderDirection = ladderBox.transform.position - ladderObject.transform.position;
        ladderDirection.y = 0f;
        ladderDirection.Normalize();
        
        Vector2 targetMouseDelta = inputManager.GetMouseDelta();

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
        
        actualMouseSensitivity = baseMouseSensitivity;
        
        currentPitch -= currentMouseDelta.y;
        
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        
        cameraTransform.localEulerAngles = Vector3.right * currentPitch * actualMouseSensitivity;
        
        Vector2 ladderDirection2D = new Vector2(ladderDirection.x, ladderDirection.z);//

        GameObject tempObject = new GameObject();
        tempObject.transform.position = transform.position;
        tempObject.transform.rotation = transform.rotation;
        tempObject.transform.Rotate(Vector3.up * currentMouseDelta.x * actualMouseSensitivity);//
        
        Vector2 cameraDirection2D = new Vector2(tempObject.transform.forward.x, tempObject.transform.forward.z);//

        float ladderFloat = Vector2.Angle(cameraDirection2D, ladderDirection2D);//
        if (ladderFloat> ladderClampAmount)
            transform.Rotate(Vector3.up * currentMouseDelta.x * actualMouseSensitivity);
        Destroy(tempObject);
    }

    private IEnumerator ClimbingLadder(GameObject ladder)
    {
        currentGravityValue = 0f;
        float ladderClimbExitDelay = 0f;
        yield return null;
        while (isClimbing)
        {
            yield return null;
            if (menu)
                continue;
            if (inputManager.PlayerJumpedThisFrame())
                isClimbing = false;

            controller.Move(new Vector3(0f, inputManager.GetPlayerMovement().y * Time.deltaTime * climbSpeed, 0f));
            UpdateClimbingMouse();
            ladderClimbExitDelay += Time.deltaTime;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundHeight, groundMask);
            if (isGrounded && ladderClimbExitDelay > 0.5f)
                isClimbing = false;
            yield return null;
        }

        currentGravityValue = gravityValue;
        SpecialMovement = false;
        yield return null;
    }

    private IEnumerator Zipline (GameObject Start, GameObject End)
    {
        # region observer
        zipLineAction?.Invoke();
        # endregion
        float zipLineSpeed = 1/Start.GetComponent<Zipline>().zipTime;
        float zipLinePercentage = 0f;
        Vector3 startPos = Start.transform.position + new Vector3(0, -characterHeight, 0);//characterHeight will bite me in the distant future
        Vector3 endPos = End.transform.position + new Vector3(0, -characterHeight, 0);
        transform.position = startPos;
        isZipping = true;
        while (zipLinePercentage<1f)
        {
            yield return null;
            if (menu)
                continue;
            zipLinePercentage += Time.deltaTime * zipLineSpeed;
            transform.position = Vector3.Lerp(startPos,endPos,zipLinePercentage);
            UpdateMouse();
            yield return null;
        }
        SpecialMovement = false;
        isZipping = false;
        yield return null;
    }
}