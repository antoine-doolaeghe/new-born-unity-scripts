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
        
        public IEnumerator postCell (string cellInfos, string cellName) {
            string url = "https://pnk98uo8jf.execute-api.eu-west-2.amazonaws.com/prod/cell";
            PostObject postObject = new PostObject (cellInfos, cellName);
            string jsonStringTrial = JsonUtility.ToJson (postObject);
            UnityWebRequest www = UnityWebRequest.Put (url, jsonStringTrial);
            yield return www.SendWebRequest ();
            if (www.isNetworkError || www.isHttpError) {
                Debug.Log ("error");
            }
        }

        public IEnumerator getCell (string id) {
            // TEMPORARY URL
            using (UnityWebRequest www = UnityWebRequest.Get ("https://pnk98uo8jf.execute-api.eu-west-2.amazonaws.com/prod/cell/" + id)) {
                yield return www.Send ();

                if (www.isNetworkError || www.isHttpError) {
                    Debug.Log (www.error);
                } else {
                    if (www.isDone) {
                        response = www.downloadHandler.text.Split ('A'); // A IS THE SPLITTER KEY BETWEEN INFOS
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