
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components.Newborn.Anatomy.Render
{
  public static class CubeMeshData
  {
    public static Vector3[] vertices = {
    new Vector3(1, 1, 1),
    new Vector3(-1, 1, 1),
    new Vector3(-1, -1, 1),
    new Vector3(1, -1, 1),
    new Vector3(-1, 1, -1),
    new Vector3(1, 1, -1),
    new Vector3(1, -1, -1),
    new Vector3(-1, -1, -1),
  };


    public static int[][] faceTriangles = {
    new int[] {0, 1, 2, 3},
    new int[] {5, 0, 3, 6},
    new int[] {4, 5, 6, 7},
    new int[] {1, 4, 7, 2},
    new int[] {5, 4, 1, 0},
    new int[] {3, 2, 7, 6},
  };

    public static Vector3[] faceVertices(Vector3[] previousVertices, int dir, float scale, Vector3 pos)
    {
      List<Vector3> fv = new List<Vector3>();
      for (int i = 0; i < 4; i++)
      {
        Vector3 newVertice = vertices[faceTriangles[dir][i]] * scale + pos;
        fv.Add(newVertice);
      }
      return fv.ToArray();
    }

    public static Vector3[] faceVertices(Vector3[] previousVertices, Direction dir, float scale, Vector3 pos)
    {
      return faceVertices(previousVertices, (int)dir, scale, pos);
    }
  }

}