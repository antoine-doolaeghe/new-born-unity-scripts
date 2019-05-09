using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiConfig : ScriptableObject
{
  public string url = "https://ilhzglf4sfgepcagdzuviwewy4.appsync-api.eu-west-1.amazonaws.com/graphql";
  public string apiKey = "da2-scnuxzzpafavxofjuecffec3hi";
  public string generationsGraphQlQuery = "query getGenerations {listGenerations { items {id} }}";
  public string generationsGraphQlMutation = "mutation createGenerations {createGenerations(input: {id: $id^, index: $index^}) { id, index }}";
  public string newBornGraphQlMutation = "mutation NewbornPost {createNewborn(input: {id: '$id^', name: $name^, newbornGenerationId: '$newbornGenerationId^'}) {id, name, newbornGenerationId, , generation{ items { index }}}}";
  public string modelGraphQlMutation = "mutation ModelPost {createModel(input: {id: '$id^', cellInfos: $cellInfos^, cellPositions: $cellPositions^, modelNewbornId: '$modelNewbornId^'}) { id, cellInfos }}";
  public string newBornGraphQlQuery = "query getNewBorn {getNewborn(id: '$id^') { id, name, models { items { cellInfos } }, generation { items {id, index}}  }}";
}