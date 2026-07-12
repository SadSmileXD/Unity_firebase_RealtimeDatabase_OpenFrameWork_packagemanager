using UnityEngine;
namespace SadSmileXD
{


public class Test22222 : MonoBehaviour
{
    public PlayerData2 testplayer;
    [ContextMenu("Test2")]
     public async void Test22()
     {
        testplayer= await RDManager.GetClassData<PlayerData2>();
     }
    [ContextMenu("Test1")]
    public async void Test1()
    {
        var player = new PlayerData2();
        player.names = "Test";
        player.age= 1;
        player.mdata= new PlayerData();
        player.mdata.names = "Sub Test";
        player.mdata.age = 30;

        await RDManager.SetClassData<PlayerData2>(player);
    }
    [ContextMenu("update")]
    public async void UpdateData()
    {
        if (testplayer != null)
        {
            var updatedata = new PlayerData2();
            updatedata.names = "Updated Test";
            updatedata.age = 2;
            await RDManager.UpdateClassData<PlayerData2>(updatedata);
        }
    }
    [ContextMenu("delete")]
    public async void DeleteData()
    {
        if (testplayer != null)
        {
            await RDManager.DeleteClassData<PlayerData2>();
        }
    }
}
}