using EasyBuildSystem.Runtimes.Internal.Group;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Storage.Data;
using EasyBuildSystem.Runtimes.Internal.Storage.Serialization;
using EasyBuildSystem.Runtimes.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Service.Trainer;
using Components.Target;
using Components.Manager;

#if UNITY_EDITOR

using UnityEditor;

#endif

public enum StorageType
{
  Desktop,
  Android
}

public enum StorageSerializerType
{
  Binary,
  Json
}

[AddComponentMenu("Easy Build System/Features/Utilities/Build Storage")]
public class BuildStorage : MonoBehaviour
{
  #region Public Fields

  public StorageType StorageType;

  public StorageSerializerType StorageSerializer = StorageSerializerType.Json;

  public bool AutoSave = false;

  public float AutoSaveInterval = 60f;

  public bool SavePrefabs = true;

  public string StorageOutputFile;

  [HideInInspector]
  public bool LoadedFile = false;

  #endregion Public Fields

  #region Private Fields

  private float TimerAutoSave;

  private List<PartBehaviour> PrefabsLoaded = new List<PartBehaviour>();

  private bool FileIsCorrupted;

  private BuildManager Manager;

  #endregion Private Fields

  #region Public Methods
  /// <summary>
  /// This method allows to save the storage file.
  /// </summary>
  public void SaveStorageFile()
  {
    StartCoroutine(SaveDataFile());
  }

  /// <summary>
  /// This method allows to check if the storage file.
  /// </summary>
  public bool ExistsStorageFile()
  {
    return File.Exists(StorageOutputFile);
  }

  #endregion Public Methods

  #region Private Methods
  private void Start()
  {
    FindObjectOfType<Manager>().DeleteEnvironment();
    GetTrainingData();


    if (AutoSave)
      TimerAutoSave = AutoSaveInterval;
  }

  private void Update()
  {
    if (AutoSave)
    {
      if (TimerAutoSave <= 0)
      {
        Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Saving of " + FindObjectsOfType<PartBehaviour>().Length + " Part(s) ...");

        SaveStorageFile();

        Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Saved with successfuly !");

        TimerAutoSave = AutoSaveInterval;
      }
      else
        TimerAutoSave -= Time.deltaTime;
    }
  }

  private void OnApplicationPause(bool pause)
  {
    if (StorageType == StorageType.Android)
    {
      if (!SavePrefabs)
        return;

      SaveStorageFile();
    }
  }

  private void OnApplicationQuit()
  {
    if (!SavePrefabs)
      return;

    SaveStorageFile();
  }

  public void GetTrainingData()
  {
    StartCoroutine(transform.GetComponent<TrainerService>().DownloadTrainer("test"));
  }

  public IEnumerator LoadDataFile(string data = null)
  {
    if (StorageType == StorageType.Desktop)
    {
      if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
      {
        Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : Please define output path.");

        yield break;
      }
    }

    int PrefabLoaded = 0;

    PrefabsLoaded = new List<PartBehaviour>();

    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Loading data file (" + StorageSerializer.ToString() + ") ...");

    BinaryFormatter Formatter = new BinaryFormatter();

    Formatter.Binder = new BinderFormatter();

    FileStream Stream = null;

    PartModel Serializer = null;

    try
    {
      data = data.Replace("\\", "");
      data = data.Replace("=", ":");
      Serializer = JsonUtility.FromJson<PartModel>(data);
    }
    catch (Exception ex)
    {
      FileIsCorrupted = true;

      Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : " + ex);

      EventHandlers.StorageFailed(ex.Message);

      yield break;
    }

    if (Serializer == null)
    {
      EventHandlers.StorageFailed("The storage file is corrupted.");
      yield break;
    }

    Manager = FindObjectOfType<BuildManager>();
    foreach (PartModel.SerializedPart Data in Serializer.Prefabs)
    {
      if (Data != null)
      {
        PartBehaviour Prefab = Manager.GetPart(Data.Id);
        Prefab.uuid = Data.uuid;
        Prefab.targetUuid = Data.targetUuid;
        if (Prefab != null)
        {
          PartBehaviour PlacedPrefab = Manager.PlacePrefab(Prefab,
              PartModel.ParseToVector3(Data.Position),
              PartModel.ParseToVector3(Data.Rotation),
              PartModel.ParseToVector3(Data.Scale),
              FindObjectOfType<Manager>().transform, null);
          PlacedPrefab.ChangeAppearance(Data.AppearanceIndex);

          PlacedPrefab.transform.position = PartModel.ParseToVector3(Data.Position);
          PlacedPrefab.transform.eulerAngles = PartModel.ParseToVector3(Data.Rotation);
          PlacedPrefab.transform.localScale = PartModel.ParseToVector3(Data.Scale);
          PlacedPrefab.ExtraProperties = Data.Properties;

          PrefabsLoaded.Add(PlacedPrefab);

          PrefabLoaded++;
        }
        else
          Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The prefab (" + Data.Id + ") does not exists in the list of Build Manager.");
      }
    }

    AssignTarget();

    if (Stream != null)
      Stream.Close();

    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Data file loaded " + PrefabLoaded + " prefab(s) loaded in " + Time.realtimeSinceStartup.ToString("#.##") + " ms.");

    LoadedFile = true;

    EventHandlers.StorageLoadingDone(PrefabsLoaded.ToArray());

    yield break;

  }

