using System.Collections.Generic;
using UnityEngine;

namespace Newborn
{
  public class AnatomyHelpers
  {
    public static List<Vector3> Sides = new List<Vector3> {
      new Vector3 (0.2f, 0f, 0f),
      new Vector3 (0f, 0.2f, 0f),
      new Vector3 (0f, 0f, 0.2f),
      new Vector3 (-0.2f, 0f, 0f),
      new Vector3 (0f, -0.2f, 0f),
      new Vector3 (0f, 0f, -0.2f)
    };
    public static void InitJoint(GameObject part, GameObject connectedBody, Vector3 jointAnchor, bool isInitJoint = false)
    {
      ConfigurableJoint cj = part.transform.gameObject.AddComponent<ConfigurableJoint>();
      cj.xMotion = ConfigurableJointMotion.Locked;
      cj.yMotion = ConfigurableJointMotion.Locked;
      cj.zMotion = ConfigurableJointMotion.Locked;
      cj.angularXMotion = isInitJoint ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
      cj.angularYMotion = isInitJoint ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
      cj.angularZMotion = isInitJoint ? ConfigurableJointMotion.Locked : ConfigurableJointMotion.Limited;
      cj.anchor = -jointAnchor;
      cj.connectedBody = connectedBody.gameObject.GetComponent<Rigidbody>();
      cj.rotationDriveMode = RotationDriveMode.Slerp;
      cj.angularYLimit = new SoftJointLimit() { limit = AgentConfig.yLimit, bounciness = AgentConfig.bounciness };
      AnatomyHelpers.HandleAngularLimit(cj, jointAnchor);
      cj.angularZLimit = new SoftJointLimit() { limit = AgentConfig.zLimit, bounciness = AgentConfig.bounciness };
      part.gameObject.GetComponent<Rigidbody>().useGravity = !isInitJoint;
      part.gameObject.GetComponent<Rigidbody>().mass = 1f;
    }
    public static void InitPosition(List<Vector3> sides, int y, int i, int z, GameObject cell, NewbornAgent newborn)
    {
      cell.transform.parent = newborn.NewBornGenerations[y - 1][i].mesh.transform;
      cell.transform.localPosition = sides[z];
    }

    public static bool IsValidPosition(NewbornAgent newborn, Vector3 cellPosition)
    {
      foreach (var position in newborn.CellPositions)
      {
        if (cellPosition == position)
        {
          return false;
        }
      }

      return true;
    }
    public static void InitRigidBody(GameObject cell)
    {
      cell.GetComponent<Rigidbody>().useGravity = true;
      cell.GetComponent<Rigidbody>().mass = 1f;
    }

    public static List<PositionPostData> ReturnModelPositions(NewbornAgent newborn)
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
    public static void HandleAngularLimit(ConfigurableJoint cj, Vector3 jointAnchor)
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
  }
}