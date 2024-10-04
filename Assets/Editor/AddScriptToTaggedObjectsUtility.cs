using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AddScriptToTaggedObjectsUtility
{
    [MenuItem("Tools/Add Script To Tagged Objects")]
    private static void AddScriptToObjects()
    {
        AddScriptToTaggedObjectsWindow.ShowWindow();
    }
}

public class AddScriptToTaggedObjectsWindow : EditorWindow
{
    private string targetTag = "YourTag";          // ��� ��� ������ ��������
    private MonoScript scriptToAdd;                // ������ �� ������, ������� ����� ��������

    // ������ Transform �������� ��� �������� ����������
    [SerializeField]
    private List<Transform> commonTransforms = new List<Transform>();

    // �������� ���� ���������
    public static void ShowWindow()
    {
        var window = GetWindow<AddScriptToTaggedObjectsWindow>("Add Script To Tagged Objects");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� ���������� �������", EditorStyles.boldLabel);

        // ���� ��� ����� ����
        targetTag = EditorGUILayout.TagField("��� ��������", targetTag);

        // ���� ��� ������ �������
        scriptToAdd = EditorGUILayout.ObjectField("������ ��� ����������", scriptToAdd, typeof(MonoScript), false) as MonoScript;

        GUILayout.Space(10);

        // ����������� ������ Transform[]
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty serializedProperty = serializedObject.FindProperty("commonTransforms");
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Impact Prefabs"), true);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        // ������ ���������� ��������
        if (GUILayout.Button("��������/�������� ������ �� ���� �������� � �����"))
        {
            AddScriptToTaggedObjects();
        }
    }

    private void AddScriptToTaggedObjects()
    {
        // �������� ���������� �������
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

        // ���������� ��� ���������� ���������� �� ���� ��������
        foreach (GameObject obj in taggedObjects)
        {
            // ���������, ���� �� ��� ��������� �� �������
            var existingComponent = obj.GetComponent(scriptToAdd.GetClass()) as MonoBehaviour;

            if (existingComponent == null)
            {
                // ���� ���������� ���, ��������� �����
                existingComponent = obj.AddComponent(scriptToAdd.GetClass()) as MonoBehaviour;
                Debug.Log($"�������� ����� ��������� {scriptToAdd.name} �� ������ {obj.name}");
            }
            else
            {
                Debug.Log($"��������� ������������ ��������� {scriptToAdd.name} �� ������� {obj.name}");
            }

            // ������������� ��������� ��� ��������� �������� ���� impactPrefabs
            if (existingComponent != null)
            {
                var targetsField = existingComponent.GetType().GetField("impactPrefabs",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (targetsField != null && targetsField.FieldType == typeof(Transform[]))
                {
                    // ��������� ���� "impactPrefabs" �������� commonTransforms
                    targetsField.SetValue(existingComponent, commonTransforms.ToArray());
                    Debug.Log($"���� 'impactPrefabs' ��������� ��� ������� {obj.name} ({commonTransforms.Count} �������� ��������)");
                }
                else
                {
                    Debug.LogWarning($"������ {scriptToAdd.name} �� �������� ����������� ���� 'impactPrefabs' ���� Transform[].");
                }
            }
        }

        Debug.Log($"������ {scriptToAdd.name} ������� �������� ��� �������� �� ���� �������� � ����� {targetTag}.");
    }
}
