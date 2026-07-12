using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class RTDB_Set : BaseRTDB, IRTDB_Set
{
    public override void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        base.init(m_dic, m_queue);
       this.KeyName= RTDBType.Set.ToString();
    }
    public  Task<bool> RealtimeDatabase_Set<T>(T data)
    {
        string classType = typeof(T).FullName;
        if (!m_RealtimeDatabaseDictionary.TryGetValue(classType, out var instance))
        {
            Debug.LogError($"{classType}에 해당하는 데이터 인스턴스를 찾을 수 없습니다.");
            return Task.FromResult(false);
        }

        // 경로 설정
        var m_root = instance.document;

        commandQueue.Enqueue(async () =>
        {
            string json = JsonConvert.SerializeObject(data);
            await m_root.SetRawJsonValueAsync(json);
            Debug.Log("구조적 저장 완료");
        });
       return Task.FromResult(true);
    }
}
