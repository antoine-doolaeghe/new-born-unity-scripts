using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Service.Newborn;
using Components.Newborn;
using Components.Newborn.Gene;
using Components.Newborn.Util;
using Components.Newborn.Anatomy;
using Components.Spawner.Newborn;

namespace Components.Target
{
  public class TargetController : MonoBehaviour
  {
    public Transform target;
    public Transform ground;
    public bool isSearchingForTarget;
    public bool isRandomFoodRespawn;
    public float searchTargetFrequency;
    public int trainingStage;
    public float maximumStaticTargetDistance;
    public float foodSpawnRadius;
    public float foodSpawnRadiusIncrementor;
    public float minimumTargetDistance;
    [HideInInspector] public NewbornSpawner spawner;
    [HideInInspector] public AgentTrainBehaviour agentTrainBehaviour;
    private float timer = 0.0f;

    void Awake()
    {
      agentTrainBehaviour = transform.GetComponent<AgentTrainBehaviour>();
    }

    void Update()
    {
      if (isSearchingForTarget)
      {
        timer += Time.deltaTime;
        if (timer > searchTargetFrequency)
        {
          SearchForTarget();
          timer = 0.0f;
        }
      }
    }

    public void TouchedNewborn(GameObject touchingNewborn)
    {
      agentTrainBehaviour.AddReward(15f);
      StartCoroutine(handleTouchedNewborn(touchingNewborn));
    }

    public void TouchedFood()
    {
      Debug.Log("🍏🍏 TOUCHED FOOD 🍏🍏");
      agentTrainBehaviour.AddReward(15f);
      spawner.resetTrainingAgents();
      RespawnFood();
    }

    public IEnumerator handleTouchedNewborn(GameObject touchingNewborn)
    {
      NewbornAgent newborn = transform.gameObject.GetComponent<NewbornAgent>();
      AnatomyBuilder builder = transform.gameObject.GetComponent<AnatomyBuilder>();
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
        yield return StartCoroutine(builder.checkNewbornGeneration());
        NewBornPostData newBornPostData = new NewBornPostData(newNewbornName, NewbornBrain.GenerateRandomName(), newNewbornGenerationId, newNewbornSex, newNewbornHex);
        NewbornService.PostNewbornFromReproductionCallback PostNewbornFromReproductionCallback = NewbornService.SuccessfullPostNewbornFromReproductionCallback;
        yield return NewbornService.PostNewbornFromReproduction(newBornPostData, transform.gameObject, touchingNewborn, PostNewbornFromReproductionCallback);
      }
    }

    private void RespawnFood()
    {
      if (isRandomFoodRespawn)
      {
        TargetController.MoveTargetRandom(target, ground, foodSpawnRadius);
      }
      else
      {
        if (trainingStage == 0)
        {
          if (target.transform.localPosition.x <= -maximumStaticTargetDistance)
          {
            Debug.Log("MOVING TARGET BACKWARD ⏮️" + target.transform.localPosition.x);
            trainingStage = 1;
            StartCoroutine(NewbornService.UpdateTrainingStage(agentTrainBehaviour.brain.name, "1"));
            target.transform.localPosition = new Vector3(35f, target.transform.localPosition.y, 35f);
          }
          else
          {
            Debug.Log("MOVING TARGET FORWARD ⏩" + target.transform.localPosition.x);
            TargetController.MoveTargetForward(target);
          }
        }
        else if (trainingStage == 1)
        {
          if (target.transform.localPosition.x >= maximumStaticTargetDistance)
          {
            trainingStage = 2;
            StartCoroutine(NewbornService.UpdateTrainingStage(agentTrainBehaviour.brain.name, "2"));
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
        else if (trainingStage == 2)
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
    }

    Transform FindClosestTarget(GameObject[] targets)
    {
      Transform bestTarget = null;
      float closestDistanceSqr = Mathf.Infinity;
      Vector3 currentPosition = transform.position;
      foreach (GameObject potentialTarget in targets)
      {
        if (potentialTarget != this.gameObject)
        {
          Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
          float dSqrToTarget = directionToTarget.sqrMagnitude;
          if (dSqrToTarget < closestDistanceSqr)
          {
            closestDistanceSqr = dSqrToTarget;
            bestTarget = potentialTarget.transform;
          }
        }
      }

      return bestTarget;
    }

    public void SearchForTarget()
    {
      Transform closestNewborn = FindClosestTarget(GameObject.FindGameObjectsWithTag("agent"));
      if (!transform.GetComponent<NewbornAgent>().isGestating && closestNewborn != null && !closestNewborn.GetComponent<NewbornAgent>().isGestating)
      {
        if (closestNewborn != null && closestNewborn.childCount > 0) { target = closestNewborn.GetChild(0); }
      }
      else
      {
        Transform closestTarget = FindClosestTarget(GameObject.FindGameObjectsWithTag("food"));
        Debug.Log(closestTarget);
        target = closestTarget;
      }
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
}
