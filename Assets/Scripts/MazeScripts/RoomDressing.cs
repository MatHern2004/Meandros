using System.Collections.Generic;
using UnityEngine;

public enum PropAnchor
{
    AgainstWalls, // places them allong the walls of the room facing into it
    Corners, // in the corners facing the center
    Scatter, // random spots inside the room
    Center //dead center, spawn points for shopkeeper/upgrades/boss would live here
}
[System.Serializable]
public class DressingProp
{
    public GameObject prefab;
    public PropAnchor anchor = PropAnchor.AgainstWalls;
    [Tooltip("How many to place. Capped by the number of available spots (e.g. 4 corners). Ignored for Center.")]
    [Min(1)] public int count = 1;
    [Tooltip("Rotate the model if it faces the wrong way (try 90, 180, 270).")]
    public float yawOffset = 0f;
    public float scale = 1f;
    [Tooltip("Gap from the wall surface (AgainstWalls) or from the cell edge (Corners/Scatter), in world units.")]
    public float inset = 0.5f;
}

/// "Treasure rooms get these props." Fill these in on the MazeBuilder in the Inspector.
[System.Serializable]
public class RoomDressing
{
    public RoomType type = RoomType.Treasure;
    public List<DressingProp> props = new List<DressingProp>();
}