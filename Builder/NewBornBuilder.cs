using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAgents;
using UnityEngine;

namespace Gene
{
  public class NewBornBuilder : MonoBehaviour
  {
    public GameObject CellPrefab;
    [Header("Connection to API Service")]
    public NewbornService newbornService;
    public AgentConfig agentConfig;
    public bool postApiData;
    public bool requestApiData;
    public string cellId;
    public int cellNb = 0;
    public int minCellNb;
    private int cellInfoIndex = 0;
    private bool Initialised;
    public List<List<GameObject>> NewBornGenerations;
    public List<GameObject> Cells;
    public List<Vector3> CellPositions;

    public List<Vector3> CelllocalPositions;
    private AgentTrainBehaviour aTBehaviour;

    private TrainingManager trainingManager;
    private List<Vector3> sides = new List<Vector3> {
                new Vector3 (1f, 0f, 0f),
                new Vector3 (0f, 1f, 0f),
                new Vector3 (0f, 0f, 1f),
                new Vector3 (-1f, 0f, 0f),
                new Vector3 (0f, -1f, 0f),
                new Vector3 (0f, 0f, -1f)
            };

    private Vector3 childPositionSum = new Vector3(0f, 0f, 0f);

    [HideInInspector] public Vector3 center;
    [HideInInspector] public bool isRequestDone;
    [HideInInspector] public float threshold;
    [HideInInspector] public int partNb;
    [HideInInspector] public List<List<float>> ModelInfosList = new List<List<float>>();


    void Awake()
    {
      trainingManager = GameObject.Find("TrainingManager").transform.GetComponent<TrainingManager>();
      Initialised = false;
    }

    void Update()
    {
      childPositionSum = new Vector3(0f, 0f, 0f);
      foreach (Transform child in transform)
      {
        childPositionSum += child.transform.position;
      }

      center = (childPositionSum / transform.childCount);
    }

    public void DeleteCells()
    {
      Cells.Clear();
      NewBornGenerations.Clear();
      CellPositions.Clear();
      CelllocalPositions.Clear();
      cellInfoIndex = 0;
      Initialised = false;
      ModelInfosList.Clear();
      cellNb = 0;
    }

    public void handleCellInfoResponse()
    {
      List<float> cellInfoResponse = newbornService.cellInfoResponse;
      if (cellInfoResponse.Count != 0 && !Initialised)
      {
        ModelInfosList.Add(new List<float>());
        for (int i = 0; i < cellInfoResponse.Count; i++)
        {
          ModelInfosList[0].Add(cellInfoResponse[i]);
        }
        initNewBorn(partNb, threshold);
        Initialised = true;
      }
    }

    public void initNewBorn(int generationNumber, float threshold)
    {
      transform.gameObject.name = transform.GetComponent<AgentTrainBehaviour>().brain + "";
      NewBornGenerations = new List<List<GameObject>>();
      Cells = new List<GameObject>();
      CellPositions = new List<Vector3>();
      CelllocalPositions = new List<Vector3>();
      if (ModelInfosList.Count == 0)
      {
        ModelInfosList.Add(new List<float>());
      }
      NewBornGenerations.Add(new List<GameObject>());
      GameObject initCell = InitBaseShape(NewBornGenerations[0], 0);
      initCell.transform.parent = transform;
      InitRigidBody(initCell);
      HandleStoreCell(initCell, initCell.transform.position, initCell.transform.position);
      for (int y = 1; y < generationNumber; y++)
      {
        int previousGenerationCellNumber = NewBornGenerations[y - 1].Count;
        NewBornGenerations.Add(new List<GameObject>());
        for (int i = 0; i < previousGenerationCellNumber; i++)
        {
          for (int z = 0; z < sides.Count; z++)
          {
            if (!requestApiData || cellInfoIndex < ModelInfosList[0].Count)
            {
              bool isValid = true;
              float cellInfo = 0f;
              Vector3 cellPosition = NewBornGenerations[y - 1][i].transform.position + sides[z];
              isValid = CheckIsValid(isValid, cellPosition);
              cellInfo = HandleCellInfos(0, cellInfoIndex);
              cellInfoIndex++;
              if (isValid)
              {
                if (cellInfo > threshold)
                {
                  GameObject cell = InitBaseShape(NewBornGenerations[y], y);
                  InitPosition(sides, y, i, z, cell);
                  InitRigidBody(cell);
                  initJoint(cell, NewBornGenerations[y - 1][i], sides[z], y, z);
                  cell.transform.parent = transform;
                  HandleStoreCell(cell, cellPosition, cellPosition);
                }
              }
            }
          }
        }
      }


      foreach (var cell in Cells)
      {
        cell.transform.parent = transform; // RESET CELL TO MAIN TRANSFORM
        cell.GetComponent<SphereCollider>().radius /= 2f;
      }

      cellNb = Cells.Count;

      checkMinCellNb();
      AddAgentPart(true);
    }

