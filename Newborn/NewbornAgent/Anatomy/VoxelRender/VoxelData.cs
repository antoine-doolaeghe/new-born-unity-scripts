using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData : MonoBehaviour
{

  int[,,] data = new int[,,] {
    { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } },
    { { 1, 1, 0 }, { 0, 0, 1 }, { 1, 0, 1 } },
    { { 1, 1, 1 }, { 1, 1, 0 }, { 1, 1, 1 } }
  };
  public int Width
  {
    get { return data.GetLength(0); }
  }

  public int Depth
  {
    get { return data.GetLength(1); }
  }

  public int Height
  {
    get { return data.GetLength(2); }
  }

  public int GetCell(int x, int y, int z)
  {
    return data[x, y, z];
  }


  public int GetNeighbor(int x, int y, int z, Direction dir)
  {
    DataCoordinate checkOffset = offsets[(int)dir];
    DataCoordinate neighborCoord = new DataCoordinate(x + checkOffset.x, y + checkOffset.y, z + checkOffset.z);

    if (neighborCoord.x < 0 || neighborCoord.x >= Width || neighborCoord.y < 0 || neighborCoord.y >= Height || neighborCoord.z < 0 || neighborCoord.z >= Depth)
    {
      return 0;
    }
    else
    {
      return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z);
    }
  }

  struct DataCoordinate
  {
    public int x;
    public int y;
    public int z;
    public DataCoordinate(int x, int y, int z)
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }
  }

  DataCoordinate[] offsets = {
    new DataCoordinate(0, 0, 1),
    new DataCoordinate(1, 0, 0),
    new DataCoordinate(0, 0, -1),
    new DataCoordinate(-1, 0, 0),
    new DataCoordinate(0, 1, 0),
    new DataCoordinate(0, -1, 0)
  };
}

public enum Direction
{
  North, East, South, West, Up, Down
}
