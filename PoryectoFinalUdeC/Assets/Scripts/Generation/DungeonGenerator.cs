using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int maxSpecialRoom;
    public List<string> specialRoomTypes; // Lista de tipos de habitaciones especiales
    private int indexBossRoom = 0;
    public DungeonGenerationData dungeonGenerationData;
    private List<Vector2Int> dungeonRooms;

    private void Start()
    {
        dungeonRooms = DungeonCrawlerController.GenerateDungeon(dungeonGenerationData);
        SpawnRooms(dungeonRooms);

        // Llamamos a UpdateAllRoomDoors aquí después de que todas las habitaciones se hayan cargado.
        RoomController.instance.UpdateAllRoomDoors();
        maxSpecialRoom = specialRoomTypes.Count();
    }

    private void SpawnRooms(IEnumerable<Vector2Int> rooms)
    {
        int specialRoomCount = 1;
        List<Vector2Int> potentialSpecialRooms = new List<Vector2Int>();
        List<Vector2Int> specialRoomLocations = new List<Vector2Int>();
        List<Vector2Int> emptyRooms = new List<Vector2Int>();

        RoomController.instance.LoadRoom("Start", 0, 0);
        Vector2Int bossRoomLocation = dungeonRooms[dungeonRooms.Count - 1];

        foreach (Vector2Int roomLocation in rooms)
        {
            if (roomLocation == bossRoomLocation && roomLocation != Vector2Int.zero)
            {
                RoomController.instance.LoadRoom("End", roomLocation.x, roomLocation.y);
            }
            else if (specialRoomCount <= maxSpecialRoom && roomLocation != Vector2Int.zero && Random.value < 0.5f && IsRoomFarEnough(roomLocation, specialRoomLocations))
            {
                string specialRoomType = specialRoomTypes[Random.Range(0, specialRoomTypes.Count)];
                RoomController.instance.LoadRoom(specialRoomType, roomLocation.x, roomLocation.y);
                specialRoomLocations.Add(roomLocation);
                specialRoomCount++;
            }
            else
            {
                emptyRooms.Add(roomLocation);
                RoomController.instance.LoadRoom("Empty", roomLocation.x, roomLocation.y);
            }
        }

        // Asegurarse de que la habitación del jefe exista
        if (!RoomController.instance.DoesRoomExist(bossRoomLocation.x, bossRoomLocation.y))
        {
            RoomController.instance.LoadRoom("End", bossRoomLocation.x, bossRoomLocation.y);
        }

        // Ahora intentamos reemplazar habitaciones vacías con habitaciones especiales
        ConvertEmptyRoomsToSpecial(emptyRooms, ref specialRoomCount, specialRoomLocations);
    }

    private void ConvertEmptyRoomsToSpecial(List<Vector2Int> emptyRooms, ref int specialRoomCount, List<Vector2Int> specialRoomLocations)
    {
        List<Vector2Int> roomsToConvert = new List<Vector2Int>(emptyRooms); // Copia para evitar modificar la lista mientras iteramos

        foreach (Vector2Int selectedLocation in roomsToConvert)
        {
            if (specialRoomCount >= maxSpecialRoom) break;

            string specialRoomType = specialRoomTypes[Random.Range(0, specialRoomTypes.Count)];

            if (RoomController.instance.DoesRoomExist(selectedLocation.x, selectedLocation.y))
            {
                RoomController.instance.ReplaceRoom(selectedLocation.x, selectedLocation.y, specialRoomType);
                specialRoomLocations.Add(selectedLocation);
                specialRoomCount++; // Incrementamos para evitar bucles infinitos
            }

            emptyRooms.Remove(selectedLocation); // Eliminamos de la lista de vacías
        }
    }

    // Método para eliminar habitaciones que no se cargaron
    private void RemoveUnloadedRooms(List<Vector2Int> roomList)
    {
        roomList.RemoveAll(room => !RoomController.instance.DoesRoomExist(room.x, room.y));
    }

    // Método para verificar si una habitación está lo suficientemente lejos de las habitaciones especiales existentes
    private bool IsRoomFarEnough(Vector2Int roomLocation, List<Vector2Int> specialRoomLocations)
    {
        foreach (Vector2Int specialLocation in specialRoomLocations)
        {
            // Calculamos la distancia de Manhattan
            int distance = Mathf.Abs(roomLocation.x - specialLocation.x) + Mathf.Abs(roomLocation.y - specialLocation.y);
            if (distance <= 1)
            {
                return false; // La habitación está demasiado cerca
            }
        }
        return true; // La habitación está lo suficientemente lejos
    }
}
