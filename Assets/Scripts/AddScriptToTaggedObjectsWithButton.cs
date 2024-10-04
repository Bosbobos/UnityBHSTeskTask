using UnityEngine;

[ExecuteInEditMode]
public class AddScriptToTaggedObjectsWithButton : MonoBehaviour
{
    [Tooltip("Укажите массив Transform, который будет передан каждому объекту")]
    public Transform[] commonTransforms;

    [Tooltip("Укажите тег объектов, к которым будет добавлен скрипт")]
    public string targetTag = "YourTag"; // Замените на ваш тег по умолчанию

    [Tooltip("Выберите скрипт, который необходимо добавить")]
    public MonoBehaviour scriptToAdd; // Ссылка на экземпляр скрипта (должен быть добавлен в сцену)

    [ContextMenu("Добавить скрипт ко всем объектам с тегом")]
    public void AddScriptToObjectsInEditor()
    {
        if (scriptToAdd == null)
        {
            Debug.LogError("Скрипт для добавления не указан. Пожалуйста, добавьте ссылку на нужный компонент.");
            return;
        }

        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogError("Пожалуйста, укажите тег объектов для добавления скрипта.");
            return;
        }

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        if (taggedObjects.Length == 0)
        {
            Debug.LogWarning($"Не найдено объектов с тегом {targetTag}.");
            return;
        }

        foreach (GameObject obj in taggedObjects)
        {
            var existingComponent = obj.GetComponent(scriptToAdd.GetType());
            if (existingComponent == null)
            {
                var newComponent = obj.AddComponent(scriptToAdd.GetType()) as MonoBehaviour;

                var targetsField = newComponent.GetType().GetField("targets",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (targetsField != null && targetsField.FieldType == typeof(Transform[]))
                {
                    targetsField.SetValue(newComponent, commonTransforms);
                }
                else
                {
                    Debug.LogWarning($"Скрипт {scriptToAdd.GetType().Name} не содержит подходящего поля 'targets' типа Transform[].");
                }
            }
        }

        Debug.Log($"Скрипт {scriptToAdd.GetType().Name} успешно добавлен ко всем объектам с тегом {targetTag}.");
    }
}
