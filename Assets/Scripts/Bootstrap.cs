using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string imageUrl = "https://i.pinimg.com/736x/50/22/6c/50226c989185196b4aa590841dfb21fa.jpg";
    [SerializeField] private SpriteRenderer imageFromURL;
    [SerializeField] private SpriteRenderer imageFromResources;
    [SerializeField] private Button sceneLoadButton;
    [SerializeField] private Slider webProgressBar;
    [SerializeField] private Slider resourcesProgressBar;
    [SerializeField] private Slider sceneProgressBar;
    [SerializeField] private SceneAsset nextScene;

    private AsyncOperation _loadOperation;

    private void Start()
    {
        // ������������� �������� ��������
        InitializeGameAsync().Forget();
    }

    private async UniTaskVoid InitializeGameAsync()
    {
        // ����������� ������ �������� ����� �� ��������� ���� ��������
        sceneLoadButton.interactable = false;

        // �������� ����������� �� URL
        await LoadImageFromURLAsync(imageUrl, imageFromURL);

        // �������� ����������� �� ����� Resources
        await LoadImageFromResourcesAsync("initial_image", imageFromResources);

        await LoadSceneAsync(nextScene.name);

        // �������� �� ������ ��� �������� �����
        sceneLoadButton.onClick.AddListener(() =>
        {
            ActivateScene();
        });

        // ���������� ������ ����� ���� ��������
        sceneLoadButton.interactable = true;
    }

    private async UniTask LoadImageFromURLAsync(string url, SpriteRenderer targetImage)
    {
        using (var webRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                // ���������� ��������-����
                webProgressBar.value = operation.progress;
                await UniTask.Yield();
            }

            if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var oldSize = targetImage.size;
                var texture = DownloadHandlerTexture.GetContent(webRequest);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                targetImage.sprite = sprite;
                targetImage.size = oldSize;
            }
            else
            {
                Debug.LogError("������ ��� �������� ����������� �� URL: " + webRequest.error);
            }
        }
    }

    private async UniTask LoadImageFromResourcesAsync(string resourceName, SpriteRenderer targetImage)
    {
        var resourceRequest = Resources.LoadAsync<Sprite>(resourceName);

        while (!resourceRequest.isDone)
        {
            // ���������� ��������-����
            resourcesProgressBar.value = resourceRequest.progress;
            await UniTask.Yield();
        }

        if (resourceRequest.asset is Sprite sprite)
        {
            var oldSize = targetImage.size;
            targetImage.sprite = sprite;
            targetImage.size = oldSize;
        }
        else
        {
            Debug.LogError("������ ��� �������� ����������� �� Resources.");
        }

        resourcesProgressBar.value = 1f;
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        _loadOperation = SceneManager.LoadSceneAsync(sceneName);
        _loadOperation.allowSceneActivation = false;

        while (_loadOperation.progress < 0.9f)
        {
            // ���������� ��������-����
            sceneProgressBar.value = _loadOperation.progress;
            await UniTask.Yield();
        }

        // ����� ������ � ���������
        sceneProgressBar.value = 1.0f;
    }

    private void ActivateScene()
    {
        _loadOperation.allowSceneActivation = true;
        _loadOperation = null;
    }
}
