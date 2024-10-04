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
    private string targetTag = "YourTag";          // Тег для поиска объектов
    private MonoScript scriptToAdd;                // Ссылка на скрипт, который нужно добавить

    // Список Transform префабов для передачи компоненту
    [SerializeField]
    private List<Transform> commonTransforms = new List<Transform>();

    // Показать окно редактора
    public static void ShowWindow()
    {
        var window = GetWindow<AddScriptToTaggedObjectsWindow>("Add Script To Tagged Objects");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("Параметры добавления скрипта", EditorStyles.boldLabel);

        // Поле для ввода тега
        targetTag = EditorGUILayout.TagField("Тег объектов", targetTag);

        // Поле для выбора скрипта
        scriptToAdd = EditorGUILayout.ObjectField("Скрипт для добавления", scriptToAdd, typeof(MonoScript), false) as MonoScript;

        GUILayout.Space(10);

        // Отображение списка Transform[]
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty serializedProperty = serializedObject.FindProperty("commonTransforms");
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Impact Prefabs"), true);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        // Кнопка выполнения операции
        if (GUILayout.Button("Добавить/Обновить скрипт ко всем объектам с тегом"))
        {
            AddScriptToTaggedObjects();
        }
    }

    private void AddScriptToTaggedObjects()
    {
        // Проверка валидности скрипта
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

        // Добавление или обновление компонента ко всем объектам
        foreach (GameObject obj in taggedObjects)
        {
            // Проверяем, есть ли уже компонент на объекте
            var existingComponent = obj.GetComponent(scriptToAdd.GetClass()) as MonoBehaviour;

            if (existingComponent == null)
            {
                // Если компонента нет, добавляем новый
                existingComponent = obj.AddComponent(scriptToAdd.GetClass()) as MonoBehaviour;
                Debug.Log($"Добавлен новый компонент {scriptToAdd.name} на объект {obj.name}");
            }
            else
            {
                Debug.Log($"Обновляем существующий компонент {scriptToAdd.name} на объекте {obj.name}");
            }

            // Использование рефлексии для установки значения поля impactPrefabs
            if (existingComponent != null)
            {
                var targetsField = existingComponent.GetType().GetField("impactPrefabs",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (targetsField != null && targetsField.FieldType == typeof(Transform[]))
                {
                    // Обновляем поле "impactPrefabs" массивом commonTransforms
                    targetsField.SetValue(existingComponent, commonTransforms.ToArray());
                    Debug.Log($"Поле 'impactPrefabs' обновлено для объекта {obj.name} ({commonTransforms.Count} префабов передано)");
                }
                else
                {
                    Debug.LogWarning($"Скрипт {scriptToAdd.name} не содержит подходящего поля 'impactPrefabs' типа Transform[].");
                }
            }
        }

        Debug.Log($"Скрипт {scriptToAdd.name} успешно добавлен или обновлен ко всем объектам с тегом {targetTag}.");
    }
}
