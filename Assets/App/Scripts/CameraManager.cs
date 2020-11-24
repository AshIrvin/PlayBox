using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.App.Scripts
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; set; }

        #region Private Variables

        private SpawnPrefab spawnPrefab;
        private GameObject head, player;
        private Rigidbody playerRb;
        private bool crouch;

        private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
        private bool isPanning;     // Is the camera being panned?
        private bool isRotating;    // Is the camera being rotated?
        private bool isZooming;     // Is the camera zooming?

        private bool buttonHeld;
        private bool mouseControl;
        [SerializeField] private bool orthoMode;
        [SerializeField] private bool flyMode;

        #endregion

        #region Public Variables

        [Header("GameObjects")]
        public GameObject appManager;
        public GameObject playerGeometry;
        public GameObject selectedGameObject;
        public List<GameObject> selectedGameObjects;

        public Camera cam;

        public float strafe, moveUp, moveForward;
        public float jumpHeight = 7f;

        [Header("Mouse movement")]
        public float turnSpeed = 10.0f;      // Speed of camera turning when mouse moves in along an axis
        public float playerTurnSpeed = 10.0f;
        public float panSpeed = 2.0f;       // Speed of the camera when being panned
        public float zoomSpeed = 2.0f;      // Speed of the camera going back and forth

        public bool canSpawn;

        [HideInInspector]
        public Color tempColor;

        [Header("Joystick settings")]
        public VJHandler jsMovement;
        public VJHandlerAim jsAim;
        public float moveSpeed = 0.2f;
        public float aimTurnSpeed = 1f;

        [Header("Spawning settings")]
        public float minDistanceToSpawnObject = 2;
        public float spawnTime = 0.1f;
        
        [Tooltip("TBA")]
        public float holdTimeForMenu = 0.7f;

        [Header("SoldierMode")]
        public GameObject bulletEmitter;
        public GameObject bullet;
        public float bulletSpeed = 200f;

        [HideInInspector]
        public int objects;

        public Texture2D defaultTexture;
        public Texture2D tex2d;

        [Header("Colour Palette")]
        public Texture2D tex;

        #endregion

        #region Private Variables

        private MeshRenderer tempMeshRenderer;
        private Color selectedColor;

        private float heldTime;
        private float Timer = 0;
        private float spawnDistance = 2.5f;

        private Vector3 direction;
        private float xMin, xMax, yMin, yMax, heightMin, heightMax, zMin, zMax;
        private float playerOffset = 2f;

        private Quaternion headRot;
        private Quaternion playerRot;
        private Quaternion angleSpeed = new Quaternion(0, 1, 0, 0);

        private Vector3 Origin;
        private Vector3 Difference;
        
        private bool Drag;

        private Color32[] pixels;
        private float cWidth, cHeight;

        #endregion

        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        private void Start()
        {
            spawnPrefab = appManager.GetComponent<SpawnPrefab>();

            strafe = 0.25f;
            moveUp = 1f;
            moveForward = 0.2f;

            SetupPlayer();

            crouch = false;
            flyMode = true;

            SwitchToPlayer();

            tempColor = Color.white;

            // testing clamping
            xMax = 50; 
            xMin = -50;
            yMax = 50;
            yMin = -50;
            zMin = -50;
            zMax = 50;
            heightMin = 0;
            heightMax = 50;

            pixels = tex.GetPixels32();
            cWidth = tex.width;
            cHeight = tex.height;

            headRot = head.transform.rotation;
            playerRot = player.transform.rotation;

            canSpawn = true;

            RemoveHighlightColour();

            playerGeometry.SetActive(false);
        }


        // Update is called once per frame
        private void Update()
        {
            CheckForMouseClicks();

            MouseMovement();

            JoystickMove();
            JoystickAim();

            ResetCamera();

            EnableFlymode();

            ChangeMouseControls();

            RoomForcefield();

            if (!flyMode)
            {
                CameraMovement();
            }
            else
            {
                PlayerMovement();
            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            RightClickDelete();
#endif
        }

        public void FireBullet()
        {
            //The Bullet instantiation happens here.
            GameObject tempBulletHandler = Instantiate(bullet, bulletEmitter.transform.position, head.transform.localRotation) as GameObject;

            //Sometimes bullets may appear rotated incorrectly due to the way its pivot was set from the original modeling package.
            //This is EASILY corrected here, you might have to rotate it from a different axis and or angle based on your particular mesh.
            //Temporary_Bullet_Handler.transform.Rotate(Vector3.left * 90);

            //Retrieve the Rigidbody component from the instantiated Bullet and control it.
            Rigidbody tempTigidbody;
            tempTigidbody = tempBulletHandler.GetComponent<Rigidbody>();

            //Tell the bullet to be "pushed" forward by an amount set by Bullet_Forward_Force.
            tempTigidbody.AddForce(head.transform.forward * bulletSpeed);

            //Basic Clean Up, set the Bullets to self destruct after 10 Seconds, I am being VERY generous here, normally 3 seconds is plenty.
            Destroy(tempBulletHandler, 10.0f);
        }

        private void RoomForcefield()
        {
            Vector3 playerPos = player.transform.position;

            //float playerPosX = player.transform.position.x;
            //float playerPosZ = player.transform.position.z;
            //float playerPosY = player.transform.position.y;
            // check X
            if (playerPos.x < xMin)
            {
                //playerPosX = xMin + playerOffset;
                playerPos.x = xMin + playerOffset;
                print("blocked X");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
                
            } else if (playerPos.x > xMax)
            {
                //playerPosX = xMax - playerOffset;
                playerPos.x = xMax - playerOffset;
                print("blocked X");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
            } 
            // check Y
            else if (playerPos.y < yMin)
            {
                //playerPosY = yMin + playerOffset;
                playerPos.y = yMin + playerOffset;
                print("blocked Y. yMin");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
            } 
            else if (playerPos.y > yMax)
            {
                //playerPosY = yMax - playerOffset;
                playerPos.y = yMax - playerOffset;
                print("blocked Y. yMax");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
            } 
            // check Z
            else if (playerPos.z < zMin)
            {
                //playerPosZ = zMin + playerOffset;
                playerPos.z = zMin + playerOffset;
                print("blocked Z");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
            } 
            else if (playerPos.z > zMax)
            {
                //playerPosZ = zMax - playerOffset;
                playerPos.z = zMax - playerOffset;
                print("blocked X");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("Out of bounds"));
            }


            player.transform.position = playerPos;
            //player.transform.position.z = playerPosZ;
            //player.transform.position.y = playerPosY;
        }

        public void OrthographicMode()
        {
            if (!orthoMode)
            {
                cam.orthographic = true;
                //buildModeImage.color = new Color(0, 0, 0, 0.5f);
                UI_Manager.Instance.orthographicImage.color = new Color(0, 0, 0, 0.5f);
                orthoMode = true;
                UI_Manager.Instance.orthoButton.GetComponentInChildren<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            else
            {
                cam.orthographic = false;
                orthoMode = false;
                UI_Manager.Instance.orthographicImage.color = new Color(1, 0.89f, 0, 1f);
                UI_Manager.Instance.orthoButton.GetComponentInChildren<Text>().color = Color.black;
            }
        }

        private void JoystickMove()
        {
            direction = jsMovement.InputDirection; //InputDirection can be used as per the need of your project

            if (direction.magnitude != 0)
            {
                player.transform.Translate(Mathf.Clamp(direction.x * moveSpeed, xMin, xMax), 0, Mathf.Clamp(direction.y * moveSpeed, yMin, yMax));
            }
        }

        private void JoystickAim() 
        {
            direction = jsAim.InputDirection;
            if (direction.magnitude != 0)
            {
                player.transform.RotateAround(player.transform.position, player.transform.up, direction.x * aimTurnSpeed);
                head.transform.RotateAround(head.transform.position, head.transform.right, -direction.y * aimTurnSpeed);
            }
        }

        // change mouse controls
        private void ChangeMouseControls()
        {
            if (Input.GetKeyDown(KeyCode.F) && mouseControl)
            {
                mouseControl = false;
            }
            else
            {
                mouseControl = true;
            }
        }

        // create the settings for the player moving around
        private void SetupPlayer()
        {
            head = GameObject.Find("Head");
            player = GameObject.Find("Player");

            player.AddComponent<Rigidbody>();
            player.transform.rotation = new Quaternion(0, 0, 0, 0);
            head.transform.rotation = new Quaternion(0, 0, 0, 0);
            playerRb = GetComponent<Rigidbody>();
            playerRb.useGravity = false;
            playerRb.angularDrag = 0.0f;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
            playerRb.drag = 0;
        }

        private void EnableFlymode()
        {
            if (Input.GetKeyDown(KeyCode.C)) {
                SwitchToPlayer();
            }
        }

        // switch to and from walk and fly modes
        public void SwitchToPlayer()
        {
            if (!flyMode)
            {
                playerRb.useGravity = true;
                //buildModeImage.color = new Color(0, 0, 0, 0.5f);
                UI_Manager.Instance.flyModeImage.color = new Color(1, 0.72f, 0, 1f);
                print("Player mode");
                flyMode = true;
                //player.transform.rotation = new Quaternion(0, 0, 0, 0);
                playerRb.drag = 0;
                mouseControl = true;

                UI_Manager.Instance.flyButton.GetComponentInChildren<Text>().color = Color.black;
            }
            else
            {
                playerRb.useGravity = false;
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 2, transform.localPosition.z);
                UI_Manager.Instance.flyModeImage.color = new Color(0, 0, 0, 0.5f);
                flyMode = false;
                playerRb.drag = 100;
                mouseControl = false;

                UI_Manager.Instance.flyButton.GetComponentInChildren<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1);
            }

            UI_Manager.Instance.ChangeFireButton();
        }

        void ResetCamera()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCamButton();
            }
        }

        public void ResetCamButton()
        {
            player.transform.position = new Vector3(0, 5, 31);
            player.transform.rotation = new Quaternion(0, 0, 0, 0);
            head.transform.rotation = new Quaternion(0, 0, 0, 0);
        }

        // Moving the Camera
        private void PlayerMovement()
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(0, 0, moveForward);
            }
            // strafe left
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(-strafe, 0, 0);
            }
            // move back
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(0, 0, -moveForward);
            }
            // strafe right
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(strafe, 0, 0);
            }
            // crouch
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!crouch)
                {
                    GetComponent<CapsuleCollider>().height = 1f;
                    crouch = true;
                }
                else
                {
                    GetComponent<CapsuleCollider>().height = 2f;
                    crouch = false;
                }
            }
            // jump
            if (Input.GetKeyDown(KeyCode.E))
            {
                //transform.Translate(0, moveUp, 0);
                //print("Player jump");
                //transform.position = new Vector3(transform.position.x, transform.position.y * jumpHeight, transform.position.z);
                // todo create better jump movement
                // rb force
                playerRb.AddForce(transform.position.x, jumpHeight, transform.position.z, ForceMode.Impulse);
            }
        }

        // moving forward, back and strafing
        private void CameraMovement()
        {

            // move forward
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(0, 0, moveForward);
            }
            // strafe left
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(-strafe, 0, 0);
            }
            // move back
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(0, 0, -moveForward);
            }
            // strafe right
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(strafe, 0, 0);
            }
            // move down a level
            if (Input.GetKeyDown(KeyCode.Q))
            {
                MoveDown();
            }
            // move up a level
            if (Input.GetKeyDown(KeyCode.E))
            {
                MoveUp();
            }

        }

        public void MoveUp()
        {
            player.transform.Translate(0, Mathf.Clamp(moveUp, heightMin, heightMax), 0);
        }

        public void MoveDown()
        {
            player.transform.Translate(0, -moveUp, 0);
        }

        private void MouseFree()
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
            head.transform.RotateAround(transform.position, transform.right, -pos.y * playerTurnSpeed);
            player.transform.RotateAround(transform.position, Vector3.up, pos.x * playerTurnSpeed);
        }



        // checks for clicks or button held down
        private void CheckForMouseClicks()
        {
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                return;
            }
            else if (Input.GetMouseButton(0))
            {
                // count seconds from holding button
                // reset once spawned an object in draw mode
                Timer += Time.deltaTime;

                if (UI_Manager.Instance.drawMode)
                {
                    if (Timer > spawnTime) // slows down drawing
                    {
                        RaycastHitSpawn();
                        Timer = 0;
                    }
                }
            }

            // Spawn object if button not held down
            if (Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1) && !Input.GetMouseButtonUp(2)) // && !buttonHeld
            {
                if (canSpawn)
                {
                    RaycastHitSpawn();
                }
                else
                {
                    canSpawn = true;
                }

            }
        }

        // for desktop only
        private void RightClickDelete()
        {
            bool rightClickEnabled = false;
            if(playerRb.velocity.magnitude < 0.1f){
                rightClickEnabled = true;
            } else {
                rightClickEnabled = false;
            }


            if (Input.GetMouseButtonUp(1) && rightClickEnabled)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && !hit.transform.CompareTag("UI") && !hit.transform.CompareTag("Room"))
                    {
                        selectedGameObject = hit.collider.gameObject;
                        Destroy(selectedGameObject);

                        // undo working?
                        //selectedGameObject.GetComponent<SaveCommands>().AddToCommands();
                    }
                }
            }
        }

        // Get mouse position and spawn object
        private void RaycastHitSpawn()
        {
            if (selectedGameObject != null && !UI_Manager.Instance.selectMultipleMode)
                selectedGameObject.gameObject.GetComponent<Renderer>().material.mainTexture = defaultTexture;
            else
                print("No Selected GameObject");

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            // screen x,y space + dir

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    float hitPointDistance = hit.distance;
                    Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f, true);

                    // EDIT MODE
                    if (UI_Manager.Instance.editMode && !hit.transform.CompareTag("UI") && !hit.transform.CompareTag("Room"))
                    {
                        // set selected colour to game object

                        // select multiple objects and increase
                        if (UI_Manager.Instance.selectMultipleMode){
                            // add each selection to List

                            // check if texture already assigned, unassign it to unselect it
                            if (hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture == tex2d){
                                hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture = defaultTexture;

                                selectedGameObjects.Remove(hit.collider.gameObject);

                                objects--;
                            } else {
                                selectedGameObjects.Add(hit.transform.gameObject);
                                hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture = tex2d;

                                objects++;
                            }

                            if (objects > 1)
                                StartCoroutine(UI_Manager.Instance.ShowInfoText(objects + " Objects selected"));
                        } else {
                            // save gameobject colour
                            tempColor = hit.collider.gameObject.GetComponent<Renderer>().material.color;

                            // todo make it glow!
                            //hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                            hit.collider.gameObject.GetComponent<Renderer>().material.mainTexture = tex2d;

                            // selected object to move
                            selectedGameObject = hit.collider.gameObject;
                        }

                        //// hold and move
                        //if (Input.GetMouseButton(0))
                        //{
                        //    selectedGameObject.gameObject.transform.position = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                        //    print("object held. moving object");
                        //}

                        //if (Input.GetMouseButtonUp(0))
                        //{
                            //hit.collider.gameObject.GetComponent<SaveCommands>().AddToCommands();
                        //    print("send mouse up command in edit mode");
                        //}
                    } 

                    // DELETE mode is on, destroy prefab when clicked
                    if (UI_Manager.Instance.deleteMode)
                    {
                        if (hit.transform.CompareTag("UI") || hit.transform.CompareTag("Room"))
                        {
                            return;
                        }

                        //if (!PhotonNetwork.inRoom)
                        //{
                            //hit.collider.gameObject.GetComponent<SaveCommands>().AddToCommands();


                            selectedGameObject = hit.collider.gameObject;
                            Destroy(selectedGameObject);
                            //hit.transform.gameObject.SetActive(false);

                            //selectedGameObject.GetComponent<SaveCommands>().AddToCommands();

                        //} else
                            //PhotonNetwork.Destroy(hit.transform.gameObject); 

                        //print("Delete object");

                    }

                    // COLOUR mode on
                    if (UI_Manager.Instance.colourMode)
                    {
                        if (hit.transform.CompareTag("UI") || hit.transform.CompareTag("Room"))
                        {
                            return;
                        }

                        if (hit.transform.CompareTag("colourPalette"))
                        {
                            print("hit palette");
                            // Save colour from palette
                            tempColor = hit.collider.GetComponent<MeshRenderer>().material.color;

                            // assign to colourCube to show selected colour
                            UI_Manager.Instance.colourCube.GetComponent<MeshRenderer>().material.color = tempColor;

                            // removes highlights
                            RemoveHighlightColour();

                            // highlights colour
                            hit.collider.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            // Apply colour
                            //if (!PhotonNetwork.inRoom)
                            //{
                                hit.collider.GetComponent<MeshRenderer>().material.color = tempColor;
                                //selectedGameObject = hit.collider.gameObject;
                                //hit.collider.gameObject.GetComponent<SaveCommands>().AddToCommands();
                            //}
                            //else
                            //{
                                // save gameobjects mesh renderer
                                tempMeshRenderer = hit.collider.GetComponent<MeshRenderer>();
                                // todo
                                //pView.RPC("ApplyColour", PhotonTargets.AllBuffered, null);
                            //}
                        }
                    }

                    // HOLD button. Only build mode
                    //else if (heldTime > holdTimeForMenu && UI_Manager.Instance.buildMode && hit.transform.CompareTag("Layer01"))
                    //{
                        ////print("holding button");
                        ////if (hit.transform.CompareTag("Layer01") || hit.transform.CompareTag("Layer02")
                        ////    || hit.transform.CompareTag("Layer03") || hit.transform.CompareTag("Layer04") || hit.transform.CompareTag("Layer05"))
                        ////{
                        //    selectedGameObject = hit.collider.gameObject;
                        //    canSpawn = false;
                        //    // selects the object and highlights it
                        //    tempColor = hit.collider.gameObject.GetComponent<Renderer>().material.color;
                        //// selectedColor = 
                        //    hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                        //    // loads object translation menu
                        //    UI_Manager.Instance.ShowRadialMenu();
                        //    heldTime = 0;
                        //    return;
                        ////}

                    // BUILD MODE - SPAWN OBJECT
                    //}
                    if (hitPointDistance > minDistanceToSpawnObject
                               //&& heldTime < holdTimeForMenu
                               && UI_Manager.Instance.buildMode
                               && canSpawn)
                    {
                        // spawn single layer
                        if (orthoMode)
                        {
                            if (hit.collider.tag == "Room")
                            {
                                spawnPrefab.SpawnCube(hit);
                                //print("Drawing in ortho mode");

                                return;
                            }
                        }
                        else
                        {
                            // spawn if nothing stopping it
                            //print("Nothing stopping me spawn: " + canSpawn);
                            spawnPrefab.SpawnCube(hit);

                            //ApplyColour();
                            return;
                        }
                    }
                } // end of hit.collider
            }
        }

        private void HoldAndMove()
        {
            //// hold and move
            if (Input.GetMouseButton(0) && UI_Manager.Instance.editMode)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            // screen x,y space + dir

                // get position mouse clicks on screen
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);


                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        float hitPointDistance = hit.distance;
                        Debug.DrawLine(ray.origin, hit.point, Color.green, 2.0f, true);

                        //selectedGameObject.gameObject.transform.position = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                        //print("object held. moving object");

                        // EDIT MODE
                        if (!hit.transform.CompareTag("UI") && !hit.transform.CompareTag("Room"))
                        {
                            //print("edit mode. selected game object.");
                            // save gameobject colour
                            //tempColor = hit.collider.gameObject.GetComponent<Renderer>().material.color;
                            // set selected colour to game object

                            // todo make it glow!
                            //hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.yellow;

                            // selected object to move
                            //selectedGameObject = hit.collider.gameObject;

                            //Vector3 mouseMove = new Vector3();
                            //mouseMove = hit.transform.position;
                            //mouseMove.z = hit.transform.position.z;

                            //selectedGameObject.gameObject.transform.localPosition = Camera.main.ScreenToViewportPoint(mouseMove);
                            //print("object held. moving object");

                            /*
                            if (Input.GetMouseButton(0))
                            {
                                Difference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - hit.transform.position ;//Camera.main.transform.position;
                                if (Drag == false)
                                {
                                    Drag = true;
                                    Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                                }

                            }
                            else
                            {
                                Drag = false;
                            }

                            //
                            if (Drag == true)
                            {
                                //Camera.main.transform.position = Origin - Difference;
                                selectedGameObject.gameObject.transform.localPosition = Origin - Difference;
                            }
                            */


                            if (Input.GetMouseButtonUp(0))
                            {
                                //hit.collider.gameObject.GetComponent<SaveCommands>().AddToCommands();
                                print("send mouse up command in edit mode");
                            }
                        }


                    }
                }

            }
        }




        // TODO FIX FOR ONLINE
        //[PunRPC]
        //void ApplyColour()
        //{
        //    tempMeshRenderer.material.color = tempColor;
        //}

        // create colourPalette array of highlighted boxes
        private void RemoveHighlightColour()
        {
            
            for (int i = 0; i < UI_Manager.Instance.colourPaletteHighlight.Length; i++){
                //colourPalette[i].SetActive(false);
                UI_Manager.Instance.colourPaletteHighlight[i].SetActive(false);
            }
        }

        // legacy movement of mouse for turning and panning
        private void MouseMovement()
        {
            // Get the right mouse button
            if (Input.GetMouseButtonDown(1))
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;
                isRotating = true;
            }

            // Get the middle mouse button
            if (Input.GetMouseButtonDown(2))
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;
                isPanning = true;
            }

            // Todo No 3rd button. Use 2 buttons together?
            if (Input.GetMouseButtonDown(3))
            {
                // Get mouse origin
                mouseOrigin = Input.mousePosition;
                isZooming = true;
            }

            // Disable movements on button release
            if (!Input.GetMouseButton(1)) isRotating = false;
            if (!Input.GetMouseButton(2)) isPanning = false;

            // Rotate camera along X and Y axis
            if (isRotating)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                head.transform.RotateAround(head.transform.position, head.transform.right, -pos.y * turnSpeed);
                //head.transform.RotateAround(head.transform.position, Vector3.up, pos.x * turnSpeed);
                player.transform.RotateAround(player.transform.position, Vector3.up, pos.x * turnSpeed);
            }

            // Move the camera on it's XY plane
            if (isPanning)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                //Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
                Vector3 move = new Vector3(pos.x * panSpeed, 0, pos.y * panSpeed);
                transform.Translate(move, Space.Self);
            }

            // Move the camera linearly along Z axis
            if (isZooming)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

                Vector3 move = pos.y * zoomSpeed * transform.forward;
                transform.Translate(move, Space.World);
            }
        }

        // todo
        // only spawn if not UI

        // Use mouse to click and spawn objects
        private void OnMouseClick()
        {
            if (Input.GetMouseButtonUp(0))
            {
                //heldTime = 0;
                //Camera cam = Camera.main;
                RaycastHit hit;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            // screen x,y space + dir

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        float hitPointDistance = hit.distance;

                        Debug.DrawLine(ray.origin, hit.point, Color.red, 3.0f, true);

                        if (hitPointDistance > spawnDistance)
                        {
                            if (orthoMode)
                            {
                                if (hit.collider.tag == "room")
                                {
                                    spawnPrefab.SpawnCube(hit);
                                    print("spawn on wall");
                                }
                                else return;
                            }
                            else
                            {
                                spawnPrefab.SpawnCube(hit);
                                print("Spawn cube");
                            }

                        }
                        //print("HIT! at: " + hitPointDistance);
                    }
                }
            }

        }

    }
}