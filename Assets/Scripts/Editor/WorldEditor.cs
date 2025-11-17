using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibuja todo el inspector default (sliders incluidos)
        DrawDefaultInspector();

        WorldGenerator worldGen = (WorldGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate World"))
        {
            // Solo funciona si estás en modo Play.
            if (Application.isPlaying)
            {
                worldGen.GenerateWorld(true);
            }
            else
            {
                Debug.LogWarning("Poné Play para regenerar el mundo.");
            }
        }
    }
}
