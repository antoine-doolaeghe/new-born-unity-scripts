using System.Collections.Generic;
using Newborn;
using MLAgents;
using UnityEngine;

[RequireComponent(typeof(JointDriveController))] // Required to set joint forces
public class AgentTrainBehaviour : Agent
{
  [Header("Morphology")]
  [HideInInspector] [SerializeField] public List<Transform> parts;
  [HideInInspector] [SerializeField] public Transform initPart;
  [Header("API Service")]
  public bool requestApiData;
  public string cellId;
  [Header("Joint Settings")]
  [Space(10)]
  public JointDriveController jdController;
  Vector3 dirToTarget;
  float movingTowardsDot;
  float facingDot;
  [Header("Reward Functions")]
  [Space(10)]
  public bool rewardMovingTowardsTarget;
  public bool rewardFacingTarget;
  public bool rewardUseTimePenalty;
  public bool penaltyFunctionMovingAgainst;
  [HideInInspector] public NewbornSpawner spawner;
  [HideInInspector] public TargetController targetController;
  private int timePenaltyMultiplier = 0;
  private bool isNewDecisionStep;
  private int currentDecisionStep;
  private bool initialized = false;
  public override void InitializeAgent()
  {
    Debug.Log("INITIALIZING AGENT");
    if (!initialized)
    {
      initBodyParts();
      if (requestApiData)
      {
        StartCoroutine(NewbornService.UpdateTrainingStage(brain.name, targetController.trainingStage.ToString()));
      }
      currentDecisionStep = 1;
      initialized = true;
    }
  }

  public void initBodyParts()
  {
    Debug.Log("INIT BOD");
    jdController.target = targetController.target;
    jdController.SetupBodyPart(initPart);
    // targetController.minimumTargetDistance = Vector3.Distance(initPart.position, targetController.target.position);
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
      NewbornAgent newbornAgent = transform.gameObject.GetComponent<NewbornAgent>();
      if (newbornAgent.isReproducing && bodyPart.collisionController && !IsDone() && !newbornAgent.isGestating && bodyPart.collisionController.touchingNewborn != null)
      {
        targetController.TouchedNewborn(bodyPart.collisionController.touchingNewborn);
      }

      if (bodyPart.collisionController && !IsDone() && bodyPart.collisionController.touchingFood)
      {
        targetController.TouchedFood();
      }
    }

    // Update pos to target
    dirToTarget = targetController.target.position - initPart.position;

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
    AddReward((0.03f * movingTowardsDot));
  }

  /// <summary>
  /// Add Penalty and reset if the agent is moving in too far in the wrong direction.
  /// </summary>
  void PenaltyFunctionMovingAgainst()
  {
    if (targetController.minimumTargetDistance < Vector3.Distance(initPart.position, targetController.target.position) - 10f)
    {
      Debug.Log("PENALTY 🚩");
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
    AddReward(-0.001f * timePenaltyMultiplier);
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
}