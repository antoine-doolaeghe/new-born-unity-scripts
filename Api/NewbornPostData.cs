using System;

[Serializable]
public struct NewBornPostData
{
  public string name;
  public int id;
  public NewBornPostData(string name, int id)
  {
    this.id = id;
    this.name = name;
  }
}
