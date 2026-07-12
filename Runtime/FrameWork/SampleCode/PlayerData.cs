
using System;
using System.Collections.Generic;
using UnityEngine;
namespace SadSmileXD
{
    [System.Serializable]
    public class PlayerData : BaseRD
    {
        public string names;
        public int age;
        public List<string> testString;
        public override void init()
        {
            m_classType = typeof(PlayerData).FullName;

            m_PathsList.Clear();
            m_PathsList.Add("user");
            m_PathsList.Add("Data");
            m_PathsList.Add("field");
            m_PathsList.Add("field2");

        }
    }
}
