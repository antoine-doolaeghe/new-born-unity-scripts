using System.Collections;
using System.Collections.Generic;
using MLAgents;
using SimpleJSON;
using UnityEngine;

namespace Newborn
{
  [ExecuteInEditMode]
  public class AnatomyBuilder : MonoBehaviour
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
    private AnatomyBuilder anatomyBuilder;
    private AgentTrainBehaviour aTBehaviour;
    private AnatomyRender renderer;
    [HideInInspector] public Academy academy;
    [HideInInspector] public float threshold;
    [HideInInspector] public int partNb;

    void Awake()
    {
      Debug.Log("AT BEHAG AWAKE");
      newborn = transform.GetComponent<NewbornAgent>();
      anatomyBuilder = transform.GetComponent<AnatomyBuilder>();
      aTBehaviour = transform.GetComponent<AgentTrainBehaviour>();
      academy = GameObject.Find("Academy").transform.GetComponent<Academy>();
      renderer = GetComponent<AnatomyRender>();
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
      renderer.ResetMesh();
      cellInfoIndex = 0;
      Initialised = false;
      cellNb = 0;
      aTBehaviour.DeleteBodyParts();
    }

    public void BuildNewBorn(float threshold)
    {
      InitaliseNewbornInformation();

      if (newborn.GeneInformations.Count == 0)
      {
        newborn.GeneInformations.Add(new GeneInformation(new List<float>()));
      }

      BuildInitCell();

      for (int y = 1; y < AgentConfig.layerNumber; y++)
      {
        int generationCount = newborn.NewBornGenerations[y - 1].Count;
        newborn.NewBornGenerations.Add(new List<AnatomyCell>());
        for (int i = 0; i < generationCount; i++)
        {
          for (int z = 0; z < AnatomyHelpers.Sides.Count; z++)
          {
            if (!requestApiData || cellInfoIndex < newborn.GeneInformations[0].info.Count) // solution ???????
            {
              GameObject previousCell = newborn.NewBornGenerations[y - 1][i].mesh;
              Vector3 newCellPosition = previousCell.transform.localPosition + AnatomyHelpers.Sides[z];
              if (AnatomyHelpers.IsValidPosition(newborn, newCellPosition))
              {
                if (ReturnCurrentInfo(0, cellInfoIndex) > threshold)
                {
                  BuildCell(y, i, z, newCellPosition);
                }
              }
              cellInfoIndex++;
            }
          }
        }
      }

      renderer.UpdateMesh();
      renderer.AssignBone();

      foreach (var cell in newborn.Cells)
      {
        cell.transform.parent = transform;
        cell.GetComponent<SphereCollider>().radius = 0.10f;
      }

      cellNb = newborn.Cells.Count;
    }


    // NOT IN USE
    public void AddNewbornGeneration(int generationInfo)
    {
      int indexInfo = 0;
      int previousGenerationCellNumber = 0;
      int generationCount = 0;
      partNb += 1;

      CheckPreviousGeneration(ref previousGenerationCellNumber, ref generationCount);

      newborn.NewBornGenerations.Add(new List<AnatomyCell>());

      for (int i = 0; i < previousGenerationCellNumber; i++)
      {
        for (int z = 0; z < AnatomyHelpers.Sides.Count; z++)
        {
          GameObject previousCell = newborn.NewBornGenerations[generationCount][i].mesh;
          Vector3 cellPosition = previousCell.transform.position + AnatomyHelpers.Sides[z];
          indexInfo++;
          if (AnatomyHelpers.IsValidPosition(newborn, cellPosition))
          {
            if (ReturnCurrentInfo(newborn.GeneInformations.Count - 1, indexInfo) > threshold)
            {
              GameObject cell = InitBaseShape(cellPosition, newborn.NewBornGenerations[generationCount + 1], generationCount + 1);
              AnatomyHelpers.InitPosition(AnatomyHelpers.Sides, generationCount + 1, i, z, cell, newborn);
              AnatomyHelpers.InitRigidBody(cell);
              AnatomyHelpers.InitJoint(cell, newborn.NewBornGenerations[generationCount][i].mesh, AnatomyHelpers.Sides[z]);
              cell.transform.parent = transform;
              StoreNewbornCell(cell, cellPosition, cellPosition);
            }
          }
        }
      }
      renderer.UpdateMesh();
      renderer.AssignBone();
      cellNb = newborn.Cells.Count;
      AddBodyPart(false);
    }

