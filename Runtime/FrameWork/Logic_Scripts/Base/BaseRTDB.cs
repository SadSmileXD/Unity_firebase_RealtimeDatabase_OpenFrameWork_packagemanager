using Firebase.Database;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public enum RTDBType
{
    Get,
    Set,
    Update,
    Delete,
    Listen,
}
public interface IRTDB_Get { Task<T> RealtimeDatabase_Get<T>(); }
public interface IRTDB_Set { Task<bool> RealtimeDatabase_Set<T>(T data); }
public interface IRTDB_Update { Task<bool> RealtimeDatabase_Update<T>(T data); }
public interface IRTDB_Delete { Task<bool> RealtimeDatabase_Delete<T>(); }
public interface IRTDB_Listen { Task<bool> RealtimeDatabase_ValueChanageAddListen<T>(Action fun); }
[System.Serializable]
public abstract class BaseRTDB 
{
    public string KeyName;
    protected Dictionary<string, BaseRD> m_RealtimeDatabaseDictionary;
    protected Queue<Func<Task>> commandQueue = new Queue<Func<Task>>();
    protected FirebaseDatabase database;
    public virtual void init(Dictionary<string, BaseRD> m_dic, Queue<Func<Task>> m_queue)
    {
        m_RealtimeDatabaseDictionary = m_dic;
        commandQueue = m_queue;
        database = FirebaseDatabase.DefaultInstance;
    }

   
}
