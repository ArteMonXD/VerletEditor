using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ExampleScript : MonoBehaviour
{
    public float value = 1.0f;
}

[CustomEditor(typeof(ExampleScript))]
public class ExampleEditor : Editor
{
    // Custom in-scene UI for when ExampleScript component is selected.
    public void OnSceneGUI()
    {
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        var evt = Event.current;

        var t = target as ExampleScript;
        var tr = t.transform;
        var pos = tr.position;

        switch (evt.GetTypeForControl(controlID))
        {
            case EventType.Layout:
            case EventType.MouseMove:
                // Set the nearest control ID to "controlID" if the mouse cursor is
                // near or directly above the solid disc handle.
                var distanceToHandle = HandleUtility.DistanceToCircle(pos, t.value);
                HandleUtility.AddControl(controlID, distanceToHandle);
                break;
            case EventType.MouseDown:
                // Log the nearest control ID if the click is near or directly above
                // the solid disc handle.
                if (controlID == HandleUtility.nearestControl && evt.button == 0)
                {
                    Debug.Log($"The nearest control is {controlID}.");

                    GUIUtility.hotControl = controlID;
                    evt.Use();
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && evt.button == 0)
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
                break;
            case EventType.Repaint:
                // Display an orange solid disc where the object is.
                Handles.color = new Color(1, 0.8f, 0.4f, 1);
                Handles.DrawSolidDisc(pos, tr.up, t.value);
                break;
        }
    }
}
