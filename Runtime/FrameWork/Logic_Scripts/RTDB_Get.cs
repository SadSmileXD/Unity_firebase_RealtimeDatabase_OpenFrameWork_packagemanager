using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class RTDB_Get : BaseRTDB, IRTDB_Get
{
    public override void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        base.init(m_dic, m_queue);
        this.KeyName = RTDBType.Get.ToString();
    }
    public  async Task<T> RealtimeDatabase_Get<T>()
    {
        string classType = typeof(T).FullName;
        if (!m_RealtimeDatabaseDictionary.TryGetValue(classType, out var instance))
        {
            Debug.LogError($"{classType} 인스턴스를 찾을 수 없습니다.");
            return default;
        }

        var m_root = database.RootReference;
        foreach (string path in instance.Paths)
        {
            if (!string.IsNullOrEmpty(path))
                m_root = m_root.Child(path);
        }

        DataSnapshot snapshot = await m_root.GetValueAsync();

        if (snapshot == null || !snapshot.Exists)
        {
            Debug.LogWarning("데이터가 없습니다.");
            return default;
        }

        string json = snapshot.GetRawJsonValue();

        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Json 역직렬화 실패\n{e}");
            return default;
        }
    }
}
