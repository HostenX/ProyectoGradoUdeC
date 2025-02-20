using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public int width;
    public int height;
    public int X;
    public int Y;
    public Door leftDoor;
    public Door rightDoor;
    public Door topDoor;
    public Door bottomDoor;

    public List<Door> doors = new List<Door>();

    // Start is called before the first frame update
    void Start()
    {
        if(RoomController.instance == null)
        {
            UnityEngine.Debug.Log("You pressed play in the wrong scene!!");
            return;
        }

        Door[]ds = GetComponentsInChildren<Door>();
        foreach (Door d in ds)
        {
            
            switch(d.doorType)
            {
                case Door.Doortype.right:
                    rightDoor = d; 
                    break;
                case Door.Doortype.left:
                    leftDoor = d; 
                    break;
                case Door.Doortype.top:
                    topDoor = d;
                    break;
                case Door.Doortype.bottom:
                    bottomDoor = d;
                    break;
            }
            doors.Add(d);
        }

        RoomController.instance.RegisterRoom(this);
    }

    public void RemoveOnConnectedDoors()
    {
        foreach (Door door in doors)
        {
            bool isConnected = false;

            switch (door.doorType)
            {
                case Door.Doortype.right:
                    isConnected = GetRight() != null;
                    break;
                case Door.Doortype.left:
                    isConnected = GetLeft() != null;
                    break;
                case Door.Doortype.top:
                    isConnected = GetTop() != null;
                    break;
                case Door.Doortype.bottom:
                    isConnected = GetBottom() != null;
                    break;
            }

            SetDoorState(door, isConnected);

            // Si la puerta no está conectada, también desactivamos los TilemapRenderer de los hijos
            if (!isConnected)
            {
                DisableChildTilemaps(door);
            }
        }
    }

    private void SetDoorState(Door door, bool isOpen)
    {
        BoxCollider2D collider = door.GetComponent<BoxCollider2D>();
        TilemapRenderer tilemapRenderer = door.GetComponent<TilemapRenderer>();

        // Desactiva el TilemapRenderer si no está abierto
        if (tilemapRenderer != null)
        {
            tilemapRenderer.enabled = isOpen;
        }

        // El colisionador solo se activa si la puerta está abierta (conectada)
        if (collider != null)
        {
            collider.enabled = !isOpen;
        }
    }

    // Deshabilita todos los TilemapRenderer de los hijos de la puerta
    private void DisableChildTilemaps(Door door)
    {
        TilemapRenderer[] childTilemaps = door.GetComponentsInChildren<TilemapRenderer>();
        foreach (TilemapRenderer childTilemap in childTilemaps)
        {
            childTilemap.enabled = false;
        }
    }




    public Room GetRight()
    {
        if (RoomController.instance.DoesRoomExist(X + 1, Y))
        {
            return RoomController.instance.FindRoom(X + 1, Y);
        }
        return null;
    }
    public Room GetLeft()
    {
        if (RoomController.instance.DoesRoomExist(X - 1, Y))
        {
            return RoomController.instance.FindRoom(X - 1, Y);
        }
        return null;
    }
    public Room GetTop()
    {
        if (RoomController.instance.DoesRoomExist(X, Y + 1))
        {
            return RoomController.instance.FindRoom(X, Y + 1);
        }
        return null;
    }
    public Room GetBottom()
    {
        if (RoomController.instance.DoesRoomExist(X, Y - 1))
        {
            return RoomController.instance.FindRoom(X, Y - 1);
        }
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0));
    }

    public Vector3 GetRoomCentre()
    {
        return new Vector3(X*width, Y*height);
    }

}
