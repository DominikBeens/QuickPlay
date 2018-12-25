using System.Collections.Generic;
using UnityEngine;

namespace DB.QuickPlay
{
    [CreateAssetMenu(fileName = "QuickPlaySettings")]
    public class QuickPlaySettings : ScriptableObject
    {

        [HideInInspector] public Vector3 quickPlayPosition;
        [HideInInspector] public List<GameObject> spawnedPlayables = new List<GameObject>();

        public List<GameObject> managersToSpawn = new List<GameObject>();
        public List<PlayableToSpawn> playablesToSpawn = new List<PlayableToSpawn>();

        [System.Serializable]
        public class PlayableToSpawn
        {
            public GameObject playable;
            public float ySpawnOffset = 0.1f;
        }

        public void ClearTempSettings()
        {
            quickPlayPosition = Vector3.zero;
            spawnedPlayables.Clear();
        }
    }
}
