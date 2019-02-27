using System;

[Serializable]
public struct NewBornPostData
{
  public string name;
  public System.Guid id;
  public NewBornPostData(string name, System.Guid id)
  {
    this.id = id;
    this.name = name;
  }
}
