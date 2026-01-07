using UnityEngine;
using System.Collections;  // 引入 System.Collections 命名空间
using UnityEngine.SceneManagement;
public enum SceneSize
{
    Small,   // 小场景
    Medium,  // 中场景
    Large    // 大场景
}

public class SceneManager : MonoBehaviour
{
    public SceneSize currentSceneSize;  // 当前场景的大小

    // 根据当前场景自动检测格子大小
    public Vector2 GetGridSize()
    {
        switch (currentSceneSize)
        {
            case SceneSize.Small:
                return new Vector2(32f, 32f);  // 小场景，每格 32x32 单位
            case SceneSize.Medium:
                return new Vector2(128f, 128f);  // 中场景，每格 128x128 单位
            case SceneSize.Large:
                return new Vector2(256f, 256f);  // 大场景，每格 256x256 单位
            default:
                return new Vector2(32f, 32f);  // 默认情况下，使用小场景的格子大小
        }
    }

    // 异步加载场景
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    // 卸载场景
    public void UnloadSceneAsync(string sceneName)
    {
        StartCoroutine(UnloadSceneCoroutine(sceneName));
    }

    private IEnumerator UnloadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncUnload = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
    }
}