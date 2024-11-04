using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum Doortype
    {
        left, right, top, bottom
    }

    public Doortype doorType;
}
