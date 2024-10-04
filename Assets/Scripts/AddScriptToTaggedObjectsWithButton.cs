using UnityEngine;

[ExecuteInEditMode]
public class AddScriptToTaggedObjectsWithButton : MonoBehaviour
{
    [Tooltip("������� ������ Transform, ������� ����� ������� ������� �������")]
    public Transform[] commonTransforms;

    [Tooltip("������� ��� ��������, � ������� ����� �������� ������")]
    public string targetTag = "YourTag"; // �������� �� ��� ��� �� ���������

    [Tooltip("�������� ������, ������� ���������� ��������")]
    public MonoBehaviour scriptToAdd; // ������ �� ��������� ������� (������ ���� �������� � �����)

    [ContextMenu("�������� ������ �� ���� �������� � �����")]
    public void AddScriptToObjectsInEditor()
    {
        if (scriptToAdd == null)
        {
            Debug.LogError("������ ��� ���������� �� ������. ����������, �������� ������ �� ������ ���������.");
            return;
        }

        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogError("����������, ������� ��� �������� ��� ���������� �������.");
            return;
        }

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        if (taggedObjects.Length == 0)
        {
            Debug.LogWarning($"�� ������� �������� � ����� {targetTag}.");
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
                    Debug.LogWarning($"������ {scriptToAdd.GetType().Name} �� �������� ����������� ���� 'targets' ���� Transform[].");
                }
            }
        }

        Debug.Log($"������ {scriptToAdd.GetType().Name} ������� �������� �� ���� �������� � ����� {targetTag}.");
    }
}
