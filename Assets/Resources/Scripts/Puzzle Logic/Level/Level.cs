using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    [HideInInspector]
    public int levelSetID;
    public LevelInfo info;

    public struct FakeConnectionGrids
    {
        public float targetAngle;
        public LevelGrid grid1;
        public LevelGrid grid2;
    }

    public List<FakeConnectionGrids> fakeConnectionGridsList = new List<FakeConnectionGrids>();

    public void ResetLevel()
    {

    }
}
