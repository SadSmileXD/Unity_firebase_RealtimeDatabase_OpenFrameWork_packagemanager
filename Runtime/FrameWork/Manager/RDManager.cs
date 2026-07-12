using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;


public class RDManager : MonoBehaviour
{
    public static RDManager Instance { get; private set; }

    protected static Dictionary<string, BaseRD> RealtimeDatabaseDictionary = new Dictionary<string, BaseRD>();
    protected static Dictionary<string, BaseRTDB> RealtimeDatabaseLogic = new Dictionary<string, BaseRTDB>();
    public static Dictionary<Type, FieldInfo[]> Reflection = new Dictionary<Type, FieldInfo[]>();
    public static FirebaseDatabase database;
    protected static Queue<Func<Task>> commandQueue = new Queue<Func<Task>>();

    protected static bool _isProcessing = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public async Task init(FirebaseDatabase _FirebaseDatabase)
    {
        database = _FirebaseDatabase;
        try
        {
          await Initialize_Dic();
          await Initialize_Logic_Dic();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    protected Task Initialize_Dic()
    {
        var stateTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(BaseRD).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in stateTypes)
        {
            var fields = type.GetFields();
            // 1. 객체 생성
            BaseRD instance = (BaseRD)System.Activator.CreateInstance(type);

            // 2. 초기화 (각 클래스에서 정의한 로직 실행)
            instance.init();

            // 3. 딕셔너리에 추가 (Key를 클래스 이름으로 설정)
            RealtimeDatabaseDictionary.Add(instance.classType, instance);
            //리플랙션 저장용
            Reflection.Add(type, fields);
           
        }
        return Task.CompletedTask;
    }
    protected Task Initialize_Logic_Dic()
    {
        var stateTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(BaseRTDB).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in stateTypes)
        {
            var fields = type.GetFields();
            // 1. 객체 생성
            var instance = (BaseRTDB)System.Activator.CreateInstance(type);

            // 2. 초기화 (각 클래스에서 정의한 로직 실행)
            instance.init(RealtimeDatabaseDictionary, commandQueue);

            // 3. 딕셔너리에 추가 (Key를 클래스 이름으로 설정)
            RealtimeDatabaseLogic.Add(instance.KeyName, instance);
            //리플랙션 저장용
            Reflection.Add(type, fields);
           
        }
        return Task.CompletedTask;
    }
    protected static async Task<bool> ProcessQueueAsync()
    {
        // 이미 처리 중이면 중복 실행 방지 
        if (_isProcessing) return false;
        _isProcessing = true;

        // 큐에 데이터가 1개 이상 있으면 전부 꺼내서 처리
        while (commandQueue.Count > 0)
        {
            var taskFunc = commandQueue.Dequeue();
            try
            {
                // 각 작업을 순차적으로 실행
                await taskFunc();
            }
            catch (Exception ex)
            {
                Debug.LogError($"큐 작업 수행 중 오류 발생: {ex.Message}");
            }
        }

        _isProcessing = false;
        return true;
    }
    public static async Task<T> GetClassData<T>() where T : BaseRD
    {
        // 1. 딕셔너리에서 로직 객체를 가져옴
        if (RealtimeDatabaseLogic.TryGetValue(RTDBType.Get.ToString(), out var logic) && logic is IRTDB_Get getter)
        {
                try
                {
                    // 3. 인터페이스를 통해 안전하게 호출
                    return await getter.RealtimeDatabase_Get<T>();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"데이터 조회 중 오류 발생: {ex.Message}");
                }
        }
        return null;
    }
    public static async Task<bool> SetClassData<T>(T Data) where T : BaseRD
    {
        if (RealtimeDatabaseLogic.TryGetValue(RTDBType.Set.ToString(), out var logic) && logic is IRTDB_Set setter)
        {
            try
            {
                var flag = await setter.RealtimeDatabase_Set<T>(Data);
                if (!flag) { Debug.LogError($"{typeof(T).FullName} 데이터 설정 실패"); return false; }
                var Successflag= await ProcessQueueAsync();
                if (!Successflag) throw new Exception("큐 처리 실패");
                return true;
            }
            catch (Exception ex) { Debug.LogError($"SetClassData 오류: {ex.Message}"); }
        }
        return false;
    }
    public static async Task<bool> UpdateClassData<T>(T partialData) where T : BaseRD
    {
        if (RealtimeDatabaseLogic.TryGetValue(RTDBType.Update.ToString(), out var logic) && logic is IRTDB_Update updater)
        {
            try
            {
                await updater.RealtimeDatabase_Update<T>(partialData);
                _ = ProcessQueueAsync();
                return true;
            }
            catch (Exception ex) { Debug.LogError($"UpdateClassData 오류: {ex.Message}"); }
        }
        return false;
    }
    public static async Task DeleteClassData<T>() where T : BaseRD
    {
        if (RealtimeDatabaseLogic.TryGetValue(RTDBType.Delete.ToString(), out var logic) && logic is IRTDB_Delete deleter)
        {
            try
            {
                await deleter.RealtimeDatabase_Delete<T>();
                var Successflag = await ProcessQueueAsync();
                if (!Successflag) throw new Exception("큐 처리 실패");
            }
            catch (Exception ex) { Debug.LogError($"DeleteClassData 오류: {ex.Message}"); }
        }
    }
    public static async Task ValueChangedEvent<T>(Action callback) where T : BaseRD
    {
        if (RealtimeDatabaseLogic.TryGetValue(RTDBType.Listen.ToString(), out var logic) && logic is IRTDB_Listen listener)
        {
            try
            {
                await listener.RealtimeDatabase_ValueChanageAddListen<T>(callback);
            }
            catch (Exception ex) { Debug.LogError($"ValueChangedEvent 오류: {ex.Message}"); }
        }
    }
}
/*
Assembly.GetExecutingAssembly()
현재 코드가 실행되고 있는 어셈블리(쉽게 말해 현재 프로젝트의 컴파일된 코드 파일 전체)를 가져옵니다.
///
.GetTypes()
해당 어셈블리 안에 정의된 모든 클래스, 인터페이스, 구조체 등 모든 타입의 목록을 배열 형태로 가져옵니다.
///
.Where(...) (필터링 조건)
가져온 모든 타입 중에서 우리가 원하는 것들만 추려내는 작업입니다.

typeof(BaseRD).IsAssignableFrom(t): 
타입 t가 BaseRD를 상속받았거나(또는 구현했거나), 
자기 자신(BaseRD)인지 확인합니다. 즉, "BaseRD의 자식 클래스인가?"를 묻는 것입니다.
///
!t.IsInterface: 인터페이스는 인스턴스를 만들 수 없으므로 제외합니다.
///
!t.IsAbstract: 추상 클래스는 직접 인스턴스화할 수 없으므로 제외합니다. (상속용으로만 존재하는 클래스 필터링)


*/

