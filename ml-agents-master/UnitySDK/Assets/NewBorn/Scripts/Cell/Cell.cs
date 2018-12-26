using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MLAgents;

namespace Gene {
    public class Cell : MonoBehaviour
    {
        [Header("Connection to API Service")]
        public bool postApiData;
        public bool requestApiData;
        public string cellId;
        [HideInInspector] public bool isRequestDone;
        [HideInInspector] public float threshold;
        [HideInInspector] public int partNb;
        [HideInInspector] public List<float> CellInfos;
        private int cellInfoIndex = 0;
        private bool initialised;
        private List<List<GameObject>> Germs;
        private List<GameObject> Cells;
        private List<Vector3> CellPositions;
        private AgentTrainBehaviour aTBehaviour;

        public PostGene postGene;

        void Awake()
        {
            initialised = false;
        }

        void Update()
        {
            if (postGene.response.Length != 0 && !initialised)
            {
                for (int i = 1; i < postGene.response.Length; i++)
                {
                    Debug.Log(postGene.response[i]);
                    Debug.Log(postGene.response[i].Split('\\')[0]);
                    float val = float.Parse(postGene.response[i].Split('\\')[0], System.Globalization.CultureInfo.InvariantCulture);
                    CellInfos.Add(val);
                }
                initGerms(partNb, threshold);
                initialised = true;
            }
        }

