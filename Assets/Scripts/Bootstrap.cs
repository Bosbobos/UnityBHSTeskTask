using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField] private TextMeshProUGUI webProgressText; 
    [SerializeField] private Slider webProgressBar;

    [SerializeField] private TextMeshProUGUI resourcesProgressText;
    [SerializeField] private Slider resourcesProgressBar;

    [SerializeField] private TextMeshProUGUI sceneProgressText;
    [SerializeField] private Slider sceneProgressBar;

    [SerializeField] private SceneAsset nextScene;


    private AsyncOperation _loadOperation;

    private void Start()
    {
        // Инициализация процесса загрузки
        InitializeGameAsync().Forget();
    }

    private async UniTaskVoid InitializeGameAsync()
    {
        // Деактивация кнопки загрузки сцены до окончания всех загрузок
        sceneLoadButton.interactable = false;

        // Загрузка изображения по URL
        await LoadImageFromURLAsync(imageUrl, imageFromURL);

        // Загрузка изображения из папки Resources
        await LoadImageFromResourcesAsync("initial_image", imageFromResources);

        await LoadSceneAsync(nextScene.name);

        // Подписка на кнопку для загрузки сцены
        sceneLoadButton.onClick.AddListener(() =>
        {
            ActivateScene();
        });

        // Активируем кнопку после всех загрузок
        sceneLoadButton.interactable = true;
    }

    private async UniTask LoadImageFromURLAsync(string url, SpriteRenderer targetImage)
    {
        using (var webRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                // Обновление прогресс-бара и текста
                webProgressBar.value = operation.progress;
                webProgressText.text = $"Загрузка (Web): {Mathf.RoundToInt(operation.progress * 100)}%";
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
                Debug.LogError("Ошибка при загрузке изображения по URL: " + webRequest.error);
            }
        }

        webProgressBar.value = 1.0f;
        webProgressText.text = "Загрузка (Web): 100%";
    }

    private async UniTask LoadImageFromResourcesAsync(string resourceName, SpriteRenderer targetImage)
    {
        var resourceRequest = Resources.LoadAsync<Sprite>(resourceName);

        while (!resourceRequest.isDone)
        {
            // Обновление прогресс-бара и текста
            resourcesProgressBar.value = resourceRequest.progress;
            resourcesProgressText.text = $"Загрузка (Resources): {Mathf.RoundToInt(resourceRequest.progress * 100)}%";
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
            Debug.LogError("Ошибка при загрузке изображения из Resources.");
        }

        resourcesProgressBar.value = 1.0f;
        resourcesProgressText.text = "Загрузка (Resources): 100%";
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        _loadOperation = SceneManager.LoadSceneAsync(sceneName);
        _loadOperation.allowSceneActivation = false;

        while (_loadOperation.progress < 0.9f)
        {
            // Обновление прогресс-бара и текста
            sceneProgressBar.value = _loadOperation.progress;
            sceneProgressText.text = $"Загрузка сцены: {Mathf.RoundToInt(_loadOperation.progress * 100)}%";
            await UniTask.Yield();
        }

        sceneProgressBar.value = 1.0f;
        sceneProgressText.text = "Загрузка сцены: 100%";
    }

    private void ActivateScene()
    {
        _loadOperation.allowSceneActivation = true;
        _loadOperation = null;
    }
}
