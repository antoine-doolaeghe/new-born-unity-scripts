using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VoxelRender : MonoBehaviour
{
  Mesh mesh;
  List<Vector3> vertices;
  List<int> triangles;
  public float scale = 1f;
  SkinnedMeshRenderer rend;
  List<Transform> bones;
  List<BoneWeight> weights;
  List<Matrix4x4> bindPoses;

  float adjScale;
  // Use this for initialization
  void Awake()
  {
    mesh = GetComponent<MeshFilter>().mesh;
    adjScale = scale * 0.5f;
  }

  // Update is called once per frame
  void Start()
  {
    initBoneWeight();
    GenerateVoxelMesh(new VoxelData());
    UpdateMesh();
    // AddBoneWeight();
  }

  void GenerateVoxelMesh(VoxelData data)
  {
    vertices = new List<Vector3>();
    triangles = new List<int>();
    int i = 0;

    for (int z = 0; z < data.Depth; z++)
    {
      for (int x = 0; x < data.Width; x++)
      {
        for (int y = 0; y < data.Height; y++)
        {
          if (data.GetCell(x, y, z) == 0)
          {
            continue;
          }
          MakeCube(adjScale, new Vector3((float)x * scale, (float)y * scale, (float)z * scale), x, y, z, data);
          MakeBone(i, new Vector3((float)x * scale, (float)y * scale, (float)z * scale));
          i++;
        }
      }
    }
  }

  void MakeCube(float cubeScale, Vector3 cubePos, int x, int y, int z, VoxelData data)
  {
    for (int i = 0; i < 6; i++)
    {
      if (data.GetNeighbor(x, y, z, (Direction)i) == 0)
      {
        MakeFace((Direction)i, cubeScale, cubePos);
      }
    }
  }

  void MakeFace(Direction dir, float faceScale, Vector3 facePos)
  {
    vertices.AddRange(CubeMeshData.faceVertices(dir, faceScale, facePos));
    int vCount = vertices.Count;
    triangles.Add(vCount - 4);
    triangles.Add(vCount - 4 + 1);
    triangles.Add(vCount - 4 + 2);
    triangles.Add(vCount - 4);
    triangles.Add(vCount - 4 + 2);
    triangles.Add(vCount - 4 + 3);
  }

  void UpdateMesh()
  {
    mesh.Clear();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    // AddBoneWeight();
    mesh.RecalculateNormals();
  }

  void initBoneWeight()
  {
    rend = transform.gameObject.AddComponent<SkinnedMeshRenderer>();
    bones = new List<Transform>();
    weights = new List<BoneWeight>();
    bindPoses = new List<Matrix4x4>();
  }

  // void AddBoneWeight()
  // {
  //   int y = 0;
  //   for (int i = 0; i < weights.Length; i += 1)
  //   {
  //     int t = 0;
  //     for (int z = 0; z < mesh.vertices.Length; z++)
  //     {
  //       if (mesh.vertices[i] == mesh.vertices[z])
  //       {
  //         if (t == 0)
  //         {
  //           weights[i].boneIndex0 = z;
  //           weights[i].weight0 = 0.75f;
  //         }
  //         else if (t == 1)
  //         {
  //           weights[i].boneIndex1 = z;
  //           weights[i].weight1 = 0.5f;
  //         }
  //         else if (t == 2)
  //         {
  //           weights[i].boneIndex2 = z;
  //           weights[i].weight2 = 0.25f;
  //         }
  //         else if (t == 3)
  //         {
  //           weights[i].boneIndex3 = z;
  //           weights[i].weight3 = 0f;
  //         }
  //         t += 1;
  //       }
  //     }
  //     // weights[i + 1].boneIndex0 = y;
  //     // weights[i + 1].weight0 = 1;
  //     y += 1;
  //   }
  //   // Create Bone Transforms and Bind poses
  //   // One bone at the bottom and one at the top
  //   mesh.boneWeights = weights;
  //   mesh.bindposes = bindPoses;

  //   // Assign bones and bind poses
  //   rend.bones = bones;
  // }

  void MakeBone(int i, Vector3 cubePos)
  {
    bones.Add(new GameObject(i.ToString()).transform);
    bones[i].parent = transform;
    // Set the position relative to the parent
    bones[i].localRotation = Quaternion.identity;
    bones[i].localPosition = cubePos;
    // The bind pose is bone's inverse transformation matrix
    // In this case the matrix we also make this matrix relative to the root
    // So that we can move the root game object around freely
    bindPoses.Add(bones[i].worldToLocalMatrix * transform.localToWorldMatrix);
  }
}
