using UnityEngine;
using System.Collections;
using Newborn;
using System.Collections.Generic;

public class TargetController : MonoBehaviour
{
  public Transform target;
  public Transform ground;
  private float timer = 0.0f;
  public static float trainingStage = 2;
  public float maximumStaticTargetDistance;
  public float foodSpawnRadius;
  public float foodSpawnRadiusIncrementor;
  public float minimumTargetDistance;
  private int timePenaltyMultiplier = 0;
  [HideInInspector] public NewbornSpawner spawner;
  [HideInInspector] public AgentTrainBehaviour agentTrainBehaviour;

  void Update()
  {
    timer += Time.deltaTime;
    if (timer > 10f)
    {
      GetClosestEnemy(GameObject.FindGameObjectsWithTag("agent"));
      timer = 0.0f;
    }
  }

  public void TouchedNewborn(GameObject touchingNewborn)
  {
    agentTrainBehaviour.AddReward(15f);
    StartCoroutine(handleTouchedNewborn(touchingNewborn));
  }

  public IEnumerator handleTouchedNewborn(GameObject touchingNewborn)
  {
    NewbornAgent newborn = transform.gameObject.GetComponent<NewbornAgent>();
    NewBornBuilder newBornBuilder = transform.gameObject.GetComponent<NewBornBuilder>();
    // CREATE A COROUTINE HERE 
    string sex = newborn.Sex;
    int generationIndex = newborn.GenerationIndex;
    string partnerSex = touchingNewborn.GetComponent<NewbornAgent>().Sex;
    int partnerGenerationIndex = touchingNewborn.GetComponent<NewbornAgent>().GenerationIndex;

    if (sex == "female" && partnerSex == "male" && generationIndex == partnerGenerationIndex) // Generation must be equal ? 
    {
      Debug.Log("Compatible partner");
      newborn.SetNewbornInGestation();
      List<GeneInformation> femaleGene = newborn.GeneInformations;
      List<GeneInformation> maleGene = touchingNewborn.GetComponent<NewbornAgent>().GeneInformations;
      List<GeneInformation> newGene = GeneHelper.ReturnMixedForReproduction(femaleGene, maleGene);
      // Prepare post data here
      string newNewbornName = "name";
      string newNewbornGenerationId = newborn.GenerationId;
      string newNewbornSex = "male";
      string newNewbornHex = "MOCK HEX";

      NewBornPostData newBornPostData = new NewBornPostData(newNewbornName, NewbornBrain.GenerateRandomName(), newNewbornGenerationId, newNewbornSex, newNewbornHex);
      NewbornService.PostNewbornFromReproductionCallback PostNewbornFromReproductionCallback = NewbornService.SuccessfullPostNewbornFromReproductionCallback;
      yield return NewbornService.PostNewbornFromReproduction(newBornPostData, transform.gameObject, touchingNewborn, PostNewbornFromReproductionCallback);
    }
  }

  public void TouchedFood()
  {
    Debug.Log("🍏🍏 TOUCHED FOOD 🍏🍏");
    agentTrainBehaviour.AddReward(15f);
    spawner.resetTrainingAgents();
    handleFoodSpawn();
  }

  private void handleFoodSpawn()
  {
    if (TargetController.trainingStage == 0)
    {
      if (target.transform.localPosition.x <= -maximumStaticTargetDistance)
      {
        Debug.Log("MOVING TARGET BACKWARD ⏮️" + target.transform.localPosition.x);
        TargetController.trainingStage = 1;
        StartCoroutine(TrainingService.UpdateTrainingStage(agentTrainBehaviour.brain.name, "1"));
        target.transform.localPosition = new Vector3(35f, target.transform.localPosition.y, 35f);
      }
      else
      {
        Debug.Log("MOVING TARGET FORWARD ⏩" + target.transform.localPosition.x);
        TargetController.MoveTargetForward(target);
      }
    }
    else if (TargetController.trainingStage == 1)
    {
      if (target.transform.localPosition.x >= maximumStaticTargetDistance)
      {
        TargetController.trainingStage = 2;
        StartCoroutine(TrainingService.UpdateTrainingStage(agentTrainBehaviour.brain.name, "2"));
        Debug.Log("MOVING TARGET RANDOMLY 🔀" + target.transform.localPosition.x);
        TargetController.MoveTargetRandom(target, ground, foodSpawnRadius);
        foodSpawnRadius += foodSpawnRadiusIncrementor;
      }
      else
      {
        Debug.Log("MOVING TARGET BACKWARD ⏮️" + target.transform.localPosition.x);
        TargetController.MoveTargetBackward(target);
      }
    }
    else if (TargetController.trainingStage == 2)
    {
      Debug.Log("MOVING TARGET RANDOMLY 🔀" + target.transform.localPosition.x);
      TargetController.MoveTargetRandom(target, ground, foodSpawnRadius);
      if (foodSpawnRadius < maximumStaticTargetDistance)
      {
        foodSpawnRadius += foodSpawnRadiusIncrementor;
      }
    }
    spawner.resetMinimumTargetDistance();
  }

  Transform GetClosestEnemy(GameObject[] targets)
  {
    Transform bestTarget = null;
    float closestDistanceSqr = Mathf.Infinity;
    Vector3 currentPosition = transform.position;
    foreach (GameObject potentialTarget in targets)
    {
      Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
      float dSqrToTarget = directionToTarget.sqrMagnitude;
      if (dSqrToTarget < closestDistanceSqr)
      {
        closestDistanceSqr = dSqrToTarget;
        bestTarget = potentialTarget.transform;
      }
    }
    Debug.Log(bestTarget);
    return bestTarget;
  }

  public static void MoveTargetForward(Transform target)
  {
    target.localPosition = new Vector3(target.localPosition.x - 5f, target.localPosition.y, target.localPosition.z + 5f);
  }

  public static void MoveTargetBackward(Transform target)
  {
    target.localPosition = new Vector3(target.localPosition.x + 5f, target.localPosition.y, target.localPosition.z - 5f);
  }

  public static void MoveTargetRandom(Transform target, Transform ground, float foodSpawnRadius)
  {
    Vector3 newTargetPos = Random.insideUnitSphere * foodSpawnRadius;
    newTargetPos.y = 5;
    target.position = newTargetPos + ground.position;
  }


  public void resetMinimumTargetDistance()
  {
    minimumTargetDistance = Vector3.Distance(agentTrainBehaviour.initPart.position, target.position);
  }
}