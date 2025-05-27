using UnityEngine;
using System.Collections.Generic;

public enum ObstacleType
{
    Low,
    High,
    Slowing,
}

public class Obstacles : MonoBehaviour
{
    public List<ObstacleType> obstacleTypes;
}
