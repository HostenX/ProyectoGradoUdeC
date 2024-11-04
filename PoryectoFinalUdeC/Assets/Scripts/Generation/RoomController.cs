using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomInfo
{
    public string name;
    public int X;
    public int Y;
}

public class RoomController : MonoBehaviour
{
    public static RoomController instance;

    string currentWorldName = "Basement";

    RoomInfo currentLoadRoomData;

    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    public List<Room> loadedRooms { get; } = new List<Room>();
    bool isLoadingRoom = false;
    bool allRoomsLoaded = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Inicializamos la cola de carga aquí si hay habitaciones predefinidas (opcional).
    }

    void Update()
    {
        UpdateRoomQueue();

        // Solo llamamos a UpdateAllRoomDoors cuando todas las habitaciones están cargadas y aún no hemos actualizado las puertas
        if (loadRoomQueue.Count == 0 && !isLoadingRoom && !allRoomsLoaded)
        {
            UpdateAllRoomDoors();
            allRoomsLoaded = true; // Marcamos que ya se han actualizado las puertas para no hacerlo de nuevo
        }
    }

    void UpdateRoomQueue()
    {
        if (isLoadingRoom || loadRoomQueue.Count == 0)
        {
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;

        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    public void LoadRoom(string name, int x, int y)
    {
        if (DoesRoomExist(x, y))
        {
            return;
        }
        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.X = x;
        newRoomData.Y = y;

        loadRoomQueue.Enqueue(newRoomData);
    }

    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        string roomName = currentWorldName + info.name;

        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);

        while (!loadRoom.isDone)
        {
            yield return null;
        }
    }

    public void RegisterRoom(Room room)
    {
        if (!DoesRoomExist(currentLoadRoomData.X, currentLoadRoomData.Y))
        {
            room.transform.position = new Vector3
            (
                currentLoadRoomData.X * room.width,
                currentLoadRoomData.Y * room.height,
                0
            );

            room.X = currentLoadRoomData.X;
            room.Y = currentLoadRoomData.Y;
            room.name = currentWorldName + "-" + currentLoadRoomData.name + " " + room.X + ", " + room.Y;
            room.transform.parent = transform;

            isLoadingRoom = false;

            loadedRooms.Add(room);
        }
        else
        {
            Destroy(room.gameObject);
            isLoadingRoom = false;
        }
    }

    public bool DoesRoomExist(int x, int y)
    {
        return loadedRooms.Find(item => item.X == x && item.Y == y) != null;
    }

    public Room FindRoom(int x, int y)
    {
        return loadedRooms.Find(item => item.X == x && item.Y == y);
    }

    public void UpdateAllRoomDoors()
    {
        foreach (Room room in loadedRooms)
        {
            room.RemoveOnConnectedDoors();
        }
    }
}
