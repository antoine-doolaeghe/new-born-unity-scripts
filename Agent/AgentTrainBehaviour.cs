using System.Collections;
using System.Collections.Generic;
using Gene;
using MLAgents;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (JointDriveController))] // Required to set joint forces
public class AgentTrainBehaviour : Agent {
	[Header ("Connection to API Service")]
	public bool requestApiData;
	public string cellId;
	public PostGene postGene;
	[Header ("Target To Walk Towards")]
	[Space (10)]
	public Transform target;
	public Transform ground;
	public bool detectTargets;
	public bool respawnTargetWhenTouched;
	public float targetSpawnRadius;

	[Header ("Morphology Parts")]
	[Space (10)]
	public int partNb;
	public float threshold;
	[HideInInspector] public List<Transform> parts;
	[HideInInspector] public Transform initPart;

	[Header ("Joint Settings")][Space (10)] JointDriveController jdController;
	Vector3 dirToTarget;
	float movingTowardsDot;
	float facingDot;

	[Header ("Reward Functions To Use")]
	[Space (10)]
	public bool rewardMovingTowardsTarget; // Agent should move towards target
	public bool rewardFacingTarget; // Agent should face the target
	public bool rewardUseTimePenalty; // Hurry up

	bool isNewDecisionStep;
	int currentDecisionStep;

	public override void InitializeAgent () {
		jdController = GetComponent<JointDriveController> ();
		// Handle starting/communication with api data
		Cell cell = GetComponent<Cell> ();
		if (requestApiData) {
			cell.partNb = partNb;
			cell.threshold = threshold;
			StartCoroutine (postGene.getCell (cellId));
		} else {
			cell.initGerms (partNb, threshold);
		}
		currentDecisionStep = 1;
	}

	public void initBodyParts () {
		jdController.SetupBodyPart (initPart);
		foreach (var part in parts) {
			jdController.SetupBodyPart (part);
		}
	}

	/// <summary>
	/// We only need to change the joint settings based on decision freq.
	/// </summary>
	public void IncrementDecisionTimer () {
		if (currentDecisionStep == agentParameters.numberOfActionsBetweenDecisions ||
			agentParameters.numberOfActionsBetweenDecisions == 1) {
			currentDecisionStep = 1;
			isNewDecisionStep = true;
		} else {
			currentDecisionStep++;
			isNewDecisionStep = false;
		}
	}

	/// <summary>
	/// Add relevant information on each morphology part to observations.
	/// </summary>
	public void CollectObservationBodyPart (BodyPart bp) {
		var rb = bp.rb;

		if (bp.rb.transform != initPart) {
			Vector3 localPosRelToBody = initPart.InverseTransformPoint (rb.position);
		}
	}

	public override void CollectObservations () {
		jdController.GetCurrentJointForces ();
		// Normalize dir vector to help generalize
		AddVectorObs (dirToTarget.normalized);

		foreach (var bodyPart in jdController.bodyPartsDict.Values) {
			CollectObservationBodyPart (bodyPart);
		}
	}

	/// <summary>
	/// Agent touched the target
	/// </summary>
	public void TouchedTarget () {
		AddReward (1f);
		if (respawnTargetWhenTouched) {
			GetRandomTargetPos ();
		}
	}

	/// <summary>
	/// Moves target to a random position within specified radius.
	/// </summary>
	public void GetRandomTargetPos () {
		Vector3 newTargetPos = Random.insideUnitSphere * targetSpawnRadius;
		newTargetPos.y = 5;
		target.position = newTargetPos + ground.position;
	}

	public override void AgentAction (float[] vectorAction, string textAction) {

		if (detectTargets) {
			foreach (var bodyPart in jdController.bodyPartsDict.Values) {
				if (bodyPart.targetContact && !IsDone () && bodyPart.targetContact.touchingTarget) {
					TouchedTarget ();
				}
			}
		}

		// Update pos to target
		dirToTarget = target.position - jdController.bodyPartsDict[initPart].rb.position;

		// Joint update logic only needs to happen when a new decision is made
		if (isNewDecisionStep) {
			// The dictionary with all the body parts in it are in the jdController
			var bpDict = jdController.bodyPartsDict;

			int i = 1;

			foreach (var part in parts) {
				// Pick a new target joint rotation
				bpDict[part].SetJointTargetRotation (vectorAction[++i], vectorAction[++i], 0);
				// Update joint strength
				bpDict[part].SetJointStrength (vectorAction[++i]);
			}

			NewMethod (vectorAction, i);
		}

		// Set reward for this step according to mixture of the following elements.
		if (rewardMovingTowardsTarget) {
			RewardFunctionMovingTowards ();
		}

		if (rewardFacingTarget) {
			RewardFunctionFacingTarget ();
		}

		if (rewardUseTimePenalty) {
			RewardFunctionTimePenalty ();
		}

		IncrementDecisionTimer ();
	}

