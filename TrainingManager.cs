using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MLAgents;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gene {
  public class TrainingManager : MonoBehaviour {
    [Header ("Environment Mode")]
    public bool isTrainingMode;

    [Header ("Environment parameters")]
    public int spawnerNumber;
    public int agentNumber;
    public GameObject Camera;
    public GameObject TrainingPrefab;
    [ConditionalField ("isTrainingMode")] public Vector3 agentScale;
    [ConditionalField ("isTrainingMode")] public Vector3 groundScale;
    [ConditionalField ("isTrainingMode")] public float floorHeight;
    [Header ("Agent parameters")]
    public GameObject AgentPrefab;
    public GameObject CellPrefab;
    public int minCellNb;
    public bool requestApiData;
    public string newbornId;
    public AgentConfig agentConfig;
    public float randomPositionIndex;
    [Header ("Target parameters")]
    [ConditionalField ("isTrainingMode")] public GameObject StaticTarget;
    [ConditionalField ("isTrainingMode")] public bool isTargetDynamic;
    [ConditionalField ("isTrainingMode")] public Vector3 targetPosition;
    [Header ("Academy parameters")]
    public Academy academy;
    public bool control;
    [Header ("Brain parameters")]
    public Brain brainObject;
    public int vectorObservationSize;
    public int vectorActionSize;
    public TextAsset brainModel;
    [Header ("Camera parameters")]

    [HideInInspector] public List<GameObject> Agents = new List<GameObject> ();

    public void Delete () {
      Transform[] childs = transform.Cast<Transform> ().ToArray ();
      foreach (Transform child in childs) {
        DestroyImmediate (child.gameObject);
      }
      Agents.Clear ();
      academy.broadcastHub.broadcastingBrains.Clear ();
    }

    public void BuildSpawners () {
      GameObject parent = transform.gameObject;
      int floor = 0;
      int squarePosition = 0;

      for (var i = 0; i < spawnerNumber; i++) {
        List<GameObject> newBornAgents = new List<GameObject> ();
        GameObject spawner;
        Brain brain = Instantiate (brainObject);

        if (isTrainingMode && i % 4 == 0) {
          parent = new GameObject ();
          NameFloor (parent, floor);
          floor++;
          squarePosition = 0;
          parent = CreateTrainingFloor (floor);
        }

        InstantiateSpawner (parent, floor, squarePosition, out spawner);

        if (isTrainingMode) {
          PositionTrainingSpawner (squarePosition, spawner);
        } else {
          // Randomly place the agents.
        }

        SetBrainParams (brain, Regex.Replace (System.Guid.NewGuid ().ToString (), @"[^0-9]", ""));

        if (!isTargetDynamic && isTrainingMode) {
          Instantiate (StaticTarget, spawner.transform);
        }

        for (int y = 0; y < agentNumber; y++) {
          AgentTrainBehaviour atBehaviour;
          NewBornBuilder newBornBuilder;
          GameObject newBornAgent;
          newBornAgents.Add (BuildAgent (spawner, out newBornAgent, out atBehaviour, out newBornBuilder));
          AddBrainToAgentBehaviour (atBehaviour, brain);
          SetApiRequestParameter (newBornBuilder, atBehaviour, requestApiData);
          AddMinCellNb (newBornBuilder, minCellNb);
        }

        AssignTarget (newBornAgents);

        squarePosition++;
      }
    }

    private void AssignTarget (List<GameObject> newBornAgents) {
      for (int y = 0; y < newBornAgents.Count; y++) {
        if (isTargetDynamic) {
          if (y != newBornAgents.Count - 1) {
            newBornAgents[y].GetComponent<AgentTrainBehaviour> ().target = newBornAgents[y + 1].transform;
          } else {
            newBornAgents[y].GetComponent<AgentTrainBehaviour> ().target = newBornAgents[0].transform;
          }
        } else {
          newBornAgents[y].GetComponent<AgentTrainBehaviour> ().target = StaticTarget.transform;
        }
      }
    }

    public void BuildNewBornFromFetch (bool buildFromPost, string responseId, int agentId = 0) {
      Debug.Log ("Building Newborn From Fetch");
      Transform agent = Agents[agentId].transform;
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour> ();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder> ();
      NewbornService newbornService = agent.GetComponent<NewbornService> ();

      if (newBornBuilder.partNb == 0 && newBornBuilder.threshold == 0f) {
        newBornBuilder.partNb = agentConfig.layerNumber;
        newBornBuilder.threshold = agentConfig.threshold;
      }

      newBornBuilder.requestApiData = true;
      newBornBuilder.handleCellInfoResponse ();

      if (buildFromPost) {
        setBrainParameters (atBehaviour, newBornBuilder);
        setBrainName (atBehaviour, responseId);
      } else if (agentId == 0) // INIT FIRST BRAIN
      {
        ClearBroadCastingBrains (academy);
        setBrainParameters (atBehaviour, newBornBuilder);
        setBrainName (atBehaviour, responseId);
        academy.broadcastHub.broadcastingBrains.Add (atBehaviour.brain);
      } else // ASSIGN ALL TO THE SAME BRAIN
      {
        atBehaviour.brain = Agents[0].transform.GetComponent<AgentTrainBehaviour> ().brain;
      }
    }

    public void BuildRandomTrainingNewBorn (bool buildFromPost, int agentId = 0) {
      Transform agent = Agents[agentId].transform;
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour> ();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder> ();
      NewbornService newbornService = agent.GetComponent<NewbornService> ();

      newBornBuilder.requestApiData = false;
      newBornBuilder.initNewBorn (agentConfig.layerNumber, agentConfig.threshold);
      setBrainParameters (atBehaviour, newBornBuilder);
    }

    public void BuildRandomProductionNewBorn (Transform agent) {
      // Handle starting/communication with api data
      AgentTrainBehaviour atBehaviour = agent.GetComponent<AgentTrainBehaviour> ();
      NewBornBuilder newBornBuilder = agent.GetComponent<NewBornBuilder> ();
      NewbornService newbornService = agent.GetComponent<NewbornService> ();

      newBornBuilder.requestApiData = false;
      newBornBuilder.initNewBorn (agentConfig.layerNumber, agentConfig.threshold);
      setBrainParameters (atBehaviour, newBornBuilder);
    }

    public void BuildRandomGeneration () {
      academy.broadcastHub.broadcastingBrains.Clear ();
      for (int a = 0; a < Agents.Count; a++) {

        NewBornBuilder newBornBuilder = Agents[a].transform.GetComponent<NewBornBuilder> ();
        AgentTrainBehaviour atBehaviour = Agents[a].transform.GetComponent<AgentTrainBehaviour> ();
        newBornBuilder.threshold = agentConfig.threshold;
        SetApiRequestParameter (newBornBuilder, atBehaviour, false);
        newBornBuilder.BuildGeneration (newBornBuilder.ModelInfosList.Count, false);
        Brain brain = Resources.Load<Brain> ("Brains/agentBrain" + a);
        SetBrainParams (brain, brain.name);
        Agents[a].gameObject.name = brain + "";
        brain.brainParameters.vectorObservationSize = vectorObservationSize;
        brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
        brain.brainParameters.vectorActionSize = new int[1] { Agents[a].transform.GetComponent<NewBornBuilder> ().cellNb * 3 };
        brain.brainParameters.vectorObservationSize = Agents[a].transform.GetComponent<NewBornBuilder> ().cellNb * 13 - 4;
        atBehaviour.brain = brain;
      }
    }

    public IEnumerator PostNewborns () {
      Debug.Log ("Posting training NewBorns to the server...");
      // Check if it has a generation
      GenerationService generationService = transform.GetComponent<GenerationService> ();

      yield return StartCoroutine (RequestGenerations ());
      if (generationService.generations.Count == 0) {
        Debug.Log ("Posting generation");
        yield return StartCoroutine (PostGeneration ());
      }
      Debug.Log ("Posted Generation");
      string generationId = generationService.generations[0];

      for (int agent = 0; agent < Agents.Count; agent++) {
        NewBornBuilder newBornBuilder = Agents[agent].transform.GetComponent<NewBornBuilder> ();
        AgentTrainBehaviour agentTrainBehaviour = Agents[agent].transform.GetComponent<AgentTrainBehaviour> ();
        string newbornId = agentTrainBehaviour.brain.name;
        newBornBuilder.HandlePostNewborn (generationId, newbornId, agent);
      }
    }

    public void PostTrainingNewborns () {
      StartCoroutine (PostNewborns ());
    }

    public IEnumerator PostGeneration () {
      GenerationService generationService = transform.GetComponent<GenerationService> ();
      yield return StartCoroutine (generationService.PostGeneration (Regex.Replace (System.Guid.NewGuid ().ToString (), @"[^0-9]", "")));
      Debug.Log (generationService.generations[0]);
    }

    public IEnumerator RequestGenerations () {
      GenerationService generationService = transform.GetComponent<GenerationService> ();
      Debug.Log ("Request Agent info from server...");
      yield return StartCoroutine (generationService.GetGenerations ());
    }

    public IEnumerator RequestNewbornAgentInfo () {
      Debug.Log ("Request Agent info from server...");
      for (int a = 0; a < Agents.Count; a++) {
        NewbornService newbornService = Agents[a].transform.GetComponent<NewbornService> ();
        yield return StartCoroutine (newbornService.GetNewborn (newbornId, a, false));
      }
      Debug.Log ("Finished to build Agents");
      academy.InitializeEnvironment ();
      academy.initialized = true;
    }

    public void RequestNewborn () {
      StartCoroutine (RequestNewbornAgentInfo ());
    }

    public void RequestProductionAgentInfo () {
      foreach (GameObject agent in GameObject.FindGameObjectsWithTag ("agent")) {
        Debug.Log (agent.name);
      }
    }

    public void DeleteCell () {
      foreach (GameObject agent in Agents) {
        agent.SetActive (true);
        agent.GetComponent<NewBornBuilder> ().DeleteCells ();
        agent.GetComponent<AgentTrainBehaviour> ().DeleteBodyParts ();
        Transform[] childs = agent.transform.Cast<Transform> ().ToArray ();
        foreach (Transform child in childs) {
          DestroyImmediate (child.gameObject);
        }
      }
    }

    private void SetBrainParams (Brain brain, string brainName) {
      brain.name = brainName;
      brain.brainParameters.vectorActionSize = new int[1] { vectorActionSize };
      academy.broadcastHub.broadcastingBrains.Add (brain);
      academy.broadcastHub.SetControlled (brain, control);
    }

    private GameObject CreateTrainingFloor (int floor) {
      GameObject trainingFloor = new GameObject ();
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
      trainingFloor.transform.localPosition = new Vector3 (0f, floorHeight * floor, 0f);
      return trainingFloor;
    }

    private static void PositionTrainingSpawner (int squarePosition, GameObject spawner) {
      Transform spawnerTransform = spawner.transform;
      Vector3 spawnerTransformGroundScale = spawnerTransform.Find ("Ground").transform.localScale;
      switch (squarePosition) {
        case 0:
          spawnerTransform.localPosition = new Vector3 (0f, 0f, 0f);
          break;
        case 1:
          spawnerTransform.localPosition = new Vector3 (spawnerTransformGroundScale.x, 0f, 0f);
          break;
        case 2:
          spawnerTransform.localPosition = new Vector3 (0f, 0f, spawnerTransformGroundScale.z);
          break;
        case 3:
          spawnerTransform.localPosition = new Vector3 (spawnerTransformGroundScale.x, 0f, spawnerTransformGroundScale.z);
          break;
      }
    }

    private static void PositionProductionSpawner () {
      Debug.Log ("TO-DO: Position the production spawner");
    }

    private void InstantiateSpawner (GameObject parent, int floor, int squarePosition, out GameObject spawner) {
      spawner = Instantiate (TrainingPrefab, parent.transform);
      spawner.name = !isTrainingMode ? ("Spawner") : ("Trainer" + floor + "." + squarePosition);
      spawner.transform.localScale = groundScale;
    }

    private GameObject BuildAgent (GameObject spawner, out GameObject newBornAgent, out AgentTrainBehaviour atBehaviour, out NewBornBuilder newBornBuilder) {
      newBornAgent = Instantiate (AgentPrefab, spawner.transform);
      atBehaviour = newBornAgent.transform.GetComponent<AgentTrainBehaviour> ();
      newBornBuilder = newBornAgent.transform.GetComponent<NewBornBuilder> ();
      newBornBuilder.CellPrefab = CellPrefab;
      Agents.Add (newBornAgent);
      newBornAgent.transform.localPosition = new Vector3 (Random.Range (-randomPositionIndex, randomPositionIndex), 0f, Random.Range (-randomPositionIndex, randomPositionIndex));
      return newBornAgent;
    }

    private void NameFloor (GameObject trainingFloor, int floor) {
      trainingFloor.name = "Floor" + floor;
      trainingFloor.transform.parent = transform;
    }

    private void AddMinCellNb (NewBornBuilder newBornBuilder, int minCellNb) {
      newBornBuilder.minCellNb = minCellNb;
    }

    private void SetApiRequestParameter (NewBornBuilder newBornBuilder, AgentTrainBehaviour atBehaviour, bool requestApiData) {
      atBehaviour.requestApiData = requestApiData;
      newBornBuilder.requestApiData = requestApiData;
    }

    private void AddBrainToAgentBehaviour (AgentTrainBehaviour atBehaviour, Brain brain) {
      atBehaviour.brain = brain;
    }

    private void ClearBroadCastingBrains (Academy academy) {
      academy.broadcastHub.broadcastingBrains.Clear ();
    }

    private void setBrainParameters (AgentTrainBehaviour atBehaviour, NewBornBuilder newBornBuilder) {
      atBehaviour.brain.brainParameters.vectorObservationSize = vectorObservationSize;
      atBehaviour.brain.brainParameters.vectorActionSpaceType = SpaceType.continuous;
      atBehaviour.brain.brainParameters.vectorActionSize = new int[1] { newBornBuilder.cellNb * 3 };
      atBehaviour.brain.brainParameters.vectorObservationSize = newBornBuilder.cellNb * 13 - 4;
    }

    private void setBrainName (AgentTrainBehaviour atBehaviour, string responseId) {
      atBehaviour.brain.name = responseId;
    }
  }
}