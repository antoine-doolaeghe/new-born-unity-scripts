using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AnatomyRender : MonoBehaviour
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
    AssignBone();
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
          MakeCube(adjScale, new Vector3((float)x * scale, (float)y * scale, (float)z * scale), x, y, z, data, i);
          i++;
        }
      }
    }
  }

  // TO DO: ASSIGN THE OTHER VERTICE. 
  void MakeCube(float cubeScale, Vector3 cubePos, int x, int y, int z, VoxelData data, int w)
  {
    Transform cubeBone = new GameObject().transform;
    cubeBone.parent = transform;
    cubeBone.localPosition = cubePos;
    for (int i = 0; i < 6; i++)
    {
      if (data.GetNeighbor(x, y, z, (Direction)i) == 0)
      {
        MakeFace(cubeBone, (Direction)i, cubeScale, cubePos);
      }
      else
      {
        // MAKE SURE THE vertices are assigned
      }
    }
  }


  void MakeFace(Transform parent, Direction dir, float faceScale, Vector3 facePos)
  {
    Vector3[] newVertices = CubeMeshData.faceVertices(dir, faceScale, facePos);
    vertices.AddRange(newVertices);
    for (var i = 0; i < newVertices.Length; i++)
    {
      int verticeIndex = vertices.IndexOf(newVertices[i]);
      MakeBone(newVertices[i], parent);
      AddBoneWeight(verticeIndex);
    }

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
    mesh.RecalculateNormals();
  }

  void initBoneWeight()
  {
    rend = transform.gameObject.AddComponent<SkinnedMeshRenderer>();
    bones = new List<Transform>();
    weights = new List<BoneWeight>();
    bindPoses = new List<Matrix4x4>();
  }

  void AddBoneWeight(int vCount)
  {
    BoneWeight boneWeight = new BoneWeight();
    boneWeight.boneIndex0 = vCount;
    boneWeight.weight0 = 1f;
    weights.Add(boneWeight);
  }

  void AssignBone()
  {
    // One bone at the bottom and one at the top
    mesh.boneWeights = weights.ToArray();
    mesh.bindposes = bindPoses.ToArray();

    // Assign bones and bind poses
    rend.bones = bones.ToArray();
  }

  void MakeBone(Vector3 cubePos, Transform parent)
  {
    bones.Add(new GameObject().transform);
    bones[bones.Count - 1].parent = parent;
    // Set the position relative to the parent
    bones[bones.Count - 1].localRotation = Quaternion.identity;
    bones[bones.Count - 1].localPosition = cubePos;
    // The bind pose is bone's inverse transformation matrix
    // In this case the matrix we also make this matrix relative to the root
    // So that we can move the root game object around freely
    bindPoses.Add(bones[bones.Count - 1].worldToLocalMatrix * transform.localToWorldMatrix);
  }
}
