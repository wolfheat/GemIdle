using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BackgroundRandomizer), true)]
public class BackgroundRandomizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the normal fields
        DrawDefaultInspector();

        BackgroundRandomizer randomizer = (BackgroundRandomizer)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Randomize Background")) {
            randomizer.RandomizeBackground();

            // Mark object dirty so changes are saved in the editor
            EditorUtility.SetDirty(randomizer);
        }
    }
}
