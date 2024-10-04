using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AddScriptToTaggedObjectsUtility
{
    [MenuItem("Tools/Add Script To Tagged Objects")]
    private static void AddScriptToObjects()
    {
        // �������� ���������������� ���� ���������
        AddScriptToTaggedObjectsWindow.ShowWindow();
    }
}

// �������� ����� ���� ���������
public class AddScriptToTaggedObjectsWindow : EditorWindow
{
    private string targetTag = "YourTag";            // ��� �������� �� ���������
    private MonoScript scriptToAdd;                  // ������ �� ������, ������� ����� ��������
    [SerializeField]
    private List<Transform> commonTransforms = new List<Transform>();  // ���������� ������ ������ �������

    // �������� ���� ���������
    public static void ShowWindow()
    {
        var window = GetWindow<AddScriptToTaggedObjectsWindow>("Add Script To Tagged Objects");
        window.minSize = new Vector2(400, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� ���������� �������", EditorStyles.boldLabel);

        // ���� ��� ����� ����
        targetTag = EditorGUILayout.TagField("��� ��������", targetTag);

        // ���� ��� ������ �������
        scriptToAdd = EditorGUILayout.ObjectField("������ ��� ����������", scriptToAdd, typeof(MonoScript), false) as MonoScript;

        GUILayout.Space(10);

        // ���������� ������ Transform-��
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty serializedProperty = serializedObject.FindProperty("commonTransforms");
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Transforms"), true);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        // ������ ���������� ��������
        if (GUILayout.Button("�������� ������ �� ���� �������� � �����"))
        {
            AddScriptToTaggedObjects();
        }
    }

    private void AddScriptToTaggedObjects()
    {
        // �������� �� ���������� �������� ������
        if (scriptToAdd == null || !typeof(MonoBehaviour).IsAssignableFrom(scriptToAdd.GetClass()))
        {
            Debug.LogError("����������, �������� �������� ������, ������������� �� MonoBehaviour.");
            return;
        }

        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogError("����������, ������� ��� ��������.");
            return;
        }

        // ����� ���� �������� � ��������� �����
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);

        if (taggedObjects.Length == 0)
        {
            Debug.LogWarning($"�� ������� �������� � ����� {targetTag}.");
            return;
        }

        // ���������� ���������� � ������� �������
        foreach (GameObject obj in taggedObjects)
        {
            // ���������, ���� �� ��� ���������, ����� �������� ������������
            var existingComponent = obj.GetComponent(scriptToAdd.GetClass());
            if (existingComponent == null)
            {
                var newComponent = obj.AddComponent(scriptToAdd.GetClass()) as MonoBehaviour;

                // ������������� ��������� ��� �������� ������� commonTransforms
                var targetsField = newComponent.GetType().GetField("targets",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (targetsField != null && targetsField.FieldType == typeof(Transform[]))
                {
                    targetsField.SetValue(newComponent, commonTransforms.ToArray());
                }
                else
                {
                    Debug.LogWarning($"������ {scriptToAdd.name} �� �������� ����������� ���� 'targets' ���� Transform[].");
                }
            }
        }

        Debug.Log($"������ {scriptToAdd.name} ������� �������� �� ���� �������� � ����� {targetTag}.");
    }
}
