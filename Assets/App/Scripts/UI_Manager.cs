using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Assets.App.Scripts
{
    public class UI_Manager : MonoBehaviour
    {
        public static UI_Manager Instance { get; set; }

        #region Private Variables

        private int menuButtonsYpos = 290; //275
        private int bottomButtons = (Screen.height - Screen.height);
        private int colourPalettePos = 240;
        private Color buttonColor = Color.gray;
        private GUIStyle myStyle = new GUIStyle();
        private RectTransform tempRT;
        private Font font;
        private Text tempText;
        private String tempString;
        private Image tempImage;
        private Button tempButton;
        private MeshRenderer tempMeshRend;
        private Image buildModeImage, colourModeImage, deleteModeImage, editModeImage;
        
        private Color highlightedColor;
        private Color normalColor;
        private Color invisibleColor;

        [SerializeField] private GameObject colourRed;
        [SerializeField] private GameObject colourBlue;
        [SerializeField] private GameObject colourGreen;
        [SerializeField] private GameObject colourWhite;
        [SerializeField] private GameObject colourBlack;
        [SerializeField] private GameObject colourOrange;
        [SerializeField] private GameObject colourYellow;
        [SerializeField] private GameObject colourBrown;
        [SerializeField] private GameObject colourPurple;
        [SerializeField] private GameObject colourPink;

        private float q;
        private float r;

        #endregion

        #region Public Variables

        [Header("Scripts")]
        public SpawnPrefab spawnPrefab;

        [Header("Drawing Buttons")]
        public GameObject singleModeGO;
        public GameObject drawModeGO;

        [Header("Images")]
        public Image flyModeImage;
        public Image orthographicImage;
        public Image singleDrawImage;
        public Image drawModeImage;

        [Header("Screens")]
        public GameObject optionsScreen;
        public GameObject menuScreen;
        public GameObject onlineScreen;
        public GameObject radialMenu;
        public GameObject helpMenu;
        public GameObject optionsControls;
        public GameObject optionsThemes;
        public GameObject nextPage;
        public GameObject saveMenu;
        public GameObject loadMenu;
        public GameObject thankYouScreen;

        [Header("Buttons")]
        public GameObject buildModeButton;
        public GameObject colourModeButton;
        public GameObject deleteModeButton;
        public GameObject editModeButton;
        public GameObject orthoButton;
        public GameObject flyButton;

        [Header("Other")]
        public GameObject colourPalette;
        public GameObject shopMenu;
        public GameObject colourCube;

        public GameObject touchControlsGo;
        public GameObject debugConsoleGo;
        public GameObject sunPosition;

        [Header("Bools")]
        public bool buildMode;
        public bool colourMode;
        public bool deleteMode;
        public bool drawMode;
        public bool singleMode;
        public bool editMode;
        public bool enableRadialMenu, enableMenuScreen;
        private bool options, menu, controls, themes;
        private bool showUiInScreens;
        public bool selectMultipleMode;
        private bool touchControls;
        private bool debugConsole;

        [Header("Sliders")]
        public Slider playerMovement;
        public Slider playerTurning;
        public Slider snapAmount;
        public Slider drawSpawns;
        public Slider sunPositionSlider;

        [Header("Text")]
        public Text playerMovementAmount;
        public Text playerTurningAmount;
        public Text snappingAmount;
        public Text snappingAmount2;
        public Text drawSpawnsAmount;
        public Text infoText;
        public Text gameVersion;
        public Text proOrFree;
        public Text sunPositionAngle;

        public float snap;

        [Header("Layers")]
        public int selectedLayer = 0;
        //public bool layer1, layer2, layer3, layer4, layer5 = true;
        //public GameObject layer01, layer02, layer03, layer04, layer05, layerGroup, layersGO;

        public GameObject roomTheme0, roomTheme1, roomTheme2;
        public GameObject dropdownList;
        public GameObject[] prefabs;
        public int prefabObject; // 1.cube, 2.platform, 3. pillar, 4. wedge, 5.step2stair

        private bool enableDropdownList;

        public SaveLoadManager saveLoadManager;

        [Header("Online Messages")]
        public GameObject ChatScreen;

        [Header("Selection Highlights")]
        public GameObject[] colourPaletteHighlight;
        public GameObject[] dropdownButtonHighlight;

        [Header("Soldier Mode")]
        public GameObject FireButton;
        public GameObject ResetCamButton;

        GameObject selectedObject;
        public GameObject shareButton;

        [Header("IAP")]
        public GameObject IapLockButton;
        public GameObject[] IapLockButtons;

        [HideInInspector]
        public bool proUnlocked;

        [Header("Canvases")]
        public GameObject canvasTop;
        public GameObject canvasBottom;



        public Material defaultMat;
        public Texture2D defaultTexture;

        #endregion

        void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            gameVersion.text = "v" + Application.version;

            buildMode = true;
            drawMode = true;

            highlightedColor = new Color(0.7f, 0.7f, 0.7f, 1);
            normalColor = new Color(1, 1, 1, 0.5f);
            invisibleColor = new Color(1, 1, 1, 0);

            buildModeImage = buildModeButton.GetComponent<Image>();
            colourModeImage = colourModeButton.GetComponent<Image>();
            deleteModeImage = deleteModeButton.GetComponent<Image>();
            editModeImage = editModeButton.GetComponent<Image>();

            if (colourRed == null) colourRed = GameObject.Find("RedBlock");
            if (colourBlue == null) colourBlue = GameObject.Find("BlueBlock");
            if (colourGreen == null) colourGreen = GameObject.Find("GreenBlock");
            if (colourWhite == null) colourWhite = GameObject.Find("WhiteBlock");
            if (colourBlack == null) colourBlack = GameObject.Find("BlackBlock");
            if (colourOrange == null) colourOrange = GameObject.Find("OrangeBlock");
            if (colourYellow == null) colourYellow = GameObject.Find("YellowBlock");
            if (colourBrown == null) colourBrown = GameObject.Find("BrownBlock");
            if (colourPurple == null) colourPurple = GameObject.Find("PurpleBlock");
            if (colourPink == null) colourPink = GameObject.Find("PinkBlock");

            DisplayColourPalette(false);
            SwitchBuildMode();
            helpMenu.SetActive(false);
            EnableDrawMode();

            if (prefabs.Length > 0)
                spawnPrefab.selectedObject = prefabs[0];

            HighlightDropdownSelection();
            if (dropdownButtonHighlight.Length > 0)
                dropdownButtonHighlight[0].SetActive(true);

            SetupPrefabsList();

            LoadOptions();

            UnlockPro();
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            //ABigThankYou();
#endif

//#if !UNITY_ANDROID || UNITY_EDITOR
//            //shareButton.SetActive(false);
//            UnlockPro();
//#endif

//#if UNITY_ANDROID || UNITY_IOS
//            UnlockPro();
//#endif

            //IapLockButtons = GameObject.FindGameObjectsWithTag("IapLock");

            StartCoroutine(ShowInfoText("Editor Loaded"));

        }

        void SaveOptions()
        {
            // save UI screenshot pref
            if (showUiInScreens)
            {
                PlayerPrefs.SetInt("UIon", 1);
            } else {
                PlayerPrefs.SetInt("UIon", 0);
            }

            // save player speed
            PlayerPrefs.SetFloat("playerMovement", playerMovement.value);

            // save player look
            PlayerPrefs.SetFloat("playerTurning", playerTurning.value);

            // save snap amount
            PlayerPrefs.SetFloat("snap", snap);

            //
            PlayerPrefs.SetFloat("drawSpawns", CameraManager.Instance.spawnTime);

            //
            print("Options Saved. " + "ShowUiInScreens: " + showUiInScreens + ", playerTurning: " + playerTurning.value + ", playerMovement" + playerMovement.value + ", snap: " + snap);

        }

        void LoadOptions()
        {
            if (PlayerPrefs.HasKey("UIon")){
                int n = PlayerPrefs.GetInt("UIon");
                if (n == 1)
                {
                    EnableUIForScreens(true);
                } else 
                {
                    EnableUIForScreens(false);
                }
            } else {
                EnableUIForScreens(true);
            }


            if (PlayerPrefs.HasKey("playerTurning"))
            {
                q = PlayerPrefs.GetFloat("playerTurning");
                SliderPlayerTurning(q);
			}

            if (PlayerPrefs.HasKey("playerMovement"))
            {
                r = PlayerPrefs.GetFloat("playerMovement");
                SliderPlayerMovement(r);
			}

            if (PlayerPrefs.HasKey("snap"))
            {
                snap = PlayerPrefs.GetFloat("snap");
                SliderSnapObjects(snap);
            }

            if (PlayerPrefs.HasKey("drawSpawns"))
            {
                float s = PlayerPrefs.GetFloat("drawSpawns");
                drawSpawns.value = s;
                drawSpawnsAmount.text = Math.Round(s, 2) + "";
                CameraManager.Instance.spawnTime = drawSpawns.value;
            }

            // use for offline use and network issues
            try {
                //SaveGame.Load("ProUnlocked", proUnlocked);
                if (proUnlocked)
                {
                    proOrFree.text = "Pro version";
                    CheckForUnlocks();
                }
            } catch (Exception e) {
                StartCoroutine(ShowInfoText("Free Mode"));
                Debug.Log("error: " + e);
                proOrFree.text = "Free version";
            }

            print("Options loaded. " + "ShowUiInScreens: " + showUiInScreens + ", playerTurning: " + q + ", playerMovement" + r);

            SliderSunPosition(132);
        }

        public void SliderSunPosition(float n)
        {
            sunPositionSlider.value = n;
            sunPositionAngle.text = n.ToString();

            Transform tran = sunPosition.gameObject.transform;

            Vector3 myAxis = Vector3.up;

            Quaternion rot = Quaternion.AngleAxis(n, myAxis);

            tran.transform.localRotation = rot;

        }

        public void EnableUIForScreens(bool on)
        {
            showUiInScreens = on;    
            print("showUiInScreens: " + showUiInScreens);
        }

        public void EnableTouchControls(bool on)
        {
            touchControls = on;
            touchControlsGo.SetActive(on);
        }

        // enable/disable the in game debug console
        public void EnableDebugconsole(bool on)
        {
            //debugConsole = on;
            //debugConsoleGo.SetActive(on);
        }

        // SWITCH OFF UI FOR SCREENSHOTS
        public void SwitchOffUI()
        {
            
            if (showUiInScreens)
            {
                print("keep UI on: " + showUiInScreens);
                canvasTop.SetActive(true);
                canvasBottom.SetActive(true);

            }
            else
            {
                print("Switch UI off: " + showUiInScreens);
                canvasTop.SetActive(false);
                canvasBottom.SetActive(false);
            }
        }

        public void SwitchOnUI()
        {
            canvasTop.SetActive(true);
            canvasBottom.SetActive(true);
        }

        public void ShowShopMenu()
        {
            if (shopMenu.activeSelf)
            {
                shopMenu.SetActive(false);
            }
            else
            {
                loadMenu.SetActive(false);
                saveMenu.SetActive(false);
                shopMenu.SetActive(true);
                Show_MenuScreen();
                menuScreen.SetActive(false);
            }
        }


        // 
        public void RestorePurchase()
        {
            StartCoroutine(ShowInfoText("Purchase Restored\nPro Unlocked"));
        }

        public void FailedPurchase()
        {
            proUnlocked = false;
            StartCoroutine(ShowInfoText("Purchase failed\nTry again?"));
        }

        // PRO IAP
        public void BuyPro()
        {
            //Purchaser.Instance.BuyNonConsumable_PRO();
        }

        // UNLOCK PRO EXTRAS
        public void UnlockPro()
        {
            proUnlocked = true;
            proOrFree.text = "Pro version";

            //SaveGame.Save("ProUnlocked", proUnlocked);

            CheckForUnlocks();
            StartCoroutine(ShowInfoText("Pro Unlocked\nThank you"));
#if UNITY_ANDROID
            //ABigThankYou();
#endif
        }

        // SHOW THANK YOU SCREEN
        public void ABigThankYou()
        {
            if (thankYouScreen.activeSelf){
                thankYouScreen.SetActive(false);
            } else {
                thankYouScreen.SetActive(true);
            }
        }

        // REMOVE LOCKS
        void CheckForUnlocks()
        {
            if (proUnlocked)
            {
                // remove locks
                //IapLockButton.SetActive(false);

                for (int i = 0; i < IapLockButtons.Length; i++)
                {
                    //IapLockButtons[i].GetComponentInChildren<Button>().interactable = true;
                    IapLockButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void SetupPrefabsList()
        {
            if (prefabs.Length > 0)
            {
                if (prefabs[0])
                {
                    prefabObject = 1; // cube
                } else if (prefabs[1])
                {
                    prefabObject = 2; // platform
                } else if (prefabs[2])
                {
                    prefabObject = 3; // pillar
                } else if (prefabs[3])
                {
                    prefabObject = 4; // wedge
                } else if (prefabs[4])
                {
                    prefabObject = 5; // step2stairs
                } else if (prefabs[5])
                {
                    prefabObject = 6;
                }
            }
            else
            {
                print("PREFABS NOT SET UP!");
            }

        }

        public void ChangeFireButton()
        {
            if (ResetCamButton.activeSelf)
            {
                ResetCamButton.SetActive(false);
                FireButton.SetActive(true);
            }
            else //if (ResetCamButton.activeSelf) 
            {
                ResetCamButton.SetActive(true);
                FireButton.SetActive(false);
            }
        }

        private void PUNsettings()
        {
            //PhotonNetwork.autoCleanUpPlayerObjects = false;
        }

        public void ConnectToPUN(bool on)
        {
            //ConnectAndJoinRandom.AutoConnect = on;
            tempString = "Joined room.";
            StartCoroutine(ShowInfoText(tempString));
        }

        public IEnumerator ShowInfoText(String temp)
        {
            infoText.text = temp;
            yield return new WaitForSeconds(3);
            infoText.text = "";
        }

        public void ExitApp()
        {
            Application.Quit();
        }

        //
        public void EnableDrawMode()
        {
            CameraManager.Instance.canSpawn = true;
            if (drawMode)
            {
                //singleMode = true;
                drawMode = false;
                drawModeImage.color = normalColor;
                singleDrawImage.color = new Color(1, 0, 0, 1.0f);

                drawModeGO.transform.localPosition = new Vector3(drawModeGO.transform.localPosition.x, -320, 0); // reset
                singleModeGO.transform.localPosition = new Vector3(singleModeGO.transform.localPosition.x, -310, 0);

                StartCoroutine(ShowInfoText("Single Draw Mode"));
            }
            else
            {
                drawMode = true;
                //singleMode = false;
                singleDrawImage.color = normalColor;
                drawModeImage.color = new Color(1, 0, 0, 1.0f);

                drawModeGO.transform.localPosition = new Vector3(drawModeGO.transform.localPosition.x, -300, 0);
                singleModeGO.transform.localPosition = new Vector3(singleModeGO.transform.localPosition.x, -320, 0); // reset

                StartCoroutine(ShowInfoText("Drawing Mode"));
            }
        }

        // set from dropdown
        public void SelectPrefab(int n)
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (n == i)
                {
                    // spawnPrefab selectedObject
                    spawnPrefab.selectedObject = prefabs[i];
                    // switch all highlights off
                    HighlightDropdownSelection();
                    // switch selected highlight on
                    dropdownButtonHighlight[i].SetActive(true);

                    StartCoroutine(CanNowSpawn());
                }
            }
        }

        // enable dropdown
        public void DropdownList()
        {
            if (enableDropdownList)
            {
                dropdownList.SetActive(false);
                enableDropdownList = false;
            }
            else
            {
                dropdownList.SetActive(true);
                enableDropdownList = true;
            }
        }

        // SHOP MENU
        public void ShowShopScreen()
        {

        }

        // SAVE MENU
        public void ShowSaveMenu()
        {
            CantSpawn();

            if (saveMenu.activeSelf)
            {
                saveMenu.SetActive(false);
            } else
                saveMenu.SetActive(true);

            StartCoroutine(CanNowSpawn());
        }

        // LOAD MENU
        public void ShowLoadMenu()
        {
            CantSpawn();

            if (loadMenu.activeSelf)
            {
                loadMenu.SetActive(false);
            } else
                loadMenu.SetActive(true);

            StartCoroutine(CanNowSpawn());
            print("loadMenu.activeSelf: " + loadMenu.activeSelf);
        }

        // CONTROLS MENU
        public void ShowOptionsControls()
        {
            if (!controls)
            {
                optionsControls.SetActive(true);
                ShowOptionsMenu();
                controls = true;
            }
            else
            {
                SaveOptions();
                optionsControls.SetActive(false);
                controls = false;
            }

            StartCoroutine(CanNowSpawn());
        }

        // OPTIONS MENU
        public void ShowOptionsThemes()
        {
            if (!themes)
            {
                optionsThemes.SetActive(true);
                ShowOptionsMenu();
                themes = true;
            }
            else
            {
                optionsThemes.SetActive(false);
                themes = false;
            }
            print("Themes menu: " + themes);
            StartCoroutine(CanNowSpawn());
        }

        // MENU PAGE 2
        public void ShowNextPage()
        {
            if (!nextPage.activeSelf)
            {
                nextPage.SetActive(true);
            }
            else
                nextPage.SetActive(false);

            StartCoroutine(CanNowSpawn());
        }

        // ONLINE MENU
        public void ShowOnlineMenu()
        {
            if (!onlineScreen.activeSelf)
            {
                onlineScreen.SetActive(true);
            }
            else
                onlineScreen.SetActive(false);

            StartCoroutine(CanNowSpawn());
        }

        // OPTIONS MENU
        public void ShowOptionsMenu()
        {
            if (!options)
            {
                optionsScreen.SetActive(true);
                menuScreen.SetActive(false);
                options = true;

            }
            else
            {
                optionsScreen.SetActive(false);
                menuScreen.SetActive(true);
                options = false;
                SaveOptions();
            }
            //print("Options menu: " + options);
            StartCoroutine(CanNowSpawn());
        }

        // CHANGE ROOM THEMES
        public void EnableRoomTheme0()
        {
            DisableThemes();

            roomTheme0.SetActive(true);
            StartCoroutine(CanNowSpawn());
        }

        public void EnableRoomTheme1()
        {
            DisableThemes();
            roomTheme1.SetActive(true);
            StartCoroutine(CanNowSpawn());
        }

        public void EnableRoomTheme2()
        {
            DisableThemes();
            roomTheme2.SetActive(true);
            StartCoroutine(CanNowSpawn());
        }

        void DisableThemes()
        {
            roomTheme0.SetActive(false);
            roomTheme1.SetActive(false);
            roomTheme2.SetActive(false);
        }

        // CHANGE USER MOVEMENT SPEED
        public void SliderPlayerMovement(float n)
        {
            try{
                playerMovement.value = n;
                playerMovementAmount.text = Math.Round(n, 2) + "";
                CameraManager.Instance.moveSpeed = playerMovement.value;
            } catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        // CHANGE USER TURNING SPEED
        public void SliderPlayerTurning(float n)
        {
            try{
                playerTurning.value = n;
                playerTurningAmount.text = Math.Round(n, 2) + "";

            CameraManager.Instance.aimTurnSpeed = playerTurning.value;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }

        // SNAP OBJECTS TO DIFFERENT GRID POSITIONS
        // 0.1, 0.2, 0.25, 0.5, 1
        public void SliderSnapObjects(float n)
        {

            if (n > 0 && n < 0.15) n = 0.1f;
            else if (n > 0.14 && n < 0.23) n = 0.2f;
            else if (n > 0.22 && n < 0.38) n = 0.25f;
            else if (n > 0.37 && n < 0.75) n = 0.5f;
            else if (n > 0.74 && n < 1) n = 1f;

            snappingAmount2.text = snappingAmount.text = n.ToString();
            snapAmount.value = n;
            snap = n;
        }

        // CHANGE DRAWING SPEED AMOUNT
        public void SliderSpawnTimer(float n)
        {
            try{
                drawSpawns.value = n;
                drawSpawnsAmount.text = Math.Round(n, 2) + "";
                CameraManager.Instance.spawnTime = drawSpawns.value;
            } catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        // radial menu
        public void ShowRadialMenu()
        {
            if (!radialMenu.activeSelf)
            {
                radialMenu.SetActive(true);
                //enableRadialMenu = true;
                CantSpawn();
            }
            else
            {
                CantSpawn();
                radialMenu.SetActive(false);
                //enableRadialMenu = false;
                StartCoroutine(CanNowSpawn());
            }

        }

        // allow to spawn after radial menu is closed
        public IEnumerator CanNowSpawn()
        {
            float seconds = 0.12f;
            yield return new WaitForSeconds(seconds);
            CameraManager.Instance.canSpawn = true;
        }

        // stops from spawning
        public void CantSpawn()
        {
            CameraManager.Instance.canSpawn = false;
        }

        // select multiple objects to move
        public void SelectMultipleObjects()
        {
            selectMultipleMode = !selectMultipleMode;
            StartCoroutine(ShowInfoText("Multiple selection\nmode: " + selectMultipleMode));

            if (!selectMultipleMode)
            {
                for (int i = 0; i < CameraManager.Instance.selectedGameObjects.Count; i++)
                {
                    // remove all textures from selected objects to show unselected
                    CameraManager.Instance.selectedGameObjects[i].gameObject.GetComponent<Renderer>().material.mainTexture = defaultTexture;
                }
            }
        }

        // select multiple objects to move
        public void SelectMultipleObjects(bool on)
        {
            selectMultipleMode = on;
            StartCoroutine(ShowInfoText("Multiple selection\nmode: " + selectMultipleMode));

            if (!selectMultipleMode && CameraManager.Instance.selectedGameObjects.Count > 0)
            {
                for (int i = 0; i <= CameraManager.Instance.selectedGameObjects.Count; i++)
                {
                    // remove all textures from selected objects to show unselected
                    CameraManager.Instance.selectedGameObjects[i].gameObject.GetComponent<Renderer>().material.mainTexture = defaultTexture;
                }
                CameraManager.Instance.selectedGameObjects.Clear();
                CameraManager.Instance.objects = 0;
            }
        }

        // copy selected object
        public void CopySelectedObject()
        {

            if (selectMultipleMode)
            {
                for (int i = 0; i < CameraManager.Instance.selectedGameObjects.Count; i++)
                {
                    Vector3 n = CameraManager.Instance.selectedGameObjects[i].transform.position;
                   
                    Quaternion quat = CameraManager.Instance.selectedGameObjects[i].transform.rotation;
                    n.y += 2;

                    Instantiate(CameraManager.Instance.selectedGameObjects[i], n, quat);
                }
            } else {
                Vector3 n = CameraManager.Instance.selectedGameObject.transform.position;
                Quaternion quat = CameraManager.Instance.selectedGameObject.transform.rotation;
                n.y += 2;

                Instantiate(CameraManager.Instance.selectedGameObject, n, quat);
			}
        }

        // move selected objects
        void MoveSelectedObjects(Vector3 n)
        {
            // put this code into edit / move ??
            if (selectMultipleMode)
            {
                // todo
                // move back to camera. create a function there rather than passing it back.
                // i'm unable to get into the for or foreach function

                for (int i = 0; i < CameraManager.Instance.selectedGameObjects.Count; i ++)
                {
                    CameraManager.Instance.selectedGameObjects[i].transform.Translate(n);
                }
            } else {
                if (CameraManager.Instance.selectedGameObject != null)
                {
                    selectedObject = CameraManager.Instance.selectedGameObject;
                    CameraManager.Instance.selectedGameObject.transform.Translate(n);
                }
                else
                    StartCoroutine(ShowInfoText("Select an object"));
			}
        }

        // translation tools for side menu
        public void TranslateObjectLeft()
        {
            Vector3 move = new Vector3(transform.position.x - snap, 0, 0);
            MoveSelectedObjects(move);
        }

        public void TranslateObjectRight()
        {
            Vector3 move = new Vector3(transform.position.x + snap, 0, 0);
            MoveSelectedObjects(move);
        }

        public void TranslateObjectUp()
        {
            Vector3 move = new Vector3(0, transform.position.x + snap, 0);
            MoveSelectedObjects(move);
  
        }

        public void TranslateObjectDown()
        {
            Vector3 move = new Vector3(0, transform.position.x - snap, 0);
            MoveSelectedObjects(move);
        }

        public void TranslateObjectForward()
        {
            Vector3 move = new Vector3(0, 0, transform.position.x + snap);
            MoveSelectedObjects(move);
        }

        public void TranslateObjectback()
        {
            Vector3 move = new Vector3(0, 0, transform.position.x - snap);
            MoveSelectedObjects(move);
        }

        public void RotateObjectAroundX()
        {

            if (CameraManager.Instance.selectedGameObject != null)
            {
                GameObject tempGO = CameraManager.Instance.selectedGameObject;
                tempGO.transform.RotateAround(tempGO.transform.position, tempGO.transform.forward, 90);
            }
            else
                StartCoroutine(ShowInfoText("Select an object"));
        }

        public void RotateObjectAroundY()
        {

            if (CameraManager.Instance.selectedGameObject != null)
            {
                GameObject tempGO = CameraManager.Instance.selectedGameObject;
                tempGO.transform.RotateAround(tempGO.transform.position, tempGO.transform.up, 90);
            }
            else
                StartCoroutine(ShowInfoText("Select an object"));
        }

        public void RotateObjectAroundZ()
        {
            if (CameraManager.Instance.selectedGameObject != null)
            {
                GameObject tempGO = CameraManager.Instance.selectedGameObject;
                tempGO.transform.RotateAround(tempGO.transform.position, tempGO.transform.right, 90);
            }
            else
                StartCoroutine(ShowInfoText("Select an object"));
        }

      
        // MAIN MENU
        public void Show_MenuScreen()
        {
            CameraManager.Instance.canSpawn = false;

            if (!menu)
            {
                menuScreen.SetActive(true);
                menu = true;
            }
            else
            {
                menuScreen.SetActive(false);
                menu = false;
            }

            StartCoroutine(CanNowSpawn());
        }

        // HELP POPUP
        public void ShowHelpMenu()
        {
            CantSpawn();

            if (helpMenu.activeSelf)
            {
                helpMenu.SetActive(false);
                StartCoroutine(CanNowSpawn());
            } else{
                helpMenu.SetActive(true);
            }
        }

        void ChangeMouseMovement()
        {
            print("Changing Mouse Movement...");
        }

        // ENABLE COLOUR PALETTE
        void DisplayColourPalette(bool on)
        {
            colourPalette.SetActive(on);
        }

        // EDIT MODE
        public void SwitchEditMode()
        {
            DisableModes();
            editModeImage.color = new Color(0, 0, 0, 0.5f);

            editMode = true;

            ShowRadialMenu();

            editModeButton.GetComponentInChildren<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1);
        }

        // BUILD MODE
        public void SwitchBuildMode()
        {
            DisableModes();
            buildModeImage.color = new Color(0, 0, 0, 0.5f);
            buildMode = true;

            // change text colour to white
            buildModeButton.GetComponentInChildren<Text>().color = new Color(0.7f,0.7f,0.7f,1);
        }

        // COLOUR MODE
        public void SwitchColourMode()
        {
            DisableModes();
            colourModeImage.color = new Color(0, 0, 0, 0.5f);
            DisplayColourPalette(true);
            colourMode = true;

            colourModeButton.GetComponentInChildren<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1);
        }

        // DELETE MODE
        public void SwitchDeleteMode()
        {
            DisableModes();
            deleteModeImage.color = new Color(0, 0, 0, 0.5f);

            deleteMode = true;

            deleteModeButton.GetComponentInChildren<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1);
        }

        //
        void DisableModes()
        {
            buildModeImage.color = new Color(0, 0.67f, 1); // blue
            colourModeImage.color = new Color(0, 0.78f, 0.07f); // green
            deleteModeImage.color = new Color(1, 0.28f, 0.13f); // red
            editModeImage.color = new Color(1, 0.12f, 0.87f); // pink. 1. 32, 221

            buildMode = false;
            colourMode = false;
            deleteMode = false;
            editMode = false;

            buildModeButton.GetComponentInChildren<Text>().color = Color.black;
            editModeButton.GetComponentInChildren<Text>().color = Color.black;
            colourModeButton.GetComponentInChildren<Text>().color = Color.black;
            deleteModeButton.GetComponentInChildren<Text>().color = Color.black;

            DisplayColourPalette(false);

            radialMenu.SetActive(false);
        }

        // create dropdown array of highlighted boxes
        void HighlightDropdownSelection()
        {
            for (int i = 0; i < dropdownButtonHighlight.Length; i++)
            {
                dropdownButtonHighlight[i].SetActive(false);
            }
        }

    }
}