using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class RTDB_Delete: BaseRTDB, IRTDB_Delete
{
    public override void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        base.init(m_dic, m_queue);
        this.KeyName = RTDBType.Delete.ToString();
    }

    public Task<bool> RealtimeDatabase_Delete<T>()
    {
        string classType = typeof(T).FullName;
        if (!m_RealtimeDatabaseDictionary.TryGetValue(classType, out var instance))
        {
            Debug.LogError($"{classType}에 해당하는 데이터 인스턴스를 찾을 수 없습니다.");
            return Task.FromResult(false);
        }
        var m_root = database.RootReference;
        foreach (string path in instance.Paths)
        {
            if (!string.IsNullOrEmpty(path)) m_root = m_root.Child(path);
        }
        commandQueue.Enqueue(async () =>
        {
            await m_root.SetValueAsync(null);
        });
        return Task.FromResult(true);
    }

  
}
