using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;
using Service.Newborn;
using Service.Generation;
using Components.Generation;
using Components.Newborn.Util;
using Components.Newborn.Gene;

namespace Components.Newborn.Anatomy
{
  [ExecuteInEditMode]
  public class AnatomyBuilder : MonoBehaviour
  {
    public JointConfig JointConfig;
    public AgentConfig AgentConfig;
    public bool HasApiConnection;
    private int CellCount = 0;
    private bool Initialised = false;
    private NewbornAgent newborn;
    private AnatomyBuilder anatomyBuilder;
    private AgentTrainBehaviour aTBehaviour;
    [HideInInspector] public Academy academy;
    [HideInInspector] public int PartCount;

    void Awake()
    {
      newborn = transform.GetComponent<NewbornAgent>();
      anatomyBuilder = transform.GetComponent<AnatomyBuilder>();
      aTBehaviour = transform.GetComponent<AgentTrainBehaviour>();
      academy = FindObjectOfType<Academy>();
    }

    public void ResetBuilder()
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
      if (newborn.CellTypes != null)
      {
        newborn.CellTypes.Clear();
      }
      if (newborn.GeneInformations != null)
      {
        newborn.GeneInformations.Clear();
      }

      Initialised = false;
      CellCount = 0;
      aTBehaviour.DeleteBodyParts();
    }

    public void BuildNewBorn()
    {
      int InfoIndex = 0;
      int PathIndex = 0;

      InitaliseNewbornInformation();

      if (newborn.GeneInformations.Count == 0)
      {
        newborn.GeneInformations.Add(new GeneInformation(new List<float>(), new List<string>()));
      }
      string initPath = ReturnCurrentPath(0, PathIndex);
      BuildInitArm(initPath);
      PathIndex++;

      for (int y = 1; y < AgentConfig.LayerNumber; y++)
      {
        int generationCount = newborn.NewBornGenerations[y - 1].Count;
        newborn.NewBornGenerations.Add(new List<GameObject>());
        for (int i = 0; i < generationCount; i++)
        {
          for (int z = 0; z < AnatomyHelpers.Rotations.Count; z++)
          {
            if (!HasApiConnection || InfoIndex < newborn.GeneInformations[0].info.Count) // solution ???????
            {
              GameObject previousCell = newborn.NewBornGenerations[y - 1][i];
              Vector3 newCellPosition = previousCell.transform.GetChild(0).position;
              Vector3 newCellRotation = AnatomyHelpers.Rotations[z];
              if (AnatomyHelpers.IsValidPosition(newCellPosition, AgentConfig.LimbSpacing))
              {
                if (ReturnCurrentInfo(0, InfoIndex) > JointConfig.threshold)
                {
                  string path = ReturnCurrentPath(0, PathIndex);
                  BuildArm(y, i, z, newCellPosition, newCellRotation, path);
                  PathIndex++;
                }
                InfoIndex++;
              }
            }
          }
        }
      }

      foreach (var cell in newborn.Cells)
      {
        cell.transform.parent = transform;
      }

      CellCount = newborn.Cells.Count;
    }


    // NOT IN USE
    public void AddNewbornGeneration(int generationInfo)
    {
      int indexInfo = 0;
      int previousGenerationCellNumber = 0;
      int generationCount = 0;
      PartCount += 1;

      CheckPreviousGeneration(ref previousGenerationCellNumber, ref generationCount);

      newborn.NewBornGenerations.Add(new List<GameObject>());

      for (int i = 0; i < previousGenerationCellNumber; i++)
      {
        for (int z = 0; z < AnatomyHelpers.Sides.Count; z++)
        {
          GameObject previousCell = newborn.NewBornGenerations[generationCount][i];
          Vector3 cellPosition = previousCell.transform.localPosition + AnatomyHelpers.Sides[z];
          indexInfo++;
          if (AnatomyHelpers.IsValidPosition(cellPosition, AgentConfig.LimbSpacing))
          {
            if (ReturnCurrentInfo(newborn.GeneInformations.Count - 1, indexInfo) > JointConfig.threshold)
            {
              string path = AnatomyHelpers.ReturnLimbPrefabPath();
              GameObject cell = InitBase(cellPosition, new Vector3(0f, 0f, 0f), newborn.NewBornGenerations[generationCount + 1], path);
              AnatomyHelpers.InitPosition(AnatomyHelpers.Sides, generationCount + 1, i, z, cell, newborn);
              AnatomyHelpers.InitRigidBody(cell);
              AnatomyHelpers.InitJoint(cell, newborn.NewBornGenerations[generationCount][i], AnatomyHelpers.Sides[z], JointConfig);
              cell.transform.parent = transform;
              StoreNewbornCell(cell, cellPosition, cellPosition, path);
            }
          }
        }
      }
      CellCount = newborn.Cells.Count;
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
      HasApiConnection = false;
      BuildNewBorn();
      SetGameObjectName(name);
      AddBodyPart(true);
      NewbornBrain.SetBrainParameters(aTBehaviour, CellCount);
      NewbornBrain.SetBrainName(aTBehaviour, name);
      aTBehaviour.enabled = true;
    }

