using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;

namespace Assets.App.Scripts
{
    public class SaveLoadManager : MonoBehaviour
    {
        #region Public Variables

        [TextArea]
        public string InfoAboutComponent;

        public static SaveLoadManager Instance { get; set; }
        
        [Header("Scripts")]
        public UI_Manager ui_Manager;
        private ObjExporter objExporter;
        
        [Header("")]
        public GameObject[] objectsArray;
        public Vector3[] positionsArray;

        public Transform[] transformArray;
        public Color[] colorArray;
        public int[] objectTypeArray;

        public int maxObjects;
        public GameObject container;
        public GameObject[] Layers;

        [Header("Different Object Prefabs")]
        public GameObject[] objectType0;
        public GameObject[] objectType1;
        public GameObject[] objectType2;

        [Header("Images for Saving/Loading screen")]
        public List<SpriteRenderer> saveImages = new List<SpriteRenderer>();
        public List<SpriteRenderer> loadImages = new List<SpriteRenderer>();

        public GameObject canvasCenter;

        [HideInInspector]
        public string tempPath;

        #endregion

        #region Private Variables

        private GameObject[] objectType3;
        private GameObject[] objectType4;
        private GameObject[] objectType5;
        private GameObject spawnedGo;

        private string saveFileName = "/saveFile_";
        private string dataAsJson;

        private SceneData sceneData;
        private SceneData loadData;

        private List<int> prefabType;
        private List<Vector3> positions;
        private List<Quaternion> rotation;
        private List<Color> objColour;
        
        #endregion

        void Awake()
        {
            Instance = this;

            if (ui_Manager == null)
                ui_Manager = GameObject.Find("UI_Manager").GetComponent<UI_Manager>();

        }

        // Use this for initialization
        void Start()
        {
            LoadImages();
        }

        // Grab dir of names
        public void LoadImages()
        {
            var directoryInfo = new DirectoryInfo(Application.persistentDataPath); // Application.streamingAssetsPath
            
            FileInfo[] allFiles = directoryInfo.GetFiles("*.png");

            foreach (FileInfo file in allFiles)
            {
                if (file.Name.Contains("Screenshot"))
                    StartCoroutine(LoadScreenshot(file));
            }
        }

