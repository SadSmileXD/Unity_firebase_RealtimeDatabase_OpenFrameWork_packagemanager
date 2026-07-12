
using System;
using System.Collections.Generic;
using UnityEngine;
namespace SadSmileXD
{ 
[System.Serializable]
public class PlayerData2 : BaseRD
{
    public string names;
    public int age;
    public List<string> testString = new();
    public PlayerData mdata;
    public override void init()
    {
        m_classType = typeof(PlayerData2).FullName;

        m_PathsList.Clear();
        m_PathsList.Add("user");
        m_PathsList.Add("Data");
        m_PathsList.Add("field");
        m_PathsList.Add("field3");

    }
}
}