    public void BuildGeneration(int generationInfo, bool isAfterRequest)
    {
      int indexInfo = 0;
      int previousGenerationCellNumber = 0;
      int germNb = 0;
      partNb += 1;


      for (int i = 0; i < NewBornGenerations.Count; i++)
      {
        if (NewBornGenerations[i].Count > 0)
        {
          previousGenerationCellNumber = NewBornGenerations[i].Count;
          germNb = i;
        }
        else
        {
          NewBornGenerations.RemoveAt(i);
        }
      }
      if (!isAfterRequest)
      {
        ModelInfosList.Add(new List<float>());
      }

      NewBornGenerations.Add(new List<GameObject>());

      for (int i = 0; i < previousGenerationCellNumber; i++)
      {
        for (int z = 0; z < sides.Count; z++)
        {
          bool isValid = true;
          float cellInfo = 0f;
          Vector3 cellPosition = NewBornGenerations[germNb][i].transform.position + sides[z];
          isValid = CheckIsValid(isValid, cellPosition);
          cellInfo = HandleCellInfos(ModelInfosList.Count - 1, indexInfo);
          indexInfo++;
          if (isValid)
          {
            if (cellInfo > threshold)
            {
              GameObject cell = InitBaseShape(NewBornGenerations[germNb + 1], germNb + 1);
              InitPosition(sides, germNb + 1, i, z, cell);
              InitRigidBody(cell);
              initJoint(cell, NewBornGenerations[germNb][i], sides[z], germNb + 1, z);
              cell.transform.parent = transform;
              HandleStoreCell(cell, cellPosition, cellPosition);
            }
          }
        }
      }
      cellNb = Cells.Count;
      AddAgentPart(false);
    }

    private void checkMinCellNb()
    {
      if (cellNb < minCellNb)
      {
        Debug.Log("Killin Object (less that requiered size");
        transform.gameObject.SetActive(false);
      }
    }

    public List<float> ReturnModelInfosList(int modelId)
    {
      List<float> ModelInfos = new List<float>();

      for (int i = 0; i < ModelInfosList[modelId].Count; i++)
      {
        ModelInfos.Add(ModelInfosList[modelId][i]);
      }

      return ModelInfos;
    }

    public List<List<float>> ReturnModelPositions()
    {
      List<List<float>> positions = new List<List<float>>();
      for (int i = 0; i < CelllocalPositions.Count; i++)
      {
        List<float> position = new List<float>();
        position.Add(CelllocalPositions[i].x);
        position.Add(CelllocalPositions[i].y);
        position.Add(CelllocalPositions[i].z);
        positions.Add(position);
      }
      return positions;
    }

    public void PostCell(string newbornId, int agentId)
    {
      string newBornName = "\"cellName\"";
      string nexBornHexColor = "\"red\"";
      NewBornPostData newBornPostData = new NewBornPostData(newBornName, newbornId, nexBornHexColor);
      StartCoroutine(newbornService.PostNewborn(newBornPostData, agentId));
    }

    public void PostNewbornModel(string newbornId, int generationId, int agentId)
    {
      List<float> ModelInfos = ReturnModelInfosList(generationId);
      List<List<float>> cellPositions = ReturnModelPositions();
      string id = Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", "");
      GenerationPostData generationPostData = new GenerationPostData(newbornId, cellPositions, ModelInfos);
      StartCoroutine(newbornService.PostNewbornModel(generationPostData, newbornId, agentId));
    }

    private void HandleStoreCell(GameObject cell, Vector3 cellPosition, Vector3 cellLocalPosition)
    {
      Cells.Add(cell);
      CellPositions.Add(cellPosition);
      CelllocalPositions.Add(cellLocalPosition);
    }