  private IEnumerator SaveDataFile()
  {
    if (FileIsCorrupted)
    {
      Debug.LogWarning("<b><color=cyan>[Easy Build System]</color></b> : The file is corrupted, the Prefabs could not be saved.");

      yield break;
    }

    if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
    {
      Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : Please define out file path.");

      yield break;
    }

    int SavedCount = 0;

    if (ExistsStorageFile())
    {
      File.Delete(StorageOutputFile);
    }
    else
    {
      EventHandlers.StorageFailed("The file does not exists or the path is not found.");
    }

    if (FindObjectsOfType<PartBehaviour>().Length > 0)
    {
      Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Saving data file ...");

      PartModel Data = new PartModel();

      PartBehaviour[] PartsAtSave = FindObjectsOfType<PartBehaviour>();

      foreach (PartBehaviour Prefab in PartsAtSave)
      {
        if (Prefab != null)
        {
          PartModel.SerializedPart DataTemp = new PartModel.SerializedPart();

          DataTemp.Id = Prefab.Id;
          DataTemp.uuid = Prefab.uuid;
          DataTemp.targetUuid = Prefab.targetUuid;
          DataTemp.AppearanceIndex = Prefab.AppearanceIndex;
          DataTemp.Position = PartModel.ParseToSerializedVector3(Prefab.transform.position);
          DataTemp.Rotation = PartModel.ParseToSerializedVector3(Prefab.transform.eulerAngles);
          DataTemp.Scale = PartModel.ParseToSerializedVector3(Prefab.transform.localScale);
          DataTemp.Properties = Prefab.ExtraProperties;
          Data.Prefabs.Add(DataTemp);

          SavedCount++;
        }
      }

      if (StorageType == StorageType.Desktop)
      {

        MemoryStream stream = new MemoryStream();
        BinaryFormatter Formatter = new BinaryFormatter();
        Formatter.Serialize(stream, Data);
        stream.Position = 0;
        StartCoroutine(TrainerService.UpdateTrainerData("test", JsonUtility.ToJson(Data)));

      }
      else
      {
        PlayerPrefs.SetString("EBS_Storage", JsonUtility.ToJson(Data));

        PlayerPrefs.Save();
      }

      Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Data file saved " + SavedCount + " Prefab(s).");

      EventHandlers.StorageSavingDone(PartsAtSave);

      yield break;
    }
  }

  private static void AssignTarget()
  {
    foreach (PartBehaviour PartA in FindObjectsOfType<PartBehaviour>())
    {
      foreach (PartBehaviour PartB in FindObjectsOfType<PartBehaviour>())
      {
        if (PartB.targetUuid == PartA.uuid)
        {
          TargetController[] tc = PartB.transform.GetComponentsInChildren<TargetController>();
          foreach (var target in tc)
          {
            target.target = PartA.transform;
          }
        }
      }
    }
  }

  #endregion Private Methods
}