    private void CheckPreviousGeneration(ref int previousGenerationCellNumber, ref int generationCount)
    {
      for (int i = 0; i < newborn.NewBornGenerations.Count; i++)
      {
        if (newborn.NewBornGenerations[i].Count > 0)
        {
          previousGenerationCellNumber = newborn.NewBornGenerations[i].Count;
          generationCount = i;
        }
        else
        {
          newborn.NewBornGenerations.RemoveAt(i);
        }
      }
    }

    public void BuildAgentRandomNewBornCoroutine()
    {
      StartCoroutine(BuildAgentRandomNewBorn());
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
      anatomyBuilder.threshold = AgentConfig.threshold;
      anatomyBuilder.AddNewbornGeneration(newborn.GeneInformations.Count);
      Brain brain = Resources.Load<Brain>("Brains/agentBrain0");
      agent.gameObject.name = brain.name;
      brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      brain.brainParameters.vectorActionSize = new int[1] { anatomyBuilder.cellNb * 3 };
      brain.brainParameters.vectorObservationSize = anatomyBuilder.cellNb * 13 - 4;
      aTBehaviour.brain = brain;
    }

    public void BuildNewbornFromResponse(GameObject agent, string responseId)
    {
      Debug.Log("Building Newborn From Fetch Response 🏗️");
      if (partNb == 0 && threshold == 0f)
      {
        partNb = AgentConfig.layerNumber;
        threshold = AgentConfig.threshold;
      }

      requestApiData = true;

      if (!Initialised)
      {
        SetGameObjectName(responseId);
        BuildNewBorn(threshold);
        checkMinCellNb();
        AddBodyPart(true);
        NewbornBrain.SetBrainParameters(aTBehaviour, cellNb);
        NewbornBrain.SetBrainName(aTBehaviour, responseId);
        Initialised = true;
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
      aTBehaviour.brain = Instantiate(transform.GetComponent<NewbornAgent>().learningBrain);
      NewbornBrain.SetBrainParameters(aTBehaviour, cellNb);
      NewbornBrain.SetBrainName(aTBehaviour, newbornId);
      academy.broadcastHub.broadcastingBrains.Add(aTBehaviour.brain);
      aTBehaviour.enabled = true;
    }

    private void StoreNewbornCell(GameObject cell, Vector3 cellPosition, Vector3 cellLocalPosition)
    {
      newborn.Cells.Add(cell);
      newborn.CellPositions.Add(cellPosition);
      newborn.CelllocalPositions.Add(cellLocalPosition);
    }

    private float ReturnCurrentInfo(int generationIndex, int cellIndex)
    {
      if (requestApiData)
      {
        Debug.Log(newborn.GeneInformations[generationIndex].info.Count);
        Debug.Log(cellIndex);
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

    private GameObject InitBaseShape(Vector3 position, List<AnatomyCell> NewBornGeneration, int y)
    {
      AnatomyCell anatomyCell = new AnatomyCell();
      anatomyCell.mesh = renderer.MakeCube(0.1f, position);
      anatomyCell.mesh.AddComponent<SphereCollider>();
      anatomyCell.mesh.AddComponent<Rigidbody>();
      NewBornGeneration.Add(anatomyCell);
      GameObject cell = newborn.NewBornGenerations[y][newborn.NewBornGenerations[y].Count - 1].mesh;
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
      if (newborn.title == "")
      {
        SetAgentNameFromBrainName();
      }
      newborn.NewBornGenerations = new List<List<AnatomyCell>>() { new List<AnatomyCell>() };
      newborn.Cells = new List<GameObject>();
      newborn.CellPositions = new List<Vector3>();
      newborn.CelllocalPositions = new List<Vector3>();
    }
    private void BuildInitCell()
    {
      GameObject initCell = InitBaseShape(new Vector3(0f, 0f, 0f), newborn.NewBornGenerations[0], 0);
      AnatomyHelpers.InitRigidBody(initCell);
      AnatomyHelpers.InitJoint(initCell, transform.gameObject, transform.position, true);
      initCell.transform.parent = transform;
      StoreNewbornCell(initCell, initCell.transform.position, initCell.transform.position);
    }

    private void BuildCell(int y, int i, int z, Vector3 cellPosition)
    {
      GameObject cell = InitBaseShape(cellPosition, newborn.NewBornGenerations[y], y);
      AnatomyHelpers.InitRigidBody(cell);
      AnatomyHelpers.InitJoint(cell, newborn.NewBornGenerations[y - 1][i].mesh, AnatomyHelpers.Sides[z]);
      cell.transform.parent = transform;
      StoreNewbornCell(cell, cellPosition, cellPosition);
    }

    private void SetAgentNameFromBrainName()
    {
      newborn.title = aTBehaviour.brain.name;
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