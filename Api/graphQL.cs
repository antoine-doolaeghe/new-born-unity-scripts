using UnityEngine;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using graphQLClient;

namespace graphQLClient {
  public class graphQL : MonoBehaviour
  {
    [Tooltip("Name of the pokemon you want to query")]
    public string pokemonName = "Pikachu";
    public string getPokemonDetails;



    public void GetPikachuDetails(string username)
    {
      getPokemonDetails = "mutation PutPost {createNewborn(input: { id: $id^, name: $name^, created: $created^}) {id, name,created}}";
      GraphQuery.url = "https://7oukyyayxba4fej55spxr7l2vy.appsync-api.eu-west-1.amazonaws.com/graphql";
      GetPikachuDetails(pokemonName);
      GraphQuery.onQueryComplete += DisplayResult;
      GraphQuery.variable["id"] = "111111";
      GraphQuery.variable["name"] = "helooo";
      GraphQuery.variable["created"] = "343434";
      GraphQuery graphQuery = new GraphQuery();
      graphQuery.POST(getPokemonDetails);
    }

    public void DisplayResult()
    {
      var N = JSON.Parse(GraphQuery.queryReturn);
    }

    void OnDisable()
    {
      GraphQuery.onQueryComplete -= DisplayResult;
    }
  }

}