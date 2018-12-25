using UnityEditor;
using UnityEngine;

namespace DB.QuickPlay
{
    [InitializeOnLoad]
    public class QuickPlay
    {

        // Double is more precise than a float. 
        // EditorApplication.timeSinceStartup returns a double.
        private static double mouseClickTime;
        private const float MOUSE_CLICK_DETECTION_TIME = 0.2f;

        private static QuickPlaySettings settings;

        static QuickPlay()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            if (!settings)
            {
                settings = Resources.Load<QuickPlaySettings>("QuickPlaySettings");
                if (!settings)
                {
                    Debug.LogWarning("Couldn't find QuickPlaySettings! Please make sure there's a QuickPlaySettings ScriptableObject named 'QuickPlaySettings' in a Resources folder.");
                }
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            //if (EditorApplication.isPlaying)
            //{
            //    return;
            //}

            Event e = Event.current;

            // Right mouse button.
            if (e.isMouse && e.button == 1)
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        mouseClickTime = EditorApplication.timeSinceStartup;
                        break;

                    case EventType.MouseUp:
                        // User took too long to release the RMB or just didnt mean to right click. Return.
                        if (EditorApplication.timeSinceStartup - mouseClickTime > MOUSE_CLICK_DETECTION_TIME)
                        {
                            return;
                        }

                        // Raycast from the sceneview into the world. If it hits something then save that position and display a menu where the use can start quick play.
                        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                        {
                            settings.quickPlayPosition = hit.point;
                            ShowMenu();
                            e.Use();
                        }
                        break;
                }
            }
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange newState)
        {
            switch (newState)
            {
                // Spawn objects (only spawns when quickplay position isnt Vector3.zero because
                // we don't want to spawn them when we manually enter play mode.
                case PlayModeStateChange.EnteredPlayMode:
                    SpawnQuickPlayObjects();
                    break;

                // Reset temp variables.
                case PlayModeStateChange.ExitingPlayMode:
                    settings.ClearTempSettings();
                    break;
            }
        }

        private static void SpawnQuickPlayObjects()
        {
            if (settings.quickPlayPosition == Vector3.zero)
            {
                return;
            }

            for (int i = 0; i < settings.managersToSpawn.Count; i++)
            {
                if (settings.managersToSpawn[i])
                {
                    Object.Instantiate(settings.managersToSpawn[i], settings.quickPlayPosition, Quaternion.identity);
                }
            }

            for (int i = 0; i < settings.playablesToSpawn.Count; i++)
            {
                if (settings.playablesToSpawn[i]?.playable)
                {
                    GameObject playable = Object.Instantiate(settings.playablesToSpawn[i].playable, settings.quickPlayPosition + Vector3.up * settings.playablesToSpawn[i].ySpawnOffset, Quaternion.identity);
                    settings.spawnedPlayables.Add(playable);
                }
            }
        }

        private static void TeleportPlayables()
        {
            if (settings.quickPlayPosition == Vector3.zero)
            {
                return;
            }

            for (int i = 0; i < settings.spawnedPlayables.Count; i++)
            {
                if (settings.spawnedPlayables[i])
                {
                    settings.spawnedPlayables[i].transform.position = settings.quickPlayPosition + Vector3.up * 0.1f;
                }
            }
        }

        private static void ShowMenu()
        {
            GenericMenu newMenu = new GenericMenu();

            if (EditorApplication.isPlaying)
            {
                newMenu.AddItem(new GUIContent("Teleport Here"), false, TeleportPlayables);
            }
            else
            {
                newMenu.AddItem(new GUIContent("Quick Play"), false, () => { EditorApplication.isPlaying = true; });
            }

            newMenu.ShowAsContext();
        }
    }
}