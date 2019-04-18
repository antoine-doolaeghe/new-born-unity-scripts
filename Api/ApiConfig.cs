using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiConfig : ScriptableObject {
  public string url = "https://ilhzglf4sfgepcagdzuviwewy4.appsync-api.eu-west-1.amazonaws.com/graphql";
  public string apiKey = "da2-i732yxi7qng65jnaphvfsyozqu";
  public string generationsGraphQlQuery = "query getGenerations {getGenerations() { id }}";
  public string generationsGraphQlMutation = "mutation createGenerations {createGenerations(input: {id: '$id^'}) { id }}";
  public string newBornGraphQlMutation = "mutation NewbornPost {createNewborn(input: {id: '$id^', name: $name^}) {id, name}}";
  public string modelGraphQlMutation = "mutation ModelPost {createModel(input: {id: '$id^', cellInfos: $cellInfos^, modelNewbornId: '$modelNewbornId^'}) { id, cellInfos }}";
  public string newBornGraphQlQuery = "query getNewBorn {getNewborn(id: '$id^') { id, name, models { items { cellInfos } } }}";
}