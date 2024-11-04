using UnityEngine;

[CreateAssetMenu(fileName = "DungeonGeneratorData.asset", menuName = "DungeonGenerationData/Dungeon Data")]

public class DungeonGenerationData : ScriptableObject
{
    public int numberOfCrawlers;
    public int iterationMin;
    public int iterationMax;
}
