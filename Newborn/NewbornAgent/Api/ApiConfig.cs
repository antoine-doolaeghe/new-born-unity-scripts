﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiConfig : ScriptableObject
{
  public static string url = "https://sw2hs7ufb5gevarvuyswhrndjm.appsync-api.eu-west-1.amazonaws.com/graphql";
  public static string apiKey = "da2-bwhkcmyrfvgqfccovxokq2btva";
  public static string generationsGraphQlQuery = "query getGenerations {listGenerations { items {id} }}";
  public static string generationsGraphQlMutation = "mutation createGeneration {createGeneration(input: {id: $id^, index: $index^}) { id, index }}";
  public static string newbornGraphQlMutation = "mutation NewbornPost {createNewborn(input: {id: '$id^', name: $name^, sex: $sex^, newbornGenerationId: '$newbornGenerationId^'}) {id, name, generation{index}}}";
  public static string newbornsGraphQlQuery = "query NewbornsQuery {listNewborns {items {id}}}";
  public static string modelGraphQlMutation = "mutation ModelPost {createModel(input: {id: '$id^', cellInfos: $cellInfos^, cellPositions: $cellPositions^, modelNewbornId: '$modelNewbornId^'}) { id, cellInfos }}";
  public static string newBornGraphQlQuery = "query getNewBorn {getNewborn(id: '$id^') { id, name, models { items { cellInfos } }, generation { id, index }  }}";
  public static string trainingGraphQlQuery = "query GetPost {start(newbornId: '$id^')}";
  public static string updateNewbornInstanceId = "mutation UpdateNewbornInstanceId {updateNewborn(input: {id: '$id^', instanceId: $instanceId^}) { id, instanceId }}";
}