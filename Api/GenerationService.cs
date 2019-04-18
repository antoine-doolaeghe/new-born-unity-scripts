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
        public IEnumerator GetGeneration () {
            string jsonData;
            byte[] postData;
            Dictionary<string, string> postHeader;

            WWW www;
            ServiceHelpers.graphQlApiRequest (out jsonData, out postData, out postHeader, out www, apiConfig.generationsGraphQlQuery);

            yield return www;
            if (www.error != null) {
                throw new Exception ("There was an error sending request: " + www.error);
            } else {
                // create newborn if the it has a generation, 
                // if it doesn't have a generation, then create a newborn. 
            }
        }

        public IEnumerator PostGeneration (string generationId) {
            string jsonData;
            byte[] postData;
            Dictionary<string, string> postHeader;

            NewbornService.variable["id"] = generationId;

            WWW www;
            ServiceHelpers.graphQlApiRequest (out jsonData, out postData, out postHeader, out www, apiConfig.generationsGraphQlMutation);

            yield return www;
            if (www.error != null) {
                throw new Exception ("There was an error sending request: " + www.error);
            } else {
                // POST new born ? 
            }
        }
    }
}