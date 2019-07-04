using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
  public float adjScale;

  void Awake()
  {
    mesh = new Mesh();
    rend = GetComponent<SkinnedMeshRenderer>();
    bones = new List<Transform>();
    weights = new List<BoneWeight>();
    bindPoses = new List<Matrix4x4>();
    vertices = new List<Vector3>();
    triangles = new List<int>();
    adjScale = scale * 0.5f;
  }

  public GameObject MakeCube(float cubeScale, Vector3 cubePos)
  {
    Transform cubeBone = new GameObject().transform;
    cubeBone.parent = transform;
    cubeBone.localPosition = cubePos;
    for (int i = 0; i < 6; i++)
    {
      MakeFace(cubeBone, (Direction)i, cubeScale, cubePos);
    }
    return cubeBone.gameObject;
  }

  void MakeFace(Transform parent, Direction dir, float faceScale, Vector3 facePos)
  {
    Vector3[] newVertices = CubeMeshData.faceVertices(vertices.ToArray(), dir, faceScale, facePos);
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

  public void UpdateMesh()
  {
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.RecalculateNormals();
  }

  public void ResetMesh()
  {
    mesh.Clear();
    bones.Clear();
    vertices.Clear();
    triangles.Clear();
    weights.Clear();
    bindPoses.Clear();
    Array.Clear(mesh.vertices, 0, mesh.vertices.Length);
    Array.Clear(mesh.triangles, 0, mesh.triangles.Length);
  }

  void AddBoneWeight(int vCount)
  {
    BoneWeight boneWeight = new BoneWeight();
    boneWeight.boneIndex0 = vCount;
    boneWeight.weight0 = 1f;
    weights.Add(boneWeight);
  }

  public void AssignBone()
  {
    // One bone at the bottom and one at the top
    mesh.boneWeights = weights.ToArray();
    mesh.bindposes = bindPoses.ToArray();

    // Assign bones and bind poses
    rend.sharedMesh = mesh;
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
