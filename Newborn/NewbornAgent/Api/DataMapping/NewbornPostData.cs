using System;

[Serializable]
public struct NewBornPostData
{
  public string name;
  public string sex;
  public string id;
  public String hexColor;
  public string generationId;
  public NewBornPostData(string name, string id, string generationId, string sex, string hexColor)
  {
    this.id = id;
    this.generationId = generationId;
    this.name = name;
    this.sex = sex;
    this.hexColor = hexColor;
  }
}