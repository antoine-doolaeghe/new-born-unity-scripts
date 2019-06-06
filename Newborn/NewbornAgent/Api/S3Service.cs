//
// Copyright 2014-2015 Amazon.com, 
// Inc. or its affiliates. All Rights Reserved.
// 
// Licensed under the AWS Mobile SDK For Unity 
// Sample Application License Agreement (the "License"). 
// You may not use this file except in compliance with the 
// License. A copy of the License is located 
// in the "license" file accompanying this file. This file is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, express or implied. See the License 
// for the specific language governing permissions and 
// limitations under the License.
//

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using System;
using Amazon.S3.Util;
using System.Collections.Generic;
using Amazon.CognitoIdentity;
using Amazon;
using MLAgents;
using Tensor = MLAgents.InferenceBrain.Tensor;
using Barracuda;

public class S3Service : MonoBehaviour
{
  public string S3Region = RegionEndpoint.USEast1.SystemName;
  private RegionEndpoint _S3Region
  {
    get { return RegionEndpoint.GetBySystemName(S3Region); }
  }
  public string S3BucketName = "newborn-training-models";

  void Awake()
  {
    UnityInitializer.AttachToGameObject(this.gameObject);
    AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
  }

  #region private members

  private IAmazonS3 _s3Client;
  private AWSCredentials _credentials;

  private AWSCredentials Credentials
  {
    get
    {
      if (_credentials == null)
        _credentials = new CognitoAWSCredentials(
            "eu-west-1:7eb0715c-0ce5-476b-9ffc-b60dec05e8ab", // ID du groupe d'identités
            RegionEndpoint.EUWest1 // Région
        );
      return _credentials;
    }
  }

  private IAmazonS3 Client
  {
    get
    {
      if (_s3Client == null)
      {
        _s3Client = new AmazonS3Client(Credentials, _S3Region);
      }
      //test comment
      return _s3Client;
    }
  }

  #endregion

  /// <summary>
  /// Get Object from S3 Bucket
  /// </summary>
  public void GetObject(string newbornId, GameObject agent)
  {
    Debug.Log("Fetching model from S3");
    string SampleFileName = newbornId + ".nn";
    Client.GetObjectAsync(S3BucketName, SampleFileName, (responseObj) =>
    {
      var response = responseObj.Response;
      if (response.ResponseStream != null)
      {
        using (StreamReader reader = new StreamReader(response.ResponseStream))
        {
          using (var fs = System.IO.File.Create(@"./Assets/Newborn/Resources/" + SampleFileName))
          {
            byte[] buffer = new byte[81920];
            int count;
            while ((count = response.ResponseStream.Read(buffer, 0, buffer.Length)) != 0)
              fs.Write(buffer, 0, count);
            fs.Flush();
            MLAgents.InferenceBrain.NNModel brainmodel = Resources.Load<MLAgents.InferenceBrain.NNModel>("20181245602442");
            agent.GetComponent<NewbornAgent>().learningBrain.model = brainmodel;
          }
        }
      }
    });
  }

  #region helper methods


  private string GetPostPolicy(string bucketName, string key, string contentType)
  {
    bucketName = bucketName.Trim();

    key = key.Trim();
    // uploadFileName cannot start with /
    if (!string.IsNullOrEmpty(key) && key[0] == '/')
    {
      throw new ArgumentException("uploadFileName cannot start with / ");
    }

    contentType = contentType.Trim();

    if (string.IsNullOrEmpty(bucketName))
    {
      throw new ArgumentException("bucketName cannot be null or empty. It's required to build post policy");
    }
    if (string.IsNullOrEmpty(key))
    {
      throw new ArgumentException("uploadFileName cannot be null or empty. It's required to build post policy");
    }
    if (string.IsNullOrEmpty(contentType))
    {
      throw new ArgumentException("contentType cannot be null or empty. It's required to build post policy");
    }

    string policyString = null;
    int position = key.LastIndexOf('/');
    if (position == -1)
    {
      policyString = "{\"expiration\": \"" + DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ") + "\",\"conditions\": [{\"bucket\": \"" +
          bucketName + "\"},[\"starts-with\", \"$key\", \"" + "\"],{\"acl\": \"private\"},[\"eq\", \"$Content-Type\", " + "\"" + contentType + "\"" + "]]}";
    }
    else
    {
      policyString = "{\"expiration\": \"" + DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ") + "\",\"conditions\": [{\"bucket\": \"" +
          bucketName + "\"},[\"starts-with\", \"$key\", \"" + key.Substring(0, position) + "/\"],{\"acl\": \"private\"},[\"eq\", \"$Content-Type\", " + "\"" + contentType + "\"" + "]]}";
    }

    return policyString;
  }

}

#endregion
