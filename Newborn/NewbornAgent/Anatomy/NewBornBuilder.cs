using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// using MLAgents;
using UnityEngine;

namespace Gene
{
  [ExecuteInEditMode]
  public class NewBornBuilder : MonoBehaviour
  {

    private Newborn newborn;
    [Header("Connection to API Service")]
    public NewbornService newbornService;
    public bool requestApiData;
    public int cellNb = 0;
    public int minCellNb;
    private int cellInfoIndex = 0;
    private bool Initialised;
    private AgentTrainBehaviour aTBehaviour;
    private NewbornManager trainingManager;
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
    [HideInInspector] public float threshold;
    [HideInInspector] public int partNb;
    public List<GeneInformation> GeneInformations;

    void Awake()
    {
      newborn = transform.GetComponent<Newborn>();
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
      newborn.Cells.Clear();
      newborn.NewBornGenerations.Clear();
      newborn.CellPositions.Clear();
      newborn.CelllocalPositions.Clear();
      cellInfoIndex = 0;
      Initialised = false;
      GeneInformations.Clear();
      cellNb = 0;
    }

    public void handleCellInfoResponse()
    {
      List<float> cellInfoResponse = NewbornService.cellInfoResponse;
      if (cellInfoResponse.Count != 0 && !Initialised)
      {
        GeneInformations.Add(new GeneInformation(new List<float>()));
        for (int i = 0; i < cellInfoResponse.Count; i++)
        {
          GeneInformations[0].info.Add(cellInfoResponse[i]);
        }
        initNewBorn(partNb, threshold);
        Initialised = true;
      }
    }

    public void initNewBorn(int generationNumber, float threshold)
    {
      transform.gameObject.name = transform.GetComponent<AgentTrainBehaviour>().brain + "";
      newborn.title = transform.gameObject.name;
      newborn.NewBornGenerations = new List<List<GameObject>>();
      newborn.Cells = new List<GameObject>();
      newborn.CellPositions = new List<Vector3>();
      newborn.CelllocalPositions = new List<Vector3>();



      if (GeneInformations.Count == 0)
      {
        GeneInformations.Add(new GeneInformation(new List<float>()));
      }
      newborn.NewBornGenerations.Add(new List<GameObject>());
      GameObject initCell = InitBaseShape(newborn.NewBornGenerations[0], 0);
      initCell.transform.parent = transform;
      InitRigidBody(initCell);
      HandleStoreCell(initCell, initCell.transform.position, initCell.transform.position);
      for (int y = 1; y < generationNumber; y++)
      {
        int previousGenerationCellNumber = newborn.NewBornGenerations[y - 1].Count;
        newborn.NewBornGenerations.Add(new List<GameObject>());
        for (int i = 0; i < previousGenerationCellNumber; i++)
        {
          for (int z = 0; z < sides.Count; z++)
          {
            if (!requestApiData || cellInfoIndex < GeneInformations[0].info.Count)
            {
              bool isValid = true;
              float cellInfo = 0f;
              Vector3 cellPosition = newborn.NewBornGenerations[y - 1][i].transform.position + sides[z];
              isValid = CheckIsValid(isValid, cellPosition);
              cellInfo = HandleCellInfos(0, cellInfoIndex);
              cellInfoIndex++;
              if (isValid)
              {
                if (cellInfo > threshold)
                {
                  GameObject cell = InitBaseShape(newborn.NewBornGenerations[y], y);
                  InitPosition(sides, y, i, z, cell);
                  InitRigidBody(cell);
                  initJoint(cell, newborn.NewBornGenerations[y - 1][i], sides[z], y, z);
                  cell.transform.parent = transform;
                  HandleStoreCell(cell, cellPosition, cellPosition);
                }
              }
            }
          }
        }
      }


      foreach (var cell in newborn.Cells)
      {
        cell.transform.parent = transform; // RESET CELL TO MAIN TRANSFORM
        cell.GetComponent<SphereCollider>().radius /= 2f;
      }

      cellNb = newborn.Cells.Count;

      checkMinCellNb();
      BuildAgentPart(true);
    }