    private float HandleCellInfos(int generationIndex, int cellIndex)
    {
      if (requestApiData)
      {
        float cellInfo = ModelInfosList[generationIndex][cellIndex];
        return cellInfo;
      }
      else
      {
        float cellInfo = Random.Range(0f, 1f);
        ModelInfosList[generationIndex].Add(cellInfo);
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
      cell.transform.parent = NewBornGenerations[y - 1][i].transform;
      cell.transform.localPosition = sides[z];
    }

    private GameObject InitBaseShape(List<GameObject> NewBornGeneration, int y)
    {
      NewBornGeneration.Add(Instantiate(CellPrefab));
      GameObject cell = NewBornGenerations[y][NewBornGenerations[y].Count - 1];
      cell.transform.position = transform.position;
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
      cj.xMotion = ConfigurableJointMotion.Locked;
      cj.yMotion = ConfigurableJointMotion.Locked;
      cj.zMotion = ConfigurableJointMotion.Locked;
      cj.angularXMotion = ConfigurableJointMotion.Limited;
      cj.angularYMotion = ConfigurableJointMotion.Limited;
      cj.angularZMotion = ConfigurableJointMotion.Limited;
      cj.anchor = -jointAnchor;
      cj.connectedBody = connectedBody.gameObject.GetComponent<Rigidbody>();
      cj.rotationDriveMode = RotationDriveMode.Slerp;
      cj.angularYLimit = new SoftJointLimit() { limit = agentConfig.yLimit, bounciness = agentConfig.bounciness };
      handleAngularLimit(cj, jointAnchor);
      cj.angularZLimit = new SoftJointLimit() { limit = agentConfig.zLimit, bounciness = agentConfig.bounciness };
      part.gameObject.GetComponent<Rigidbody>().useGravity = true;
      part.gameObject.GetComponent<Rigidbody>().mass = 1f;
    }

    private void handleAngularLimit(ConfigurableJoint cj, Vector3 jointAnchor)
    {
      if (jointAnchor.y == -1)
      {
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -agentConfig.highLimit, bounciness = agentConfig.bounciness };
        cj.highAngularXLimit = new SoftJointLimit() { limit = -agentConfig.lowLimit, bounciness = agentConfig.bounciness };
      }
      else if (jointAnchor.y == 1)
      {
        cj.highAngularXLimit = new SoftJointLimit() { limit = agentConfig.highLimit, bounciness = agentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = agentConfig.lowLimit, bounciness = agentConfig.bounciness };
      }
      else if (jointAnchor.x == 1)
      {
        cj.highAngularXLimit = new SoftJointLimit() { limit = agentConfig.highLimit, bounciness = agentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = agentConfig.lowLimit, bounciness = agentConfig.bounciness };
        cj.axis = new Vector3(0f, -1f, 0f);
      }
      else if (jointAnchor.x == -1)
      {
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -agentConfig.highLimit, bounciness = agentConfig.bounciness };
        cj.highAngularXLimit = new SoftJointLimit() { limit = -agentConfig.highLimit, bounciness = agentConfig.bounciness };
        cj.axis = new Vector3(0f, -1f, 0f);
      }
      else if (jointAnchor.z == 1)
      {
        cj.axis = new Vector3(0f, -1f, 0f);
        cj.highAngularXLimit = new SoftJointLimit() { limit = agentConfig.lowLimit, bounciness = agentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -agentConfig.lowLimit, bounciness = agentConfig.bounciness };
      }
      else if (jointAnchor.z == -1)
      {
        cj.axis = new Vector3(-1f, 0f, 0f);
        cj.highAngularXLimit = new SoftJointLimit() { limit = agentConfig.lowLimit, bounciness = agentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -agentConfig.lowLimit, bounciness = agentConfig.bounciness };
      }
    }

    private void AddAgentPart(bool init)
    {
      aTBehaviour = transform.gameObject.GetComponent<AgentTrainBehaviour>();
      aTBehaviour.initPart = Cells[0].transform;
      for (int i = 1; i < cellNb; i++)
      {
        if (aTBehaviour.parts.Count < i)
        {
          aTBehaviour.parts.Add(Cells[i].transform);
        }
      }
      if (init)
      {
        aTBehaviour.initBodyParts();
      }
    }
  }
}