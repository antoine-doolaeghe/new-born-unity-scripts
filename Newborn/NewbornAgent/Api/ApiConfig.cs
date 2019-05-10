using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiConfig : ScriptableObject
{
  public static string url = "https://ilhzglf4sfgepcagdzuviwewy4.appsync-api.eu-west-1.amazonaws.com/graphql";
  public static string apiKey = "da2-scnuxzzpafavxofjuecffec3hi";
  public static string generationsGraphQlQuery = "query getGenerations {listGenerations { items {id} }}";
  public static string generationsGraphQlMutation = "mutation createGenerations {createGenerations(input: {id: $id^, index: $index^}) { id, index }}";
  public static string newBornGraphQlMutation = "mutation NewbornPost {createNewborn(input: {id: '$id^', name: $name^, sex: $sex^, newbornGenerationId: '$newbornGenerationId^'}) {id, name, generation{index}}}";
  public static string modelGraphQlMutation = "mutation ModelPost {createModel(input: {id: '$id^', cellInfos: $cellInfos^, cellPositions: $cellPositions^, modelNewbornId: '$modelNewbornId^'}) { id, cellInfos }}";
  public static string newBornGraphQlQuery = "query getNewBorn {getNewborn(id: '$id^') { id, name, models { items { cellInfos } }, generation { items {id, index}}  }}";
}