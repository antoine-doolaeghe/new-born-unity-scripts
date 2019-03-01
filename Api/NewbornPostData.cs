using System;

[Serializable]
public struct NewBornPostData
{
  public string name;
  public System.Guid id;
  public String hexColor;
  public NewBornPostData(string name, System.Guid id, string hexColor)
  {
    this.id = id;
    this.name = name;
    this.hexColor = hexColor;
  }
}