    public void BuildGeneration(int generationInfo, bool isAfterRequest)
    {
      int indexInfo = 0;
      int previousGenerationCellNumber = 0;
      int germNb = 0;
      partNb += 1;


      for (int i = 0; i < newborn.NewBornGenerations.Count; i++)
      {
        if (newborn.NewBornGenerations[i].Count > 0)
        {
          previousGenerationCellNumber = newborn.NewBornGenerations[i].Count;
          germNb = i;
        }
        else
        {
          newborn.NewBornGenerations.RemoveAt(i);
        }
      }
      if (!isAfterRequest)
      {
        GeneInformations.Add(new GeneInformation(new List<float>()));
      }

      newborn.NewBornGenerations.Add(new List<GameObject>());

      for (int i = 0; i < previousGenerationCellNumber; i++)
      {
        for (int z = 0; z < sides.Count; z++)
        {
          bool isValid = true;
          float cellInfo = 0f;
          Vector3 cellPosition = newborn.NewBornGenerations[germNb][i].transform.position + sides[z];
          isValid = CheckIsValid(isValid, cellPosition);
          cellInfo = HandleCellInfos(GeneInformations.Count - 1, indexInfo);
          indexInfo++;
          if (isValid)
          {
            if (cellInfo > threshold)
            {
              GameObject cell = InitBaseShape(newborn.NewBornGenerations[germNb + 1], germNb + 1);
              InitPosition(sides, germNb + 1, i, z, cell);
              InitRigidBody(cell);
              initJoint(cell, newborn.NewBornGenerations[germNb][i], sides[z], germNb + 1, z);
              cell.transform.parent = transform;
              HandleStoreCell(cell, cellPosition, cellPosition);
            }
          }
        }
      }
      cellNb = newborn.Cells.Count;
      BuildAgentPart(false);
    }

    private void checkMinCellNb()
    {
      if (cellNb < minCellNb)
      {
        Debug.Log("Killin Object (less that requiered size");
        transform.gameObject.SetActive(false);
      }
    }

    public List<float> ReturnGeneInformations(int modelIndex)
    {
      List<float> ModelInfos = new List<float>();

      for (int i = 0; i < GeneInformations[modelIndex].info.Count; i++)
      {
        ModelInfos.Add(GeneInformations[modelIndex].info[i]);
      }

      return ModelInfos;
    }

    public List<PositionPostData> ReturnModelPositions()
    {
      List<PositionPostData> positions = new List<PositionPostData>();
      for (int i = 0; i < newborn.CelllocalPositions.Count; i++)
      {
        List<float> position = new List<float>();
        position.Add(newborn.CelllocalPositions[i].x);
        position.Add(newborn.CelllocalPositions[i].y);
        position.Add(newborn.CelllocalPositions[i].z);
        positions.Add(new PositionPostData(position));
      }
      return positions;
    }

    public void PostNewborn(NewBornPostData newBornPostData, GameObject agent)
    {
      StartCoroutine(NewbornService.PostNewborn(newBornPostData, agent));
    }

    public IEnumerator PostNewbornModel(string newbornId, int modelIndex, GameObject agent, NewbornService.RebuildAgentCallback responseCallback)
    {
      List<float> modelInfos = ReturnGeneInformations(modelIndex);
      List<PositionPostData> cellPositions = ReturnModelPositions();
      string id = Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", "");
      GenerationPostData generationPostData = new GenerationPostData(newbornId, cellPositions, modelInfos);
      yield return NewbornService.PostNewbornModel(transform, generationPostData, newbornId, agent, responseCallback);
    }

    private void HandleStoreCell(GameObject cell, Vector3 cellPosition, Vector3 cellLocalPosition)
    {
      newborn.Cells.Add(cell);
      newborn.CellPositions.Add(cellPosition);
      newborn.CelllocalPositions.Add(cellLocalPosition);
    }

