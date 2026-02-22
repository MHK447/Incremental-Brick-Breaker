using System;
using System.IO;
using System.Text;
using Firebase.Firestore;
using Firebase.Extensions;

[FirestoreData]
public struct FirebaseUserData
{
    [FirestoreProperty]
    public byte[] binary { get; set; }
    [FirestoreProperty]
    public long saveDate { get; set; }
    [FirestoreProperty]
    public string userid { get; set; }
    [FirestoreProperty]
    public string binary_str { get; set; }
}

public class TpFirebaseDataProp
{
    public static readonly string dbPath = "USERDATA/";

    public void Init()
    {
        var firestore = FirebaseFirestore.DefaultInstance;
        firestore.Settings.PersistenceEnabled = false;
    }

    public void UpLoadData(System.Action<bool> OnEnd)
    {
        var firestore = FirebaseFirestore.DefaultInstance;
        if (TpPlatformLoginProp.fUser != null)
        {
            byte[] saveData = null;
            var filePath = GameRoot.Instance.UserData.GetSaveFilePath();

            if (File.Exists(filePath))
            {
                saveData = File.ReadAllBytes(filePath);
            }

            if (saveData == null)
                return;

            BpLog.Log($"binary str : {Convert.ToBase64String(saveData)}");

            var userData = new FirebaseUserData()
            {
                binary = saveData,
                saveDate = TimeSystem.GetCurTime().Ticks,
                userid = TpPlatformLoginProp.fUser.UserId,
                binary_str = Convert.ToBase64String(saveData)
            };

            firestore.Document($"{dbPath}{TpPlatformLoginProp.fUser.UserId}")
            .SetAsync(userData).ContinueWithOnMainThread(task =>
            {
                UnityEngine.Assertions.Assert.IsNull(task.Exception);
                OnEnd?.Invoke(task.IsCompletedSuccessfully);
            }); ;
        }
    }

    public void GetSaveDate(Action<long> OnReturn)
    {
        var firestore = FirebaseFirestore.DefaultInstance;
        if (TpPlatformLoginProp.fUser != null)
        {
            firestore.Document($"{dbPath}{TpPlatformLoginProp.fUser.UserId}")
                        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
                        {
                            UnityEngine.Assertions.Assert.IsNull(task.Exception);

                            if (task.Result.Exists)
                            {
                                var userData = task.Result.ConvertTo<FirebaseUserData>();
                                OnReturn?.Invoke(userData.saveDate);
                            }
                            else
                            {
                                OnReturn?.Invoke(0);
                            }
                        });
        }
    }

    public void DownLoadData(Action OnEnd)
    {
        var firestore = FirebaseFirestore.DefaultInstance;
        if (TpPlatformLoginProp.fUser != null)
        {
            firestore.Document($"{dbPath}{TpPlatformLoginProp.fUser.UserId}")
                        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
                        {
                            UnityEngine.Assertions.Assert.IsNull(task.Exception);

                            if (task.Result.Exists)
                            {
                                var userData = task.Result.ConvertTo<FirebaseUserData>();

                                var filePath = GameRoot.Instance.UserData.GetBackUpSaveFilePath("backup");
                                if (string.IsNullOrEmpty(userData.binary_str))
                                {
                                    File.WriteAllBytes(filePath, userData.binary);
                                }
                                else
                                {
                                    File.WriteAllBytes(filePath, Convert.FromBase64String(userData.binary_str));
                                }
                            }
                            OnEnd?.Invoke();
                        });

        }
    }

    public void EditorDownLoadData(Action OnEnd)
    {
#if UNITY_EDITOR
        var binary = "OAAAAAAAMgBMAAAAKAAUACAAHAAYAAAAAAAAAAwAEAAIAAAAOAA8ADQAAABAAAQARAAAAAAASAAyAAAACgAAAAEAAAAGAAAAwAMAAEAAAACIAgAAoAMAAOC28C4bYN4IsAMAAGgAAAB4AAAAeAEAAKABAACoAQAAuAEAANABAABQAgAAAQAAADAAAAAFAAAANAAAACgAAAAcAAAAEAAAAAQAAAABAAAANgAAAAEAAAA1AAAAAQAAADQAAAABAAAAMwAAAAEAAAAyAAAA6Pz//wABAQEEAAAAAgAAAGVuAAAHAAAA6AAAAMQAAACgAAAAcAAAAEwAAAAoAAAABAAAADz9//8IAAAAAQAAAA0AAABTdGFnZVJld2FyZF82AAAAXP3//wgAAAABAAAADQAAAFN0YWdlUmV3YXJkXzUAAAB8/f//CAAAAAEAAAANAAAAU3RhZ2VSZXdhcmRfNAAAAJz9//8IAAAAAQAAABoAAABOZXdFbmVteUJsb2NrU3Bhd25lclJld2FyZAAAyP3//wgAAAABAAAADQAAAFN0YWdlUmV3YXJkXzMAAADo/f//CAAAAAEAAAANAAAAU3RhZ2VSZXdhcmRfMgAAAAj+//8IAAAAAQAAAAQAAABJbml0AAAAACD+//8cAAAABAAAAAEAAAAEAAAANP7//wEAAAABAAAAAQAAAAEAAAAEAAQABAAAAAgACgAAAAQACAAAAAQAAAAAAAoAEAAEAAgADAAKAAAABgAAAAAAAAAEAAAABgAAAFBsYXllcgAABwAAAGwAAABUAAAARAAAADQAAAAkAAAAFAAAAAQAAACo/v//awAAAAEAAAC0/v//bAAAAAIAAADA/v//AgAAAAEAAADM/v//ZQAAAAEAAADY/v//ZwAAAAIAAADk/v//bQAAAAEAAAAIAA4ABAAIAAgAAAABAAAAAQAAAAAABgAIAAQABgAAAAEAAAAHAAAA8AAAAMQAAACgAAAAcAAAAEwAAAAoAAAABAAAADT///8IAAAAAQAAAA0AAABTdGFnZVJld2FyZF82AAAAVP///wgAAAABAAAADQAAAFN0YWdlUmV3YXJkXzUAAAB0////CAAAAAEAAAANAAAAU3RhZ2VSZXdhcmRfNAAAAJT///8IAAAAAQAAABoAAABOZXdFbmVteUJsb2NrU3Bhd25lclJld2FyZAAAwP///wgAAAABAAAADQAAAFN0YWdlUmV3YXJkXzMAAADg////CAAAAAEAAAANAAAAU3RhZ2VSZXdhcmRfMgAAAAgADAAEAAgACAAAAAgAAAABAAAABAAAAEluaXQAAAAAEAAMAAgABQAGAAAAAAAHABAAAAAAAQEBBAAAAAIAAABlbgAAAQAAADAAAAAAAAAAAAAAAA==";
        var filePath = GameRoot.Instance.UserData.GetBackUpSaveFilePath("backup");
        File.WriteAllBytes(filePath, Convert.FromBase64String(binary));

        OnEnd?.Invoke();
#endif
    }
}
