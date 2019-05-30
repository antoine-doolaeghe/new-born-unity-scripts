using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newborn;
using MLAgents;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(JointDriveController))] // Required to set joint forces
public class AgentTrainBehaviour : Agent
{
  [Header("Morphology")]
  [SerializeField] public List<Transform> parts;
  [SerializeField] public Transform initPart;
  [Header("API Service")]
  public bool requestApiData;
  public string cellId;
  public NewbornService newbornService;

  [Header("Target")]
  [Space(10)]
  public Transform target;
  public Transform ground;
  public bool respawnFoodWhenTouched;
  public float foodSpawnRadius;

  [Header("Joint Settings")]
  [Space(10)]
  public JointDriveController jdController;
  Vector3 dirToTarget;
  float initTargetDistance;
  float movingTowardsDot;
  float facingDot;

  [Header("Reward Functions")]
  [Space(10)]
  public bool rewardMovingTowardsTarget; // Agent should move towards target
  public bool rewardFacingTarget; // Agent should face the target
  public bool rewardUseTimePenalty; // Hurry up
  public bool penaltyFunctionMovingAgainst; // stay in the zone

  private bool isNewDecisionStep;
  private int currentDecisionStep;

  public override void InitializeAgent()
  {
    initBodyParts();
    currentDecisionStep = 1;
  }

  public void initBodyParts()
  {
    jdController.target = target;
    jdController.SetupBodyPart(initPart);
    initTargetDistance = Vector3.Distance(initPart.position, target.position);
    foreach (var part in parts)
    {
      jdController.SetupBodyPart(part);
    }
  }

  public void DeleteBodyParts()
  {
    parts.Clear();
  }

