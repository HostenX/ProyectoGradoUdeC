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

        // Llamamos a UpdateAllRoomDoors aqu� despu�s de que todas las habitaciones se hayan cargado.
        RoomController.instance.UpdateAllRoomDoors();
        maxSpecialRoom = specialRoomTypes.Count();
    }

    private void SpawnRooms(IEnumerable<Vector2Int> rooms)
    {
        int specialRoomCount = 1;
        List<Vector2Int> potentialSpecialRooms = new List<Vector2Int>();
        List<Vector2Int> specialRoomLocations = new List<Vector2Int>();

        RoomController.instance.LoadRoom("Start", 0, 0);

        // Guardamos la �ltima habitaci�n como la ubicaci�n de la habitaci�n del jefe
        Vector2Int bossRoomLocation = dungeonRooms[dungeonRooms.Count - 1];

        foreach (Vector2Int roomLocation in rooms)
        {
            if (roomLocation == bossRoomLocation && roomLocation != Vector2Int.zero)
            {
                // Generamos la habitaci�n de jefe aqu�
                RoomController.instance.LoadRoom("End", roomLocation.x, roomLocation.y);
                indexBossRoom += 1;
            }
            else if (specialRoomCount <= maxSpecialRoom && roomLocation != Vector2Int.zero && Random.value < 0.5f && IsRoomFarEnough(roomLocation, specialRoomLocations))
            {
                // Seleccionamos aleatoriamente un tipo de habitaci�n especial
                string specialRoomType = specialRoomTypes[Random.Range(0, specialRoomTypes.Count)];
                RoomController.instance.LoadRoom(specialRoomType, roomLocation.x, roomLocation.y);
                specialRoomLocations.Add(roomLocation);
                specialRoomCount += 1;
            }
            else
            {
                potentialSpecialRooms.Add(roomLocation);
                RoomController.instance.LoadRoom("Empty", roomLocation.x, roomLocation.y);
            }
        }

        // Aseguramos que si no se gener� la habitaci�n "End" en el bucle anterior, la generamos manualmente
        if (!RoomController.instance.DoesRoomExist(bossRoomLocation.x, bossRoomLocation.y))
        {
            RoomController.instance.LoadRoom("End", bossRoomLocation.x, bossRoomLocation.y);
        }

        while (specialRoomCount <= maxSpecialRoom && potentialSpecialRooms.Count > 0)
        {
            Vector2Int selectedLocation = potentialSpecialRooms[Random.Range(0, potentialSpecialRooms.Count)];
            if (IsRoomFarEnough(selectedLocation, specialRoomLocations))
            {
                // Seleccionamos aleatoriamente un tipo de habitaci�n especial
                string specialRoomType = specialRoomTypes[Random.Range(0, specialRoomTypes.Count)];
                RoomController.instance.LoadRoom(specialRoomType, selectedLocation.x, selectedLocation.y);
                specialRoomLocations.Add(selectedLocation);
                specialRoomCount += 1;
            }
            potentialSpecialRooms.Remove(selectedLocation);
        }
    }

    // M�todo para verificar si una habitaci�n est� lo suficientemente lejos de las habitaciones especiales existentes
    private bool IsRoomFarEnough(Vector2Int roomLocation, List<Vector2Int> specialRoomLocations)
    {
        foreach (Vector2Int specialLocation in specialRoomLocations)
        {
            // Calculamos la distancia de Manhattan
            int distance = Mathf.Abs(roomLocation.x - specialLocation.x) + Mathf.Abs(roomLocation.y - specialLocation.y);
            if (distance <= 1)
            {
                return false; // La habitaci�n est� demasiado cerca
            }
        }
        return true; // La habitaci�n est� lo suficientemente lejos
    }
}