    public void BuildAgentRandomGeneration(Transform agent)
    {
      anatomyBuilder.AddNewbornGeneration(newborn.GeneInformations.Count);
      Brain brain = Resources.Load<Brain>("Brains/agentBrain0");
      agent.gameObject.name = brain.name;
      brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      brain.brainParameters.vectorActionSize = new int[1] { anatomyBuilder.CellCount * 3 };
      brain.brainParameters.vectorObservationSize = anatomyBuilder.CellCount * 13 - 4;
      aTBehaviour.brain = brain;
    }

    public void BuildNewbornFromResponse(GameObject agent, string responseId)
    {
      Debug.Log("Building Newborn From Fetch Response 🏗️");
      if (PartCount == 0)
      {
        PartCount = AgentConfig.LayerNumber;
      }

      HasApiConnection = true;

      if (!Initialised)
      {
        SetGameObjectName(responseId);
        BuildNewBorn();
        AddBodyPart(true);
        NewbornBrain.SetBrainParameters(aTBehaviour, CellCount);
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
      NewbornBrain.SetBrainParameters(aTBehaviour, CellCount);
      NewbornBrain.SetBrainName(aTBehaviour, newbornId);
      academy.broadcastHub.broadcastingBrains.Add(aTBehaviour.brain);
      aTBehaviour.enabled = true;
    }

    #region private methods
    private void StoreNewbornCell(GameObject cell, Vector3 cellPosition, Vector3 cellLocalPosition, string type)
    {
      newborn.Cells.Add(cell);
      newborn.CellPositions.Add(cellPosition);
      newborn.CelllocalPositions.Add(cellLocalPosition);
      newborn.CellTypes.Add(type);
    }


    // TODO Move to helper ? 
    private float ReturnCurrentInfo(int generationIndex, int cellIndex)
    {
      if (HasApiConnection)
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

    private string ReturnCurrentPath(int generationIndex, int cellIndex)
    {
      if (HasApiConnection)
      {
        string path = newborn.GeneInformations[generationIndex].path[cellIndex];
        return path;
      }
      else
      {
        string path = AnatomyHelpers.ReturnLimbPrefabPath();
        newborn.GeneInformations[generationIndex].path.Add(path);
        return path;
      }
    }

    private GameObject InitBase(Vector3 position, Vector3 rotation, List<GameObject> NewBornGeneration, string path)
    {
      GameObject Base = SpawnBase(position, rotation, path);
      SpawnJoint(Base);
      NewBornGeneration.Add(Base);
      return Base;
    }

    private GameObject SpawnBase(Vector3 position, Vector3 rotation, string path)
    {
      GameObject BaseShape = Instantiate(Resources.Load<GameObject>(path));
      BaseShape.transform.parent = transform;
      BaseShape.transform.position = position;
      BaseShape.transform.eulerAngles = rotation;
      return BaseShape;
    }

    private void SpawnJoint(GameObject Base)
    {
      GameObject Joint = Instantiate(Resources.Load<GameObject>("Prefabs/Anatomy/joints/prefab"));
      Joint.transform.parent = Base.transform;
      Joint.transform.localPosition = Base.transform.Find("Joint").localPosition;
      Joint.transform.eulerAngles = Base.transform.eulerAngles;
    }

    private void AddBodyPart(bool init)
    {
      aTBehaviour.initPart = newborn.Cells[0].transform;
      for (int i = 1; i < CellCount; i++)
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
      newborn.NewBornGenerations = new List<List<GameObject>>() { new List<GameObject>() };
      newborn.Cells = new List<GameObject>();
      newborn.CellPositions = new List<Vector3>();
      newborn.CelllocalPositions = new List<Vector3>();
      newborn.CellTypes = new List<string>();
    }

    private void BuildInitArm(string path)
    {
      GameObject initCell = InitBase(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), newborn.NewBornGenerations[0], path);
      AnatomyHelpers.InitRigidBody(initCell);
      initCell.transform.parent = transform;
      initCell.transform.localPosition = new Vector3(0f, 0f, 0f);
      StoreNewbornCell(initCell, initCell.transform.position, initCell.transform.position, path);
    }

    private void BuildArm(int y, int i, int z, Vector3 cellPosition, Vector3 cellRotation, string path)
    {
      GameObject arm = InitBase(cellPosition, cellRotation, newborn.NewBornGenerations[y], path);
      Vector3 anchor = arm.transform.Find("Anchor").transform.localPosition;
      AnatomyHelpers.InitRigidBody(arm);
      AnatomyHelpers.InitJoint(arm, newborn.NewBornGenerations[y - 1][i], anchor, JointConfig);
      StoreNewbornCell(arm, cellPosition, cellPosition, path);
    }



    private void SetAgentNameFromBrainName()
    {
      newborn.title = aTBehaviour.brain.name;
      transform.gameObject.name = aTBehaviour.brain.name;
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
      GenerationPostData generationPostData = new GenerationPostData(newbornId, cellPositions, modelInfos, newborn.CellTypes);
      yield return NewbornService.PostNewbornModel(transform, generationPostData, newbornId, agent, responseCallback);
    }
    #endregion
  }
}