        // load screenshots into save and load screens
        private IEnumerator LoadScreenshot(FileInfo screenshot)
        {
            if (screenshot.Name.Contains("meta"))
            {
                yield break;
            }
            else
            {
                string wwwPlayerFilePath = "file://" + screenshot.FullName;

                WWW www = new WWW(wwwPlayerFilePath);
                yield return www;

                string[] digits = Regex.Split(wwwPlayerFilePath, @"\D+");

                foreach (string value in digits)
                {
                    int number;
                    if (int.TryParse(value, out number))
                    {
                        var sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));

                        number--;
                        loadImages[number].sprite = saveImages[number].sprite = sprite;
                    }
                }

            }
        }

        // find cube, platform, pillar, wedge, step2Stair
        private void SetObjectType()
        {
            objectType0 = GameObject.FindGameObjectsWithTag("Cube");
            objectType1 = GameObject.FindGameObjectsWithTag("Pillar");
            objectType2 = GameObject.FindGameObjectsWithTag("Platform");
            objectType3 = GameObject.FindGameObjectsWithTag("Wedge");
            objectType4 = GameObject.FindGameObjectsWithTag("Step2Stair");

            print("objectType0 length: " + objectType0.Length);
            print("objectType1 length: " + objectType1.Length);
            print("objectType2 length: " + objectType2.Length);
            print("objectType3 length: " + objectType3.Length);
            print("objectType4 length: " + objectType4.Length);

            FindAllObjects();
        }

        // find all cube objects on screen
        private void FindAllObjects()
        {
            //goNumber = new List<int>();
            prefabType = new List<int>();
            positions = new List<Vector3>();
            rotation = new List<Quaternion>();
            objColour = new List<Color>();

            // cycle through cubes
            for (int i = 0; i < objectType0.Length; i++)
            {
                // if string = cube. prefab = 0
                if (objectType0[i].name.Contains("Cube"))
                {
                    // set object type - 0.cube, 1.pillar, 2.platform, 3.wedge, 4.step2stair
                    prefabType.Add(0);
                    // Add all positions
                    positions.Add(objectType0[i].gameObject.transform.position);

                    // add all rotations
                    rotation.Add(objectType0[i].gameObject.transform.rotation);

                    // Add all colours
                    objColour.Add(objectType0[i].gameObject.GetComponent<MeshRenderer>().material.color);

                    // move to folder
                    objectType0[i].transform.parent = Layers[0].transform;
                }
            }
            // cycle through pillars
            for (int j = 0; j < objectType1.Length; j++)
            {
                if (objectType1[j].name.Contains("Pillar"))
                {
                    prefabType.Add(1);
                    positions.Add(objectType1[j].gameObject.transform.position);
                    rotation.Add(objectType1[j].gameObject.transform.rotation);
                    objColour.Add(objectType1[j].gameObject.GetComponent<MeshRenderer>().material.color);

                    objectType1[j].transform.parent = Layers[0].transform;
                }
            }
            // cycle through platforms
            for (int k = 0; k < objectType2.Length; k++)
            {
                if (objectType2[k].name.Contains("Platform"))
                {
                    prefabType.Add(2);
                    positions.Add(objectType2[k].gameObject.transform.position);
                    rotation.Add(objectType2[k].gameObject.transform.rotation);
                    objColour.Add(objectType2[k].gameObject.GetComponent<MeshRenderer>().material.color);
                    objectType2[k].transform.parent = Layers[0].transform;
                }
            }
            // cycle through wedges
            for (int l = 0; l < objectType3.Length; l++)
            {
                if (objectType3[l].name.Contains("Wedge"))
                {
                    prefabType.Add(3);
                    positions.Add(objectType3[l].gameObject.transform.position);
                    rotation.Add(objectType3[l].gameObject.transform.rotation);
                    objColour.Add(objectType3[l].gameObject.GetComponent<MeshRenderer>().material.color);
                    objectType3[l].transform.parent = Layers[0].transform;
                }
            }
            // cycle through stairs
            for (int m = 0; m < objectType4.Length; m++)
            {
                if (objectType4[m].name.Contains("Step2Stair"))
                {
                    prefabType.Add(4);
                    positions.Add(objectType4[m].gameObject.transform.position);
                    rotation.Add(objectType4[m].gameObject.transform.rotation);

                    objColour.Add(objectType4[m].gameObject.GetComponentInChildren<MeshRenderer>().material.color);
                    objColour.Add(objectType4[m].transform.GetChild(1).GetComponentInChildren<MeshRenderer>().material.color);
                    objectType4[m].transform.parent = Layers[0].transform;
                }
            }

            print("Setup lists");
        }

        private void ClearData()
        {
            // set object type - 0.cube, 1.pillar, 2.platform, 3.wedge, 4.step2stair
            prefabType.Clear();

            // Add all positions
            positions.Clear();

            // add all rotations
            rotation.Clear();

            // Add all colours
            objColour.Clear();

            print("data cleared");
        }

        // save to JSON format to cut down file size
        public void SaveRoomNew(int n)
        {
            SetObjectType();

            // string sceneNumber, List<int> goNumber, List<int> objectType, List<Transform> transforms, List<Color> objColour
            sceneData = new SceneData(prefabType, positions, rotation, objColour);

            // save all objects
            dataAsJson = JsonUtility.ToJson(sceneData, true);

            string filePath = Application.persistentDataPath + saveFileName + n + ".json";
            File.WriteAllText(filePath, dataAsJson);

#if UNITY_EDITOR
            //OpenInFileBrowser.OpenInMac(filePath);
#endif

            print("new save: " + n);

            // disable UI
            ui_Manager.ShowSaveMenu();
            ui_Manager.Show_MenuScreen();

            // Take screenshot
            StartCoroutine(CaptureScreenshot(n, 400, 250, "Screenshot"));

            ui_Manager.infoText.text = "Scene " + n + " saved.";
            StartCoroutine(ResetSaveInfo());

            StartCoroutine(UI_Manager.Instance.CanNowSpawn());
        }

        public void LoadRoomNew(int n)
        {
            // get file
            string filePath = Application.persistentDataPath + saveFileName + n + ".json";

            print("file path: " + filePath);

            // load into object
            if (File.Exists(filePath))
            {
                string jsonFile = File.ReadAllText(filePath);

                loadData = JsonUtility.FromJson<SceneData>(jsonFile);

            } else {
                print("error");
                StartCoroutine(UI_Manager.Instance.ShowInfoText("File doesn't exist"));
            }

            for (int i = 0; i < loadData.prefabType.Count; i++)
            {
                for (int j = 0; j < UI_Manager.Instance.prefabs.Length; j++)
                {
                    if (loadData.prefabType[i] == j)
                    {
                        spawnedGo = Instantiate(UI_Manager.Instance.prefabs[j], loadData.positions[i], loadData.rotation[i]);
                        spawnedGo.GetComponent<MeshRenderer>().material.color = loadData.objColour[i];
                        spawnedGo.transform.parent = Layers[0].transform;
                    }
                }
            }

            // switch menus off
            ui_Manager.ShowLoadMenu();
            ui_Manager.Show_MenuScreen();

            ui_Manager.ShowInfoText("Scene " + n + " Loaded.");
            StartCoroutine(ResetSaveInfo());

            StartCoroutine(UI_Manager.Instance.CanNowSpawn());

            //ClearData();
        }

        // to be used publicly
        public void TakeScreenshot(int n, int sizeX, int sizeY, string screenName){
            StartCoroutine(CaptureScreenshot(n, sizeX, sizeY, screenName));
        }

        private IEnumerator CaptureScreenshot(int n, int sizeX, int sizeY, string screenName)
        {
            // Switch UI off
            UI_Manager.Instance.SwitchOffUI();

            yield return new WaitForEndOfFrame();

            tempPath = Application.persistentDataPath + "/" + screenName // Application.streamingAssetsPath
                    + "_" + n + ".png";

            print("CaptureScreenshot screenshot path: " + tempPath);

            Texture2D screenImage = new Texture2D(Screen.width, Screen.height);

            //Get Image from screen
            screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenImage.Apply();

            // resize image before writing to file
            Scale(screenImage, sizeX, sizeY); // 198x100

            //Convert to png
            byte[] imageBytes = screenImage.EncodeToPNG();
            screenImage.LoadImage(imageBytes);

            //Save image to file
            System.IO.File.WriteAllBytes(tempPath, imageBytes);

            // switch UI back on
            UI_Manager.Instance.SwitchOnUI();
        }

        private IEnumerator ResetSaveInfo()
        {
            yield return new WaitForSeconds(3);
            ui_Manager.infoText.text = "";
        }

        // deletes everything - works
        public void NewScene()
        {
            DestroyObjects("Cube");
            DestroyObjects("Pillar");
            DestroyObjects("Platform");
            DestroyObjects("Wedge");
            DestroyObjects("Step2Stair");

            print("new scene");
            ui_Manager.Show_MenuScreen();
        }

        private void DestroyObjects(string tagName)
        {
            objectsArray = GameObject.FindGameObjectsWithTag(tagName);

            for (int i = 0; i < objectsArray.Length; i++)
            {
                Destroy(objectsArray[i]);
            }
        }

        private IEnumerator WaitWhileDeleting()
        {
            yield return new WaitForSeconds(1f);
        }

        // BORROWED
        // Scales the texture data of the given texture.
        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(tex, width, height, mode);

            // Update new texture
            tex.Resize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);

            print("image scaled. w: " + width + ", h: " + height);
        }

        // BORROWED
        // Internal utility that renders the source texture into the RTT - the scaling method itself.
        private static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 16);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }

    [System.Serializable]
    public class SceneData
    {
        public List<int> prefabType;
        public List<Vector3> positions;
        public List<Quaternion> rotation;
        public List<Color> objColour;

        public SceneData(List<int> prefabType, List<Vector3> positions, List<Quaternion> rotation, List<Color> objColour)
        {
            this.prefabType = prefabType;
            this.positions = positions;
            this.rotation = rotation;
            this.objColour = objColour;
        }
    }
}