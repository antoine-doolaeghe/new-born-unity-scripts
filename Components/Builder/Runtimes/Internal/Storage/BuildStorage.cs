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

  public static BuildStorage Instance;

  public StorageType StorageType;

  public StorageSerializerType StorageSerializer = StorageSerializerType.Json;

  public bool AutoSave = false;

  public float AutoSaveInterval = 60f;

  public bool SavePrefabs = true;

  public bool LoadPrefabs = true;

  public string StorageOutputFile;

  [HideInInspector]
  public bool LoadedFile = false;

  #endregion Public Fields

  #region Private Fields

  private float TimerAutoSave;

  private List<PartBehaviour> PrefabsLoaded = new List<PartBehaviour>();

  private bool FileIsCorrupted;

  #endregion Private Fields

  #region Public Methods

  /// <summary>
  /// (Editor) This method allows to load a storage file in Editor scene.
  /// </summary>
  public void LoadInEditor(string path)
  {
    int PrefabLoaded = 0;
    PrefabsLoaded = new List<PartBehaviour>();

    BuildManager Manager = FindObjectOfType<BuildManager>();

    if (Manager == null)
    {
      Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : The BuildManager is not in the scene, please add it to load a file.");

      return;
    }

    BinaryFormatter Formatter = new BinaryFormatter();

    Formatter.Binder = new BinderFormatter();

    FileStream Stream = File.Open(path, FileMode.Open);

    PartModel Serializer = null;

    try
    {
      if (StorageSerializer == StorageSerializerType.Binary)
      {
        Serializer = (PartModel)Formatter.Deserialize(Stream);
      }
      else if (StorageSerializer == StorageSerializerType.Binary)
      {
        using (StreamReader Reader = new StreamReader(Stream))
        {
          Serializer = JsonUtility.FromJson<PartModel>(Reader.ReadToEnd());
        }
      }
    }
    catch
    {
      Stream.Close();

      Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : Please check that the file extension to load is correct.");

      return;
    }

    if (Serializer == null || Serializer.Prefabs == null)
    {
      Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The file is empty or the data are corrupted.");

      return;
    }

    GameObject Parent = new GameObject("(Editor) " + path, typeof(GroupBehaviour));

    foreach (PartModel.SerializedPart Data in Serializer.Prefabs)
    {
      if (Data != null)
      {
        PartBehaviour Prefab = Manager.GetPart(Data.Id);

        if (Prefab != null)
        {
          PartBehaviour PlacedPrefab = Manager.PlacePrefab(Prefab,
              PartModel.ParseToVector3(Data.Position),
              PartModel.ParseToVector3(Data.Rotation),
              PartModel.ParseToVector3(Data.Scale),
              Parent.transform, null);

          PlacedPrefab.transform.position = PartModel.ParseToVector3(Data.Position);
          PlacedPrefab.transform.eulerAngles = PartModel.ParseToVector3(Data.Rotation);
          PlacedPrefab.transform.localScale = PartModel.ParseToVector3(Data.Scale);

          PrefabsLoaded.Add(PlacedPrefab);

          PrefabLoaded++;
        }
        else
          Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The Prefab (" + Data.Id + ") does not exists in the Build Manager.");
      }
    }

    Stream.Close();

#if UNITY_EDITOR
    Selection.activeGameObject = Parent;

    if (SceneView.lastActiveSceneView != null)
      SceneView.lastActiveSceneView.FrameSelected();
#endif

    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Data file loaded " + PrefabLoaded + " Prefab(s) loaded in " + Time.realtimeSinceStartup.ToString("#.##") + " ms in the Editor scene.");

    PrefabsLoaded.Clear();
  }

  /// <summary>
  /// This method allows to load the storage file.
  /// </summary>
  public void LoadStorageFile()
  {
    StartCoroutine(LoadDataFile());
  }

  /// <summary>
  /// This method allows to save the storage file.
  /// </summary>
  public void SaveStorageFile()
  {
    StartCoroutine(SaveDataFile());
  }

  /// <summary>
  /// This method allows to delete the storage file.
  /// </summary>
  public void DeleteStorageFile()
  {
    StartCoroutine(DeleteDataFile());
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

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    // if (LoadPrefabs)
    // {
    StartCoroutine(transform.GetComponent<TrainerService>().GetObject("test"));
    // }


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
    StartCoroutine(transform.GetComponent<TrainerService>().GetObject("test"));
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

    // GameObject Parent = new GameObject("(Runtime) " + StorageOutputFile, typeof(GroupBehaviour))
    GameObject Parent = new GameObject("(Runtime) " + StorageOutputFile, typeof(GroupBehaviour));
    Parent.transform.parent = GameObject.Find("NewbornManager").transform;

    foreach (PartModel.SerializedPart Data in Serializer.Prefabs)
    {
      if (Data != null)
      {
        Debug.Log(BuildManager.Instance);

        PartBehaviour Prefab = BuildManager.Instance.GetPart(Data.Id);
        Prefab.uuid = Data.uuid;
        Prefab.targetUuid = Data.targetUuid;
        if (Prefab != null)
        {
          PartBehaviour PlacedPrefab = BuildManager.Instance.PlacePrefab(Prefab,
              PartModel.ParseToVector3(Data.Position),
              PartModel.ParseToVector3(Data.Rotation),
              PartModel.ParseToVector3(Data.Scale),
              Parent.transform, null);

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

    foreach (var part1 in FindObjectsOfType<PartBehaviour>())
    {
      foreach (var part2 in FindObjectsOfType<PartBehaviour>())
      {
        if (part2.targetUuid == part1.uuid)
        {
          TargetController[] tc = part2.transform.GetComponentsInChildren<TargetController>();
          foreach (var target in tc)
          {
            target.target = part1.transform;
          }
        }
      }
    }

    if (Stream != null)
      Stream.Close();

    Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : Data file loaded " + PrefabLoaded + " prefab(s) loaded in " + Time.realtimeSinceStartup.ToString("#.##") + " ms.");

    LoadedFile = true;

    EventHandlers.StorageLoadingDone(PrefabsLoaded.ToArray());

    /// LAUNCH THE NEWBORN FETCH

    yield break;

  }

  private IEnumerator SaveDataFile()
  {
    string filename = "test.bin";
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

  private IEnumerator DeleteDataFile()
  {
    if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
    {
      Debug.LogError("<b><color=cyan>[Easy Build System]</color></b> : Please define out file path.");

      yield break;
    }

    if (File.Exists(StorageOutputFile) == true)
    {
      foreach (PartBehaviour Prefab in PrefabsLoaded)
      {
        Destroy(Prefab.gameObject);
      }

      File.Delete(StorageOutputFile);

      Debug.Log("<b><color=cyan>[Easy Build System]</color></b> : The storage file has been removed.");
    }
    else
    {
      EventHandlers.StorageFailed("The file does not exists or the path is not found.");
    }

    EventHandlers.StorageDeleted();
  }
  public static byte[] ReadToEnd(System.IO.Stream stream)
  {
    long originalPosition = 0;

    if (stream.CanSeek)
    {
      originalPosition = stream.Position;
      stream.Position = 0;
    }

    try
    {
      byte[] readBuffer = new byte[4096];

      int totalBytesRead = 0;
      int bytesRead;

      while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
      {
        totalBytesRead += bytesRead;

        if (totalBytesRead == readBuffer.Length)
        {
          int nextByte = stream.ReadByte();
          if (nextByte != -1)
          {
            byte[] temp = new byte[readBuffer.Length * 2];
            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
            readBuffer = temp;
            totalBytesRead++;
          }
        }
      }

      byte[] buffer = readBuffer;
      if (readBuffer.Length != totalBytesRead)
      {
        buffer = new byte[totalBytesRead];
        Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
      }
      return buffer;
    }
    finally
    {
      if (stream.CanSeek)
      {
        stream.Position = originalPosition;
      }
    }
  }

  #endregion Private Methods
}