	/// <summary>
	/// Reward moving towards target & Penalize moving away from target.
	/// </summary>
	void RewardFunctionMovingTowards () {
		movingTowardsDot = Vector3.Dot (jdController.bodyPartsDict[initPart].rb.velocity, dirToTarget.normalized);
		AddReward (0.03f * movingTowardsDot);
	}

	/// <summary>
	/// Reward facing target & Penalize facing away from target
	/// </summary>
	void RewardFunctionFacingTarget () {
		facingDot = Vector3.Dot (dirToTarget.normalized, initPart.forward);
		AddReward (0.01f * facingDot);
	}

	/// <summary>
	/// Existential penalty for time-contrained tasks.
	/// </summary>
	void RewardFunctionTimePenalty () {
		AddReward (-0.001f);
	}

	/// <summary>
	/// Loop over Morphology parts and reset them to initial conditions.
	/// </summary>
	public override void AgentReset () {
		if (dirToTarget != Vector3.zero) {
			transform.rotation = Quaternion.LookRotation (dirToTarget);
		}

		foreach (var bodyPart in jdController.bodyPartsDict.Values) {
			bodyPart.Reset (bodyPart);
		}

		isNewDecisionStep = true;
		currentDecisionStep = 1;
	}
}

// //////////////////////
/// TEMPORARY METHOD ///
// ////////////////////// 
// private void NewMethod (float[] vectorAction, int i) {
	//   if (colorGrading != null && vectorAction[i++] > 0.5f && vectorAction[i] < 0.55f)
	//   {
	//     colorGrading.temperature.value = 100f;
	//     colorGrading.tint.value = 100f;
	//     colorGrading.saturation.value = 100f;
	//     colorGrading.contrast.value = -60f;
	//   }
	//   else
	//   {
	//     colorGrading.temperature.value = colorGrading.temperature.value == 0f ? colorGrading.temperature.value : colorGrading.temperature.value - 10f;
	//     colorGrading.tint.value = 0f;
	//     colorGrading.saturation.value = colorGrading.saturation.value == 0f ? colorGrading.saturation.value : colorGrading.saturation.value - 10f;
	//     colorGrading.contrast.value = 0f;
	//   }

	//   ParticleSystem[] particleSystems = particleSystem.GetComponentsInChildren<ParticleSystem>();

	//   if (vectorAction[i++] > 0.3f)
	//   {
	//     text.SetActive(true);
	//   }
	//   else
	//   {
	//     text.SetActive(false);
	//   }

	//   if (vectorAction[i++] > 0.3f)
	//   {
	//     particleSystems[0].Play();
	//   }
	//   else
	//   {
	//     particleSystems[0].Stop();
	//   }

	//   if (vectorAction[i++] > 0.3f && vectorAction[i] > 0.75f)
	//   {
	//     particleSystems[1].Play();
	//   }
	//   else
	//   {
	//     particleSystems[1].Stop();
	//   }

	//   if (vectorAction[i++] > 0.3f && vectorAction[i] > 0.75f)
	//   {
	//     particleSystems[2].Play();
	//   }
	//   else
	//   {
	//     particleSystems[2].Stop();
	//   }

	//   if (vectorAction[i++] > 0.65f)
	//   {
	//     image.texture = Resources.Load("Textures/1") as Texture;
	//     // image.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(Random.Range(-100f, 110f), Random.Range(-100f, 100f), 0f);
	//     // image.gameObject.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, Random.Range(-50f, 50f));
	//     imageAlpha = 1f;
	//   }
	//   else
	//   {
	//     imageAlpha = imageAlpha - 0.1f;
	//   }
	//   Color color = Color.white;
	//   color.a = imageAlpha;
	//   image.color = color;

	//   handleAgentSound(vectorAction, i);
	//   return i;
// }