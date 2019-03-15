using System;

[Serializable]
public struct NewBornPostData
{
  public string name;
  public string id;
  public String hexColor;
  public NewBornPostData(string name, string id, string hexColor)
  {
    this.id = id;
    this.name = name;
    this.hexColor = hexColor;
  }
}
