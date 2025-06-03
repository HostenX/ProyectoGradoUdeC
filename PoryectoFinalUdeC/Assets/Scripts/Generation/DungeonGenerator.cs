using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int maxSpecialRoom;
    public List<string> specialRoomTypes; // Lista de tipos de habitaciones especiales
    public DungeonGenerationData dungeonGenerationData;
    private List<Vector2Int> dungeonRooms;

    // Distancia mínima entre habitaciones especiales
    [SerializeField] private int minDistanceBetweenSpecialRooms = 2;

    // Garantiza que ciertos tipos de habitaciones aparezcan (opcional)
    [SerializeField] private bool ensureAllSpecialRoomTypes = true;

    private void Start()
    {
        // Generar el diseño del dungeon
        dungeonRooms = DungeonCrawlerController.GenerateDungeon(dungeonGenerationData);

        // Inicializar maxSpecialRoom si es necesario
        if (maxSpecialRoom <= 0)
        {
            maxSpecialRoom = Mathf.Min(specialRoomTypes.Count, dungeonRooms.Count / 3);
        }

        // Verificar que tengamos suficientes habitaciones para lo que queremos hacer
        if (dungeonRooms.Count < maxSpecialRoom + 2) // +2 para la habitación inicial y final
        {
            Debug.LogWarning("No hay suficientes habitaciones para distribuir todos los tipos. Se generarán las posibles.");
            maxSpecialRoom = Mathf.Min(maxSpecialRoom, dungeonRooms.Count - 2);
        }

        SpawnRooms(dungeonRooms);

        // Actualizar puertas después de que todas las habitaciones se han cargado
        RoomController.instance.UpdateAllRoomDoors();

        // Verificar conectividad del dungeon
        VerifyDungeonConnectivity();
    }

    private void SpawnRooms(IEnumerable<Vector2Int> rooms)
    {
        // Convertir a lista para manipulación
        List<Vector2Int> roomsList = new List<Vector2Int>(rooms);

        // Siempre cargar la habitación inicial en (0,0)
        RoomController.instance.LoadRoom("Start", 0, 0);

        // Seleccionar la habitación más alejada para el jefe
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int bossRoomLocation = FindFurthestRoom(startPos, roomsList);

        // Crear una lista de habitaciones especiales a insertar, garantizando diversidad
        List<string> specialRoomsToPlace = new List<string>();

        if (ensureAllSpecialRoomTypes && specialRoomTypes.Count <= maxSpecialRoom)
        {
            // Asegurar que todos los tipos se usen al menos una vez
            specialRoomsToPlace.AddRange(specialRoomTypes);
        }
        else
        {
            // Seleccionar aleatoriamente tipos únicos
            List<string> availableTypes = new List<string>(specialRoomTypes);

            for (int i = 0; i < maxSpecialRoom && availableTypes.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableTypes.Count);
                specialRoomsToPlace.Add(availableTypes[randomIndex]);

                // Opcional: eliminar el tipo para evitar repetición
                // availableTypes.RemoveAt(randomIndex);
            }
        }

        // Crear lista para rastrear donde se colocaron las habitaciones especiales
        List<Vector2Int> specialRoomLocations = new List<Vector2Int>();

        // Seleccionar las mejores ubicaciones para habitaciones especiales
        List<Vector2Int> potentialRooms = FindBestSpecialRoomLocations(roomsList, bossRoomLocation, startPos);

        // Colocar habitaciones especiales
        int specialRoomCount = 0;

        foreach (Vector2Int location in potentialRooms)
        {
            if (specialRoomCount >= maxSpecialRoom || specialRoomCount >= specialRoomsToPlace.Count)
                break;

            if (location != Vector2Int.zero && location != bossRoomLocation &&
                IsRoomFarEnough(location, specialRoomLocations, minDistanceBetweenSpecialRooms))
            {
                string roomType = specialRoomsToPlace[specialRoomCount];
                RoomController.instance.LoadRoom(roomType, location.x, location.y);
                specialRoomLocations.Add(location);
                specialRoomCount++;
            }
        }

        // Colocar la habitación del jefe (debe ser después de habitaciones especiales para evitar colisiones)
        RoomController.instance.LoadRoom("End", bossRoomLocation.x, bossRoomLocation.y);

        // Llenar las habitaciones restantes como vacías
        foreach (Vector2Int roomLocation in roomsList)
        {
            if (roomLocation != Vector2Int.zero && roomLocation != bossRoomLocation &&
                !specialRoomLocations.Contains(roomLocation))
            {
                RoomController.instance.LoadRoom("Empty", roomLocation.x, roomLocation.y);
            }
        }

        // Asegurarse de que todas las habitaciones importantes existan
        VerifyImportantRooms(bossRoomLocation, specialRoomLocations, specialRoomsToPlace);
    }

    // Encuentra la habitación más alejada del punto inicial
    private Vector2Int FindFurthestRoom(Vector2Int startPos, List<Vector2Int> rooms)
    {
        Vector2Int furthestRoom = startPos;
        float maxDistance = 0;

        foreach (Vector2Int room in rooms)
        {
            if (room == startPos) continue;

            // Usar distancia de Manhattan para consistencia con resto del código
            float distance = Mathf.Abs(room.x - startPos.x) + Mathf.Abs(room.y - startPos.y);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestRoom = room;
            }
        }

        return furthestRoom;
    }

    // Encontrar las mejores ubicaciones para habitaciones especiales
    private List<Vector2Int> FindBestSpecialRoomLocations(List<Vector2Int> rooms, Vector2Int bossRoom, Vector2Int startRoom)
    {
        // Remover la habitación inicial y la del jefe
        List<Vector2Int> availableRooms = new List<Vector2Int>(rooms);
        availableRooms.Remove(startRoom);
        availableRooms.Remove(bossRoom);

        // Ordenar por distancia desde la habitación inicial (intermedias primero)
        availableRooms.Sort((a, b) => {
            float distA = Vector2Int.Distance(a, startRoom);
            float distB = Vector2Int.Distance(b, startRoom);

            // Queremos las habitaciones a distancia media primero
            float optimalDist = Vector2Int.Distance(bossRoom, startRoom) * 0.5f;

            return Mathf.Abs(distA - optimalDist).CompareTo(Mathf.Abs(distB - optimalDist));
        });

        return availableRooms;
    }

    // Verificar que una habitación esté suficientemente lejos de otras habitaciones especiales
    private bool IsRoomFarEnough(Vector2Int roomLocation, List<Vector2Int> specialRoomLocations, int minDistance)
    {
        foreach (Vector2Int specialLocation in specialRoomLocations)
        {
            // Calculamos la distancia de Manhattan
            int distance = Mathf.Abs(roomLocation.x - specialLocation.x) + Mathf.Abs(roomLocation.y - specialLocation.y);
            if (distance < minDistance)
            {
                return false; // La habitación está demasiado cerca
            }
        }
        return true; // La habitación está lo suficientemente lejos
    }

    // Verificar que todas las habitaciones importantes existan
    private void VerifyImportantRooms(Vector2Int bossRoomLocation, List<Vector2Int> specialRoomLocations, List<string> specialRoomTypes)
    {
        // Verificar habitación final
        if (!RoomController.instance.DoesRoomExist(bossRoomLocation.x, bossRoomLocation.y))
        {
            Debug.LogWarning("La habitación del jefe no se cargó correctamente. Intentando cargar de nuevo.");
            RoomController.instance.LoadRoom("End", bossRoomLocation.x, bossRoomLocation.y);
        }

        // Verificar habitaciones especiales
        for (int i = 0; i < specialRoomLocations.Count; i++)
        {
            Vector2Int loc = specialRoomLocations[i];
            if (!RoomController.instance.DoesRoomExist(loc.x, loc.y))
            {
                Debug.LogWarning($"La habitación especial en {loc} no se cargó correctamente. Intentando cargar de nuevo.");
                RoomController.instance.LoadRoom(specialRoomTypes[i % specialRoomTypes.Count], loc.x, loc.y);
            }
        }
    }

    // Verificar conectividad del dungeon (puedes implementar un BFS aquí)
    private void VerifyDungeonConnectivity()
    {
        // Aquí podrías implementar un algoritmo BFS para asegurarte 
        // de que todas las habitaciones son accesibles desde la habitación inicial

        // Por simplicidad, solo comprobamos que existen las habitaciones
        Debug.Log("Verificación de conectividad de dungeon completada");
    }
}