    private float HandleCellInfos(int generationIndex, int cellIndex)
    {
      if (requestApiData)
      {
        float cellInfo = GeneInformations[generationIndex].info[cellIndex];
        return cellInfo;
      }
      else
      {
        float cellInfo = Random.Range(0f, 1f);
        GeneInformations[generationIndex].info.Add(cellInfo);
        return cellInfo;
      }
    }



    private static void InitRigidBody(GameObject cell)
    {
      // cell.AddComponent<Rigidbody>();
      cell.GetComponent<Rigidbody>().useGravity = true;
      cell.GetComponent<Rigidbody>().mass = 1f;
    }

    private void InitPosition(List<Vector3> sides, int y, int i, int z, GameObject cell)
    {
      cell.transform.parent = newborn.NewBornGenerations[y - 1][i].transform;
      cell.transform.localPosition = sides[z];
    }

    private GameObject InitBaseShape(List<GameObject> NewBornGeneration, int y)
    {
      NewBornGeneration.Add(Instantiate(newborn.CellPrefab));
      GameObject cell = newborn.NewBornGenerations[y][newborn.NewBornGenerations[y].Count - 1];
      cell.transform.position = transform.position;
      return cell;
    }

    private bool CheckIsValid(bool isValid, Vector3 cellPosition)
    {
      foreach (var position in newborn.CellPositions)
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
      cj.angularYLimit = new SoftJointLimit() { limit = AgentConfig.yLimit, bounciness = AgentConfig.bounciness };
      handleAngularLimit(cj, jointAnchor);
      cj.angularZLimit = new SoftJointLimit() { limit = AgentConfig.zLimit, bounciness = AgentConfig.bounciness };
      part.gameObject.GetComponent<Rigidbody>().useGravity = true;
      part.gameObject.GetComponent<Rigidbody>().mass = 1f;
    }

    private void handleAngularLimit(ConfigurableJoint cj, Vector3 jointAnchor)
    {
      if (jointAnchor.y == -1)
      {
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.highLimit, bounciness = AgentConfig.bounciness };
        cj.highAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
      }
      else if (jointAnchor.y == 1)
      {
        cj.highAngularXLimit = new SoftJointLimit() { limit = AgentConfig.highLimit, bounciness = AgentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
      }
      else if (jointAnchor.x == 1)
      {
        cj.highAngularXLimit = new SoftJointLimit() { limit = AgentConfig.highLimit, bounciness = AgentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
        cj.axis = new Vector3(0f, -1f, 0f);
      }
      else if (jointAnchor.x == -1)
      {
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.highLimit, bounciness = AgentConfig.bounciness };
        cj.highAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.highLimit, bounciness = AgentConfig.bounciness };
        cj.axis = new Vector3(0f, -1f, 0f);
      }
      else if (jointAnchor.z == 1)
      {
        cj.axis = new Vector3(0f, -1f, 0f);
        cj.highAngularXLimit = new SoftJointLimit() { limit = AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
      }
      else if (jointAnchor.z == -1)
      {
        cj.axis = new Vector3(-1f, 0f, 0f);
        cj.highAngularXLimit = new SoftJointLimit() { limit = AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
        cj.lowAngularXLimit = new SoftJointLimit() { limit = -AgentConfig.lowLimit, bounciness = AgentConfig.bounciness };
      }
    }

    private void BuildAgentPart(bool init)
    {
      aTBehaviour = transform.gameObject.GetComponent<AgentTrainBehaviour>();
      aTBehaviour.initPart = newborn.Cells[0].transform;
      for (int i = 1; i < cellNb; i++)
      {
        if (aTBehaviour.parts.Count < i)
        {
          aTBehaviour.parts.Add(newborn.Cells[i].transform);
        }
      }
      if (init)
      {
        aTBehaviour.initBodyParts();
      }
    }
  }
}