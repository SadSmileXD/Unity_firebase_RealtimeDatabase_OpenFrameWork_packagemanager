using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
[Serializable]
public class RTDB_Update : BaseRTDB, IRTDB_Update
{
    private static Dictionary<Type, FieldInfo[]> m_Reflection =new Dictionary<Type, FieldInfo[]>();  
    public override void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        base.init(m_dic, m_queue);
        m_Reflection = RDManager.Reflection;
        this.KeyName = RTDBType.Update.ToString();
    }
    public  Task<bool> RealtimeDatabase_Update<T>(T partialData)
    {
        string classType = typeof(T).FullName;
        if (!m_RealtimeDatabaseDictionary.TryGetValue(classType, out var instance))
        {
            Debug.LogError($"{classType}에 해당하는 데이터 인스턴스를 찾을 수 없습니다.");
            return Task.FromResult(false);
        }

        if (!m_Reflection.TryGetValue(typeof(T), out FieldInfo[] fields))
        {
            Debug.LogError("필드 정보를 찾을 수 없습니다.");
            return Task.FromResult(false);
        }

        var updateDict = new Dictionary<string, object>();

        foreach (var field in fields)
        {
            object value = field.GetValue(partialData);

            // null이면 스킵
            if (value == null)
                continue;

            // List 처리
            if (value is System.Collections.IList list)
            {
                if (list.Count > 0)
                {
                    updateDict[field.Name] = value;
                }
                continue;
            }

            Type valueType = value.GetType();

            // string 제외 사용자 정의 클래스 처리
            if (valueType.IsClass && valueType != typeof(string))
            {

                var jObject = JObject.FromObject(value);
                var dict = jObject.ToObject<Dictionary<string, object>>();
                updateDict[field.Name] = dict;

            }
            else
            {
                // int, float, bool, enum 등
                updateDict[field.Name] = value;
            }
        }

        if (updateDict.Count == 0)
            return Task.FromResult(false);

        var m_root = instance.document;
        
        commandQueue.Enqueue(async () =>
        {
            await m_root.UpdateChildrenAsync(updateDict);
            Debug.Log($"{classType} 업데이트 완료");
        });
        return Task.FromResult(true);
    }
}
