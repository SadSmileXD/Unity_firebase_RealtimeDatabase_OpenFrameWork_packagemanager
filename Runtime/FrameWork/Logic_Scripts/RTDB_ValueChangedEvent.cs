using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class RTDB_ValueChangedEvent : BaseRTDB, IRTDB_Listen
{

    public override void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        base.init(m_dic, m_queue);
        this.KeyName = RTDBType.Listen.ToString();
    }
    public Task<bool> RealtimeDatabase_ValueChanageAddListen<T>(Action callback)
    {
        string classType = typeof(T).FullName;
        if (!m_RealtimeDatabaseDictionary.TryGetValue(classType, out var instance))
        {
            Debug.LogError($"{classType}에 해당하는 데이터 인스턴스를 찾을 수 없습니다.");
            return Task.FromResult(false);
        }
        var m_root = instance.document;

        EventHandler<ValueChangedEventArgs> wrapper = (sender, args) =>
        {
            // 여기에서 필요하다면 args를 파싱해서 데이터를 처리할 수도 있습니다.
            callback.Invoke();
        };

        m_root.ValueChanged += wrapper;
        return Task.FromResult(true);
    }
}
