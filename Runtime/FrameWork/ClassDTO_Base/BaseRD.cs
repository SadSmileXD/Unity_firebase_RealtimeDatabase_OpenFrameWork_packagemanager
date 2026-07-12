using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public abstract class BaseRD
{
    [JsonIgnore][SerializeField] protected List<string> m_PathsList = new();
    [JsonIgnore][SerializeField] protected string m_classType;

    // m_root는 외부에서 직접 수정하지 않도록 관리
    [JsonIgnore] protected DatabaseReference m_root;

    public List<string> Paths => m_PathsList;
    public string classType => m_classType;

    // document 프로퍼티 호출 시 경로가 없으면 자동 생성 (Lazy Loading)
    [JsonIgnore]
    public DatabaseReference document
    {
        get
        {
            if (m_root == null) SetDocument();
            return m_root;
        }
    }

    // 자식 클래스에서 데이터를 정의할 초기화 메서드
    public virtual void init() { }

    // Firebase 경로 설정
    public virtual void SetDocument()
    {
        // RDManager가 아직 초기화되지 않았다면 null 방지
        if (RDManager.database == null) return;

        m_root = RDManager.database.RootReference;
        foreach (var path in m_PathsList)
        {
            m_root = m_root.Child(path);
        }
    }
     
}