        public void initGerms(int numGerms, float threshold)
        {
            // Physics.gravity = Vector3.zero;
            // RENAME THE PARENT GAMEOBJECT//
            //transform.gameObject.name = Utils.RandomName();
            /////////////////////////////////
            // INIT GENE LIST //
            ////////////////////
            Germs = new List<List<GameObject>>();
            Cells = new List<GameObject>();
            CellPositions = new List<Vector3>();

            List<Vector3> sides = new List<Vector3>{
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(-1f, 0f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, 0f, -1f)
            };

            //////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////INIT BASE GERMS///////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////
            /// 1ST CELL ///
            // init object shape
            Germs.Add(new List<GameObject>());
            GameObject initCell = InitBaseShape(Germs[0], 0);
            InitRigidBody(initCell);
            initCell.transform.parent = transform;
            HandleStoreCell(initCell, initCell.transform.localPosition);

            //////////////////////////////////////////////////////////////////////////////////////
            /////////////////// Iterate for each new part of the morphology //////////////////////
            /// //////////////////////////////////////////////////////////////////////////////////
            for (int y = 1; y < numGerms; y++)
            {
                int prevCount = Germs[y - 1].Count;
                Germs.Add(new List<GameObject>());

                //////////////////////////////////////////////////
                /// ITERATE FOR EACH PREVIOUS GERM CELL NUMBER ///
                for (int i = 0; i < prevCount; i++)
                {
                    //////////////////////////////////////////////////
                    ////////// ITERATE FOR EACH CELL SIDES ///////////
                    for (int z = 0; z < sides.Count; z++)
                    {
                        /// RANDOM ITERATION FROM THE PREVIOUS CELL
                        
                        bool isValid = true;
                        float cellInfo = 0f;
                        Vector3 cellPosition = Germs[y - 1][i].transform.position + sides[z];

                        isValid = CheckIsValid(isValid, cellPosition);
                        cellInfo = HandleCellsRequest(cellInfoIndex);

                        if(isValid) {
                            if(cellInfo > threshold) {
                                GameObject cell = InitBaseShape(Germs[y], y);
                                InitPosition(sides, y, i, z, cell);
                                InitRigidBody(cell);
                                initJoint(cell, Germs[y - 1][i], sides[z], y, z);
                                HandleStoreCell(cell, cellPosition);
                                cell.transform.parent = transform;
                                cell.GetComponent<Renderer>().material.SetVector("_position", new Vector2(cell.transform.position.z, cell.transform.position.y));
                                cell.GetComponent<Renderer>().material.SetFloat("_X", (cell.transform.position.magnitude * 6) / 220);
                                cell.GetComponent<Renderer>().material.SetFloat("_Scale", (cell.transform.position.magnitude * 90) / 220);
                            }
                        }
                    }
                }

                foreach(var cell in Cells) {
                    cell.transform.parent = transform;
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////


            foreach (var cell in Cells)
            {
                //cell.transform.localScale *= 2f;
                cell.GetComponent<SphereCollider>().radius /= 2f;
            }

            //////////////////////////////////////////////////////////////////////////////////////
            AddAgentPart();

            if (postApiData)
            {
                ////Post data to Api
                //GameObject.Find("Focus Camera").GetComponent<WebCamPhotoCamera>().CaptureScreenshot();
                string postData = HandlePostData();
                StartCoroutine(postGene.postCell(postData, transform.gameObject.name));
            }
            //gameObject.transform.GetChild(0).transform.parent = gameObject.transform.GetChild(1).transform;
        }

        private void HandleStoreCell(GameObject cell, Vector3 cellPosition)
        {
            Cells.Add(cell);
            CellPositions.Add(cellPosition);
        }

        private string HandlePostData()
        {
            string postData = "";
            foreach (var info in CellInfos)
            {   
                postData = postData + 'A' + info.ToString();
            }

            return postData;
        }

        private float HandleCellsRequest(int x)
        {
            if (requestApiData)
            {
                cellInfoIndex++;
                return CellInfos[x];
            }
            else
            {
                float cellInfo = Random.Range(0f, 1f);
                CellInfos.Add(cellInfo);
                cellInfoIndex++;
                return cellInfo;
            }
        }

        private static void InitRigidBody(GameObject cell)
        {
            cell.AddComponent<Rigidbody>();
            cell.GetComponent<Rigidbody>().useGravity = true;
            cell.GetComponent<Rigidbody>().mass = 1f;
        }

        private void InitPosition(List<Vector3> sides, int y, int i, int z, GameObject cell)
        {
            cell.transform.parent = Germs[y - 1][i].transform;
            cell.transform.localPosition = sides[z];
        }

        private GameObject InitBaseShape(List<GameObject> germs, int y)
        {
            germs.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            GameObject cell = Germs[y][Germs[y].Count - 1];
            cell.transform.position = transform.position;
            //cell.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            //cell.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/cell/cell");
            
            return cell;
        }

        private bool CheckIsValid(bool isValid, Vector3 cellPosition)
        {
            foreach (var position in CellPositions)
            {
                if (cellPosition == position)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private void initJoint(GameObject part, GameObject connectedBody, Vector3 jointAnchor, int y, int z)
        {
            ConfigurableJoint cj = part.transform.gameObject.AddComponent<ConfigurableJoint>();
            ///////////////////////
            // Configurable Joint Motion 
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
            // Configurable Joint Angular Mortion
            cj.angularXMotion = ConfigurableJointMotion.Limited;
            cj.angularYMotion = ConfigurableJointMotion.Limited;
            cj.angularZMotion = ConfigurableJointMotion.Limited;
            // Configurable Joint Connected Body AND Anchor settings
            cj.anchor = new Vector3(0f, 0f, 0f);
            cj.connectedBody = connectedBody.gameObject.GetComponent<Rigidbody>();
            cj.rotationDriveMode = RotationDriveMode.Slerp;
            // Configurable Joint Angular Limit
            cj.angularYLimit = new SoftJointLimit() { limit = 90f, bounciness = 10f };
            cj.highAngularXLimit = new SoftJointLimit() { limit = 50f, bounciness = 10f };
            cj.lowAngularXLimit = new SoftJointLimit() { limit = 0f, bounciness = 10f };
            cj.angularZLimit = new SoftJointLimit() { limit = 1f, bounciness = 10f };
            part.gameObject.GetComponent<Rigidbody>().useGravity = true;
            part.gameObject.GetComponent<Rigidbody>().mass= 1f; 
        }

        private void AddAgentPart()
        {
            aTBehaviour = transform.gameObject.GetComponent<AgentTrainBehaviour>();
            aTBehaviour.initPart = Cells[0].transform;
            for (int i = 1; i < Cells.Count; i++)
            {
                aTBehaviour.parts.Add(Cells[i].transform);
            }
            aTBehaviour.initBodyParts();
        }
    }
}
