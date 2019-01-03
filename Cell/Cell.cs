using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

namespace Gene {
    public class Cell : MonoBehaviour {
        [Header ("Connection to API Service")]
        public PostGene postGene;
        public AgentConfig agentConfig;
        public bool postApiData;
        public bool requestApiData;
        public string cellId;

        private int cellInfoIndex = 0;
        private bool initialised;
        private List<List<GameObject>> Germs;
        private List<GameObject> Cells;
        private List<Vector3> CellPositions;
        private AgentTrainBehaviour aTBehaviour;

        [HideInInspector] public bool isRequestDone;
        [HideInInspector] public float threshold;
        [HideInInspector] public int partNb;
        [HideInInspector] public List<float> CellInfos;

        void Awake () {
            initialised = false;
        }

        void Update () {
            string[] response = postGene.response;
            if (response.Length != 0 && !initialised) {
                for (int i = 1; i < response.Length; i++) {
                    float val = float.Parse (response[i].Split ('\\') [0], System.Globalization.CultureInfo.InvariantCulture);
                    CellInfos.Add (val);
                }
                initGerms (partNb, threshold);
                initialised = true;
            }
        }

        public void initGerms (int numGerms, float threshold) {

            transform.gameObject.name = transform.GetComponent<AgentTrainBehaviour>().brain +  Random.Range (0, 1000);

            Germs = new List<List<GameObject>> ();
            Cells = new List<GameObject> ();
            CellPositions = new List<Vector3> ();

            List<Vector3> sides = new List<Vector3> {
                new Vector3 (1f, 0f, 0f),
                new Vector3 (0f, 1f, 0f),
                new Vector3 (0f, 0f, 1f),
                new Vector3 (-1f, 0f, 0f),
                new Vector3 (0f, -1f, 0f),
                new Vector3 (0f, 0f, -1f)
            };

            Germs.Add (new List<GameObject> ());
            GameObject initCell = InitBaseShape (Germs[0], 0);
            initCell.transform.parent = transform;
            InitRigidBody (initCell);
            HandleStoreCell (initCell, initCell.transform.localPosition);

            for (int y = 1; y < numGerms; y++) {
                int prevCount = Germs[y - 1].Count;
                Germs.Add (new List<GameObject> ());

                for (int i = 0; i < prevCount; i++) {
                    for (int z = 0; z < sides.Count; z++) {
                        bool isValid = true;
                        float cellInfo = 0f;
                        Vector3 cellPosition = Germs[y - 1][i].transform.position + sides[z];

                        isValid = CheckIsValid (isValid, cellPosition);
                        cellInfo = HandleCellsRequest (cellInfoIndex);

                        if (isValid) {
                            if (cellInfo > threshold) {
                                GameObject cell = InitBaseShape (Germs[y], y);
                                InitPosition (sides, y, i, z, cell);
                                InitRigidBody (cell);
                                initJoint (cell, Germs[y - 1][i], sides[z], y, z);
                                HandleStoreCell (cell, cellPosition);
                                cell.transform.parent = transform;
                                cell.GetComponent<Renderer> ().material.SetVector ("_position", new Vector2 (cell.transform.position.z, cell.transform.position.y));
                                cell.GetComponent<Renderer> ().material.SetFloat ("_X", (cell.transform.position.magnitude * 6) / 220);
                                cell.GetComponent<Renderer> ().material.SetFloat ("_Scale", (cell.transform.position.magnitude * 90) / 220);
                            }
                        }
                    }
                }

                foreach (var cell in Cells) {
                    cell.transform.parent = transform;
                }
            }

            foreach (var cell in Cells) {
                cell.GetComponent<SphereCollider> ().radius /= 2f;
            }

            AddAgentPart ();

            if (postApiData) {
                string postData = HandlePostData ();
                StartCoroutine (postGene.postCell (postData, transform.gameObject.name));
            }
        }

        private void HandleStoreCell (GameObject cell, Vector3 cellPosition) {
            Cells.Add (cell);
            CellPositions.Add (cellPosition);
        }

        private string HandlePostData () {
            string postData = "";
            foreach (var info in CellInfos) {
                postData = postData + 'A' + info.ToString ();
            }

            return postData;
        }

        private float HandleCellsRequest (int x) {
            if (requestApiData) {
                cellInfoIndex++;
                return CellInfos[x];
            } else {
                float cellInfo = Random.Range (0f, 1f);
                CellInfos.Add (cellInfo);
                cellInfoIndex++;
                return cellInfo;
            }
        }

        private static void InitRigidBody (GameObject cell) {
            cell.AddComponent<Rigidbody> ();
            cell.GetComponent<Rigidbody> ().useGravity = true;
            cell.GetComponent<Rigidbody> ().mass = 1f;
        }

        private void InitPosition (List<Vector3> sides, int y, int i, int z, GameObject cell) {
            cell.transform.parent = Germs[y - 1][i].transform;
            cell.transform.localPosition = sides[z];
        }

        private GameObject InitBaseShape (List<GameObject> germs, int y) {
            germs.Add (GameObject.CreatePrimitive (PrimitiveType.Sphere));
            GameObject cell = Germs[y][Germs[y].Count - 1];
            cell.transform.position = transform.position;
            return cell;
        }

        private bool CheckIsValid (bool isValid, Vector3 cellPosition) {
            foreach (var position in CellPositions) {
                if (cellPosition == position) {
                    isValid = false;
                }
            }

            return isValid;
        }

        private void initJoint (GameObject part, GameObject connectedBody, Vector3 jointAnchor, int y, int z) {
            ConfigurableJoint cj = part.transform.gameObject.AddComponent<ConfigurableJoint> ();
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
            cj.angularXMotion = ConfigurableJointMotion.Limited;
            cj.angularYMotion = ConfigurableJointMotion.Limited;
            cj.angularZMotion = ConfigurableJointMotion.Limited;
            cj.anchor = new Vector3 (0f, 0f, 0f);
            cj.connectedBody = connectedBody.gameObject.GetComponent<Rigidbody> ();
            cj.rotationDriveMode = RotationDriveMode.Slerp;
            cj.angularYLimit = new SoftJointLimit () { limit = agentConfig.yLimit, bounciness = agentConfig.bounciness };
            cj.highAngularXLimit = new SoftJointLimit () { limit = agentConfig.highXLimit, bounciness = agentConfig.bounciness };
            cj.lowAngularXLimit = new SoftJointLimit () { limit = agentConfig.lowXLimit, bounciness = agentConfig.bounciness };
            cj.angularZLimit = new SoftJointLimit () { limit = agentConfig.zLimit, bounciness = agentConfig.bounciness };
            part.gameObject.GetComponent<Rigidbody> ().useGravity = true;
            part.gameObject.GetComponent<Rigidbody> ().mass = 1f;
        }

        private void AddAgentPart () {
            aTBehaviour = transform.gameObject.GetComponent<AgentTrainBehaviour> ();
            aTBehaviour.initPart = Cells[0].transform;
            for (int i = 1; i < Cells.Count; i++) {
                aTBehaviour.parts.Add (Cells[i].transform);
            }
            aTBehaviour.initBodyParts ();
        }
    }
}