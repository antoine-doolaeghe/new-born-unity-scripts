using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLAgents;
using UnityEngine;

namespace Newborn
{
  [ExecuteInEditMode]
  public class NewBornBuilder : MonoBehaviour
  {
    [Header("Connection to API Service")]
    public NewbornService newbornService;
    public bool requestApiData;
    public int cellNb = 0;
    public int minCellNb;
    public List<GeneInformation> GeneInformations;
    private int cellInfoIndex = 0;
    private bool Initialised = false;
    private NewbornAgent newborn;
    private NewBornBuilder newBornBuilder;
    private AgentTrainBehaviour aTBehaviour;
    [HideInInspector] public Academy academy;
    [HideInInspector] public float threshold;
    [HideInInspector] public int partNb;

    void Awake()
    {
      newborn = transform.GetComponent<NewbornAgent>();
      newBornBuilder = transform.GetComponent<NewBornBuilder>();
      aTBehaviour = transform.GetComponent<AgentTrainBehaviour>();
      academy = GameObject.Find("Academy").transform.GetComponent<Academy>();
    }
    public void ClearNewborns()
    {
      if (newborn.Cells != null)
      {
        newborn.Cells.Clear();
      }
      if (newborn.NewBornGenerations != null)
      {
        newborn.NewBornGenerations.Clear();
      };
      if (newborn.CellPositions != null)
      {
        newborn.CellPositions.Clear();
      }
      if (newborn.CelllocalPositions != null)
      {
        newborn.CelllocalPositions.Clear();
      }
      if (newborn.GeneInformations != null)
      {
        newborn.GeneInformations.Clear();
      }
      cellInfoIndex = 0;
      Initialised = false;
      // academy.broadcastHub.broadcastingBrains.Clear();
      cellNb = 0;
      aTBehaviour.DeleteBodyParts();
    }

    public void BuildNewBorn(float threshold)
    {
      SetAgentNameFromBrainName();
      InitaliseNewbornInformation();

      if (newborn.GeneInformations.Count == 0)
      {
        newborn.GeneInformations.Add(new GeneInformation(new List<float>()));
      }

      newborn.NewBornGenerations.Add(new List<GameObject>());
      GameObject initCell = InitBaseShape(newborn.NewBornGenerations[0], 0);
      initCell.transform.parent = transform;
      AnatomyHelpers.InitRigidBody(initCell);
      StoreNewbornCell(initCell, initCell.transform.position, initCell.transform.position);
      for (int y = 1; y < AgentConfig.layerNumber; y++)
      {
        int previousGenerationCellNumber = newborn.NewBornGenerations[y - 1].Count;
        newborn.NewBornGenerations.Add(new List<GameObject>());
        for (int i = 0; i < previousGenerationCellNumber; i++)
        {
          for (int z = 0; z < AnatomyHelpers.Sides.Count; z++)
          {
            if (!requestApiData || cellInfoIndex < newborn.GeneInformations[0].info.Count) // solution ???????
            {
              bool IsValidPosition = true;
              float cellInfo = 0f;
              Vector3 cellPosition = newborn.NewBornGenerations[y - 1][i].transform.position + AnatomyHelpers.Sides[z];
              IsValidPosition = AnatomyHelpers.IsValidPosition(newborn, IsValidPosition, cellPosition);
              cellInfo = HandleCellInfos(0, cellInfoIndex);
              cellInfoIndex++;
              if (IsValidPosition)
              {
                if (cellInfo > threshold)
                {
                  BuildCell(y, i, z, cellPosition);
                }
              }
            }
          }
        }
      }


      foreach (var cell in newborn.Cells)
      {
        cell.transform.parent = transform;
        cell.GetComponent<SphereCollider>().radius /= 2f;
      }
      // aTBehaviour.enabled = true;
      cellNb = newborn.Cells.Count;
    }

    public void BuildNewGeneration(int generationInfo, bool isAfterRequest)
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
        newborn.GeneInformations.Add(new GeneInformation(new List<float>()));
      }

      newborn.NewBornGenerations.Add(new List<GameObject>());

