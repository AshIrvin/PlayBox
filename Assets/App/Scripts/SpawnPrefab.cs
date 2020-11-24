using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.App.Scripts
{
    public class SpawnPrefab : MonoBehaviour
    {
        #region Public Variables
        public static SpawnPrefab Instance { get; set; }

        public GameObject selectedObject;
        public GameObject Prefab;
        public int objectType;
        public int setToLayer;
        public bool buildMode;
        public bool showGui;
        public Color selectedObjectcolour;

        #endregion

        #region Private Variables

        private int objectNo;
        private GameObject player;
        private GameObject head;
        private float centerScreenX = Screen.width / 2;
        
        private MeshRenderer tempMeshRenderer;

        #endregion

        void Awake()
        {
            Instance = this;
            buildMode = true;

            player = GameObject.Find("Player");
            head = GameObject.Find("Head");

            //selectedObject.tag = "Layer01";

            selectedObjectcolour = Color.red;
        }

        // SET INITIAL SPAWNING OBJECT
        public void SetUpObject()
        {
            //selectedObject = Drop.selected;
            selectedObject = UI_Manager.Instance.prefabs[0];
        }

        public void SpawnCube(RaycastHit hit)
        {
            // get vector3 point of impact
            Vector3 goPos = hit.point;

            // check size of object being spawned
            // selectedObject.gameObject.GetComponent<RectTransform>()

            // change to snap amount
            float snap = UI_Manager.Instance.snap;

            Vector3 newPos = new Vector3(Mathf.Round(goPos.x / snap) * snap, Mathf.Round(goPos.y / snap) * snap, Mathf.Round(goPos.z / snap) * snap);

            if (buildMode)
            {
                // BUILD OFFLINE
                //if (!PhotonNetwork.inRoom)
                //{
                    var spawnedGO = Instantiate(selectedObject, newPos, Quaternion.identity);
                    spawnedGO.transform.parent = SaveLoadManager.Instance.Layers[0].transform;
                    
                    string tempName = spawnedGO.name;
                    tempName = tempName + objectNo;

                    spawnedGO.name = tempName;
                    // colour object
                    // find mesh rendered in root or sub folder
                    if (spawnedGO.gameObject.GetComponent<MeshRenderer>())
                    {
                        spawnedGO.gameObject.GetComponent<MeshRenderer>().material.color = CameraManager.Instance.tempColor;
                    } else {
                        spawnedGO.GetComponentInChildren<MeshRenderer>().material.color = CameraManager.Instance.tempColor;
                        spawnedGO.gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material.color = CameraManager.Instance.tempColor;
                    }

                    objectNo++;

                    // add to list
                    //SaveCommands.Instance.AddToCommands();
                //}
                // BUILD ONLINE
                //else
                //{
                //    PhotonNetwork.Instantiate(selectedObject.name, newPos, Quaternion.identity, 0);
                //    print("Photon Instantiate");
                //}

                //GetObjectAfterSpawn();
            }

        }
        
        void GetObjectDetails(RaycastHit hit)
        {
            GameObject go = hit.collider.gameObject;
            Color colour = CameraManager.Instance.tempColor;
            //UndoRedo.Instance.CommandList(go, go.transform.position, go.transform.rotation, colour);
        }
        

        void SetTypeForObject()
        {
            if (UI_Manager.Instance.prefabs[0])
            {
                selectedObject.tag = "Cube";
                //objectType = 0;
            } else if (UI_Manager.Instance.prefabs[1])
            {
                selectedObject.tag = "Pillar";
                //objectType = 1;
            } else if (UI_Manager.Instance.prefabs[2])
            {
                selectedObject.tag = "Platform";
                //objectType = 2;
            } else if (UI_Manager.Instance.prefabs[3])
            {
                selectedObject.tag = "Wedge";
                //objectType = 3;
            } else if (UI_Manager.Instance.prefabs[4])
            {
                selectedObject.tag = "Step2Stair";
                //objectType = 4;
            }
        }

        // Todo - Set up layers
        void SetLayerForObject()
        {
            if (UI_Manager.Instance.selectedLayer == 0)
            {
                selectedObject.tag = "Layer01";
            }
        }
    }

}