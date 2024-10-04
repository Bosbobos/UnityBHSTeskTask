using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AddScriptToTaggedObjectsUtility
{
    [MenuItem("Tools/Add Script To Tagged Objects")]
    private static void AddScriptToObjects()
    {
        // Показать пользовательское окно редактора
        AddScriptToTaggedObjectsWindow.ShowWindow();
    }
}

// Основной класс окна редактора
public class AddScriptToTaggedObjectsWindow : EditorWindow
{
    private string targetTag = "YourTag";            // Тег объектов по умолчанию
    private MonoScript scriptToAdd;                  // Ссылка на скрипт, который нужно добавить
    [SerializeField]
    private List<Transform> commonTransforms = new List<Transform>();  // Используем список вместо массива

    // Показать окно редактора
    public static void ShowWindow()
    {
        var window = GetWindow<AddScriptToTaggedObjectsWindow>("Add Script To Tagged Objects");
        window.minSize = new Vector2(400, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("Параметры добавления скрипта", EditorStyles.boldLabel);

        // Поле для ввода тега
        targetTag = EditorGUILayout.TagField("Тег объектов", targetTag);

        // Поле для выбора скрипта
        scriptToAdd = EditorGUILayout.ObjectField("Скрипт для добавления", scriptToAdd, typeof(MonoScript), false) as MonoScript;

        GUILayout.Space(10);

        // Отображаем список Transform-ов
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty serializedProperty = serializedObject.FindProperty("commonTransforms");
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Transforms"), true);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        // Кнопка выполнения операции
        if (GUILayout.Button("Добавить скрипт ко всем объектам с тегом"))
        {
            AddScriptToTaggedObjects();
        }
    }

    private void AddScriptToTaggedObjects()
    {
        // Проверка на валидность введённых данных
        if (scriptToAdd == null || !typeof(MonoBehaviour).IsAssignableFrom(scriptToAdd.GetClass()))
        {
            Debug.LogError("Пожалуйста, выберите валидный скрипт, наследующийся от MonoBehaviour.");
            return;
        }

        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogError("Пожалуйста, укажите тег объектов.");
            return;
        }

        // Поиск всех объектов с указанным тегом
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);

        if (taggedObjects.Length == 0)
        {
            Debug.LogWarning($"Не найдено объектов с тегом {targetTag}.");
            return;
        }

        // Добавление компонента к каждому объекту
        foreach (GameObject obj in taggedObjects)
        {
            // Проверяем, есть ли уже компонент, чтобы избежать дублирования
            var existingComponent = obj.GetComponent(scriptToAdd.GetClass());
            if (existingComponent == null)
            {
                var newComponent = obj.AddComponent(scriptToAdd.GetClass()) as MonoBehaviour;

                // Использование рефлексии для передачи массива commonTransforms
                var targetsField = newComponent.GetType().GetField("targets",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (targetsField != null && targetsField.FieldType == typeof(Transform[]))
                {
                    targetsField.SetValue(newComponent, commonTransforms.ToArray());
                }
                else
                {
                    Debug.LogWarning($"Скрипт {scriptToAdd.name} не содержит подходящего поля 'targets' типа Transform[].");
                }
            }
        }

        Debug.Log($"Скрипт {scriptToAdd.name} успешно добавлен ко всем объектам с тегом {targetTag}.");
    }
}