      for (int i = 0; i < previousGenerationCellNumber; i++)
      {
        for (int z = 0; z < AnatomyHelpers.Sides.Count; z++)
        {
          bool IsValidPosition = true;
          float cellInfo = 0f;
          Vector3 cellPosition = newborn.NewBornGenerations[germNb][i].transform.position + AnatomyHelpers.Sides[z];
          IsValidPosition = AnatomyHelpers.IsValidPosition(newborn, IsValidPosition, cellPosition);
          cellInfo = HandleCellInfos(newborn.GeneInformations.Count - 1, indexInfo);
          indexInfo++;
          if (IsValidPosition)
          {
            if (cellInfo > threshold)
            {
              GameObject cell = InitBaseShape(newborn.NewBornGenerations[germNb + 1], germNb + 1);
              AnatomyHelpers.InitPosition(AnatomyHelpers.Sides, germNb + 1, i, z, cell, newborn);
              AnatomyHelpers.InitRigidBody(cell);
              AnatomyHelpers.InitJoint(cell, newborn.NewBornGenerations[germNb][i], AnatomyHelpers.Sides[z], germNb + 1, z);
              cell.transform.parent = transform;
              StoreNewbornCell(cell, cellPosition, cellPosition);
            }
          }
        }
      }
      cellNb = newborn.Cells.Count;
      AddBodyPart(false);
    }


    public IEnumerator BuildAgentRandomNewBorn()
    {
      yield return StartCoroutine(checkNewbornGeneration());
      string name = NewbornBrain.GenerateRandomName();
      newborn.GenerationIndex = GenerationService.generations.Count;
      newborn.GenerationId = GenerationService.generations[newborn.GenerationIndex - 1];
      requestApiData = false;
      BuildNewBorn(AgentConfig.threshold);
      SetGameObjectName(name);
      checkMinCellNb();
      AddBodyPart(true);
      NewbornBrain.SetBrainParameters(aTBehaviour, cellNb);
      NewbornBrain.SetBrainName(aTBehaviour, name);
      aTBehaviour.enabled = true;
    }

    public void BuildAgentRandomGeneration(Transform agent)
    {
      newBornBuilder.threshold = AgentConfig.threshold;
      newBornBuilder.BuildNewGeneration(newborn.GeneInformations.Count, false);
      Brain brain = Resources.Load<Brain>("Brains/agentBrain0");
      agent.gameObject.name = brain.name;
      brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      brain.brainParameters.vectorActionSize = new int[1] { newBornBuilder.cellNb * 3 };
      brain.brainParameters.vectorObservationSize = newBornBuilder.cellNb * 13 - 4;
      aTBehaviour.brain = brain;
    }

    public void BuildNewbornFromResponse(GameObject agent, string responseId, List<float> infoResponse)
    {
      Debug.Log("Building Newborn From Fetch Response 🏗️");
      if (partNb == 0 && threshold == 0f)
      {
        partNb = AgentConfig.layerNumber;
        threshold = AgentConfig.threshold;
      }

      requestApiData = true;

      if (infoResponse.Count != 0 && !Initialised)
      {
        newborn.GeneInformations.Add(new GeneInformation(new List<float>()));
        newborn.GeneInformations[0].info = infoResponse;
        BuildNewBorn(threshold);
        SetGameObjectName(responseId);
        checkMinCellNb();
        AddBodyPart(true);
        NewbornBrain.SetBrainParameters(aTBehaviour, cellNb);
        NewbornBrain.SetBrainName(aTBehaviour, responseId);
        Initialised = true;
        aTBehaviour.enabled = true;
      }
    }


    public List<float> ReturnGeneInformations(int modelIndex)
    {
      List<float> ModelInfos = new List<float>();

      for (int i = 0; i < newborn.GeneInformations[modelIndex].info.Count; i++)
      {
        ModelInfos.Add(newborn.GeneInformations[modelIndex].info[i]);
      }

      return ModelInfos;
    }

    public void LoadModelToLearningBrain(string newbornId, MLAgents.InferenceBrain.NNModel model)
    {
      transform.GetComponent<NewbornAgent>().learningBrain.model = model;
      aTBehaviour.brain = transform.GetComponent<NewbornAgent>().learningBrain;
      NewbornBrain.SetBrainParameters(aTBehaviour, cellNb);
      NewbornBrain.SetBrainName(aTBehaviour, newbornId);
      aTBehaviour.enabled = true;
    }

    private void StoreNewbornCell(GameObject cell, Vector3 cellPosition, Vector3 cellLocalPosition)
    {
      newborn.Cells.Add(cell);
      newborn.CellPositions.Add(cellPosition);
      newborn.CelllocalPositions.Add(cellLocalPosition);
    }

    private float HandleCellInfos(int generationIndex, int cellIndex)
    {
      if (requestApiData)
      {
        float cellInfo = newborn.GeneInformations[generationIndex].info[cellIndex];
        return cellInfo;
      }
      else
      {
        float cellInfo = Random.Range(0f, 1f);
        newborn.GeneInformations[generationIndex].info.Add(cellInfo);
        return cellInfo;
      }
    }

    private GameObject InitBaseShape(List<GameObject> NewBornGeneration, int y)
    {
      NewBornGeneration.Add(Instantiate(newborn.CellPrefab));
      GameObject cell = newborn.NewBornGenerations[y][newborn.NewBornGenerations[y].Count - 1];
      cell.transform.position = transform.position;
      return cell;
    }

    private void AddBodyPart(bool init)
    {
      aTBehaviour.initPart = newborn.Cells[0].transform;
      for (int i = 1; i < cellNb; i++)
      {
        if (aTBehaviour.parts.Count < i)
        {
          aTBehaviour.parts.Add(newborn.Cells[i].transform);
        }
      }
    }

    private void InitaliseNewbornInformation()
    {
      newborn.title = transform.gameObject.name;
      newborn.NewBornGenerations = new List<List<GameObject>>();
      newborn.Cells = new List<GameObject>();
      newborn.CellPositions = new List<Vector3>();
      newborn.CelllocalPositions = new List<Vector3>();
    }
    private void BuildCell(int y, int i, int z, Vector3 cellPosition)
    {
      GameObject cell = InitBaseShape(newborn.NewBornGenerations[y], y);
      AnatomyHelpers.InitPosition(AnatomyHelpers.Sides, y, i, z, cell, newborn);
      AnatomyHelpers.InitRigidBody(cell);
      AnatomyHelpers.InitJoint(cell, newborn.NewBornGenerations[y - 1][i], AnatomyHelpers.Sides[z], y, z);
      cell.transform.parent = transform;
      StoreNewbornCell(cell, cellPosition, cellPosition);
    }

    private void SetAgentNameFromBrainName()
    {
      transform.gameObject.name = aTBehaviour.brain.name;
    }

    private void checkMinCellNb()
    {
      if (cellNb < minCellNb)
      {
        Debug.Log("Killing Object (less that requiered size");
        transform.gameObject.SetActive(false);
      }
    }

    private void SetGameObjectName(string name)
    {
      transform.name = name;
    }
    public IEnumerator checkNewbornGeneration()
    {
      yield return StartCoroutine(GenerationService.GetGenerations()); /// This check should be made as you build the AGENT and not as you post the agents.
      if (GenerationService.generations.Count == 0)
      {
        yield return StartCoroutine(GenerationService.PostGeneration(NewbornBrain.GenerateRandomName(), 1));
      }
    }

    public IEnumerator PostNewborn(NewBornPostData newBornPostData, GameObject agent)
    {
      yield return StartCoroutine(NewbornService.PostNewborn(newBornPostData, agent));
    }

    public IEnumerator PostNewbornModel(string newbornId, int modelIndex, GameObject agent, NewbornService.PostModelCallback responseCallback)
    {
      List<float> modelInfos = ReturnGeneInformations(modelIndex);
      List<PositionPostData> cellPositions = AnatomyHelpers.ReturnModelPositions(newborn);
      string id = NewbornBrain.GenerateRandomName();
      GenerationPostData generationPostData = new GenerationPostData(newbornId, cellPositions, modelInfos);
      yield return NewbornService.PostNewbornModel(transform, generationPostData, newbornId, agent, responseCallback);
    }
  }
}