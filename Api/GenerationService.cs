using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using graphQLClient;
using Gene;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gene {
    [ExecuteInEditMode]
    public class GenerationService : MonoBehaviour {
        public ApiConfig apiConfig;
        private String graphQlInput;
        public static Dictionary<string, string> variable = new Dictionary<string, string> ();
        public static Dictionary<string, string[]> array = new Dictionary<string, string[]> ();

        public IEnumerator GetGeneration () {
            byte[] postData;
            Dictionary<string, string> postHeader;

            WWW www;
            ServiceHelpers.graphQlApiRequest (variable, array, out postData, out postHeader, out www, out graphQlInput, apiConfig.generationsGraphQlQuery);

            yield return www;
            if (www.error != null) {
                throw new Exception ("There was an error sending request: " + www.error);
            } else {
                // create newborn if the it has a generation, 
                // if it doesn't have a generation, then create a newborn. 
            }
        }

        public IEnumerator PostGeneration (string generationId) {
            byte[] postData;
            Dictionary<string, string> postHeader;

            NewbornService.variable["id"] = generationId;

            WWW www;
            ServiceHelpers.graphQlApiRequest (variable, array, out postData, out postHeader, out www, out graphQlInput, apiConfig.generationsGraphQlMutation);

            yield return www;
            if (www.error != null) {
                throw new Exception ("There was an error sending request: " + www.error);
            } else {
                // POST new born ? 
            }
        }
    }
}