using System.Collections;
using System.Collections.Generic;
using Gene;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gene {
    public class PostGene : MonoBehaviour {
        public string[] response;
        public GameObject cell;
        public ApiConfig apiConfig;
        
        public IEnumerator postCell (string cellInfos, string cellName) {
            string url = apiConfig.url;
            PostObject postObject = new PostObject (cellInfos, cellName);
            string jsonString = JsonUtility.ToJson (postObject);
            UnityWebRequest www = UnityWebRequest.Put (url, jsonString);
            yield return www.SendWebRequest ();
            if (www.isNetworkError || www.isHttpError) {
                Debug.Log ("error");
            }
        }

        public IEnumerator getCell (string id) {
            using (UnityWebRequest www = UnityWebRequest.Get (apiConfig.url + id)) {
                yield return www.Send ();

                if (www.isNetworkError || www.isHttpError) {
                    Debug.Log (www.error);
                } else {
                    if (www.isDone) {
                        response = www.downloadHandler.text.Split ('A');
                    }
                }
            }
        }
    }

    public class PostObject {
        public string cellInfos;
        public string cellName;

        public PostObject (string cellInfos, string cellName) {
            this.cellInfos = cellInfos;
            this.cellName = cellName;
        }
    }
}