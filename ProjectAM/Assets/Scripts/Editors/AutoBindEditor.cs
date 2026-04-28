#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class AutoBindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        string typeName = target.GetType().Name;
        if (typeName.Contains("UI") == false) return;

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Auto Bind", GUILayout.Height(30)))
        {
            AutoBind((MonoBehaviour)target);
        }

        EditorGUILayout.Space(5);
    }

    private void AutoBind(MonoBehaviour target)
    {
        Undo.RecordObject(target, "Auto Bind");

        Type type = target.GetType();

        BindFields(target, type);
        BindProperties(target, type);

        EditorUtility.SetDirty(target);
    }

    private void BindFields(MonoBehaviour target, Type type)
    {
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            BindAttribute bindAttribute = field.GetCustomAttribute<BindAttribute>();

            if (bindAttribute == null) continue;
            if (typeof(Component).IsAssignableFrom(field.FieldType) == false) continue;

            string targetName = bindAttribute.Name ?? field.Name;

            Component comp = FindComponent(target.transform, targetName, field.FieldType);

            if (comp != null)
            {
                field.SetValue(target, comp);
                Debug.Log("[AutoBind] Field Bind Success: " + field.Name, target);
            }
            else
            {
                Debug.LogWarning("[AutoBind] Field Bind Failed: " + field.Name, target);
            }
        }
    }

    private void BindProperties(MonoBehaviour target, Type type)
    {
        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (PropertyInfo prop in props)
        {
            BindAttribute bindAttribute = prop.GetCustomAttribute<BindAttribute>();

            if (bindAttribute == null) continue;
            if (prop.CanWrite == false) continue;
            if (typeof(Component).IsAssignableFrom(prop.PropertyType) == false) continue;

            string targetName = bindAttribute.Name ?? prop.Name;

            Component comp = FindComponent(target.transform, targetName, prop.PropertyType);

            if (comp != null)
            {
                prop.SetValue(target, comp);
                Debug.Log("[AutoBind] Property Bind Success: " + prop.Name, target);
            }
            else
            {
                Debug.LogWarning("[AutoBind] Property Bind Failed: " + prop.Name, target);
            }
        }
    }

    private Component FindComponent(Transform root, string name, Type type)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform tempTransform in transforms)
        {
            if (tempTransform.name == name)
            {
                return tempTransform.GetComponent(type);
            }
        }

        return null;
    }
}

#endif