  /// <summary>
  /// We only need to change the joint settings based on decision freq.
  /// </summary>
  public void IncrementDecisionTimer()
  {
    if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
      agentParameters.numberOfActionsBetweenDecisions == 1)
    {
      currentDecisionStep = 1;
      isNewDecisionStep = true;
    }
    else
    {
      currentDecisionStep++;
      isNewDecisionStep = false;
    }
  }

  /// <summary>
  /// Add relevant information on each morphology part to observations.
  /// </summary>
  public void CollectObservationBodyPart(BodyPart bp)
  {
    var rb = bp.rb;
    AddVectorObs(rb.velocity);
    AddVectorObs(rb.angularVelocity);

    if (bp.rb.transform != initPart)
    {
      Vector3 localPosRelToBody = initPart.InverseTransformPoint(rb.position);
      AddVectorObs(localPosRelToBody);
      AddVectorObs(bp.currentXNormalizedRot); // Current x rot
      AddVectorObs(bp.currentYNormalizedRot); // Current y rot
      AddVectorObs(bp.currentZNormalizedRot); // Current z rot
      AddVectorObs(bp.currentStrength / jdController.maxJointForceLimit);
    }
  }

  public override void CollectObservations()
  {
    jdController.GetCurrentJointForces();
    // Normalize dir vector to help generalize
    AddVectorObs(dirToTarget.normalized);

    foreach (var bodyPart in jdController.bodyPartsDict.Values)
    {
      CollectObservationBodyPart(bodyPart);
    }
  }

  public override void AgentAction(float[] vectorAction, string textAction)
  {

    foreach (var bodyPart in jdController.bodyPartsDict.Values)
    {
      if (bodyPart.collisionController && !IsDone() && !transform.gameObject.GetComponent<NewbornAgent>().isGestating && bodyPart.collisionController.touchingNewborn != null)
      {
        TouchedNewborn(bodyPart.collisionController.touchingNewborn);
      }

      if (bodyPart.collisionController && !IsDone() && bodyPart.collisionController.touchingFood)
      {
        TouchedFood();
      }
    }

    // Update pos to target
    dirToTarget = target.position - initPart.position;

    // Joint update logic only needs to happen when a new decision is made
    if (isNewDecisionStep)
    {
      // The dictionary with all the body parts in it are in the jdController
      var bpDict = jdController.bodyPartsDict;

      int i = 1;

      foreach (var part in parts)
      {
        // Pick a new target joint rotation
        bpDict[part].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
        // Update joint strength
        bpDict[part].SetJointStrength(vectorAction[++i]);
      }
    }

    // Set reward for this step according to mixture of the following elements.
    if (rewardMovingTowardsTarget)
    {
      RewardFunctionMovingTowards();
    }

    if (rewardFacingTarget)
    {
      RewardFunctionFacingTarget();
    }

    if (rewardUseTimePenalty)
    {
      RewardFunctionTimePenalty();
    }

    if (penaltyFunctionMovingAgainst)
    {
      PenaltyFunctionMovingAgainst();
    }

    IncrementDecisionTimer();
  }

  /// <summary>
  /// Reward moving towards target & Penalize moving away from target.
  /// </summary>
  void RewardFunctionMovingTowards()
  {
    movingTowardsDot = Vector3.Dot(jdController.bodyPartsDict[initPart].rb.velocity, dirToTarget.normalized);
    AddReward(0.03f * movingTowardsDot);
  }

  /// <summary>
  /// Add Penalty and reset if the agent is moving in too far in the wrong direction.
  /// </summary>
  void PenaltyFunctionMovingAgainst()
  {
    if (initTargetDistance < Vector3.Distance(initPart.position, target.position) - 10f)
    {
      Debug.Log("HERE PENALTY");
      SetReward(-10f);
      AgentReset();
    };
  }

  /// <summary>
  /// Reward facing target & Penalize facing away from target
  /// </summary>
  void RewardFunctionFacingTarget()
  {
    facingDot = Vector3.Dot(dirToTarget.normalized, initPart.forward);
    AddReward(0.01f * facingDot);
  }

  /// <summary>
  /// Existential penalty for time-contrained tasks.
  /// </summary>
  void RewardFunctionTimePenalty()
  {
    AddReward(-0.001f);
  }

  /// <summary>
  /// Loop over Morphology parts and reset them to initial conditions.
  /// </summary>
  public override void AgentReset()
  {

    if (dirToTarget != Vector3.zero)
    {
      transform.rotation = Quaternion.LookRotation(dirToTarget);
    }

    foreach (var bodyPart in jdController.bodyPartsDict.Values)
    {
      bodyPart.Reset(bodyPart);
    }

    isNewDecisionStep = true;
    currentDecisionStep = 1;
  }

  /// <summary>
  /// Agent touched the target
  /// </summary>
  public void TouchedNewborn(GameObject touchingNewborn)
  {
    AddReward(15f);
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
      newborn.isGestating = true;
      List<GeneInformation> femaleGene = newBornBuilder.GeneInformations;
      List<GeneInformation> maleGene = touchingNewborn.GetComponent<NewBornBuilder>().GeneInformations;
      List<GeneInformation> newGene = GeneHelper.ReturnMixedForReproduction(femaleGene, maleGene);
      // prepare post data
      string newNewbornName = "name";
      string newNewbornId = Regex.Replace(System.Guid.NewGuid().ToString(), @"[^0-9]", "");
      string newNewbornGenerationId = newborn.GenerationId;
      string newNewbornSex = "male";
      string newNewbornHex = "MOCK HEX";
      // DO a generation check ? 
      NewBornPostData newBornPostData = new NewBornPostData(newNewbornName, newNewbornId, newNewbornGenerationId, newNewbornSex, newNewbornHex);
      // SEND THE TRAINING INSTANCE HERE;
      yield return NewbornService.PostReproducedNewborn(newBornPostData, transform.gameObject, touchingNewborn);
      NewbornService.BuildAgentCallback Callback = NewbornService.SuccessfullReproductionCallback;
      yield return newBornBuilder.PostNewbornModel(newborn.childs[newborn.childs.Count - 1], 0, transform.gameObject, Callback); // will it always be first generation
      yield return TrainingService.TrainNewborn(newborn.childs[newborn.childs.Count - 1]);
    }
  }
  public void TouchedFood()
  {
    AddReward(15f);
    AgentReset();
    if (respawnFoodWhenTouched)
    {
      GetRandomFoodPos();
    }
  }

  /// <summary>
  /// Moves target to a random position within specified radius.
  /// </summary>
  public void GetRandomFoodPos()
  {
    Vector3 newTargetPos = Random.insideUnitSphere * foodSpawnRadius;
    newTargetPos.y = 5;
    target.position = newTargetPos + ground.position;
  }
}