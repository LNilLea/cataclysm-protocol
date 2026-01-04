using UnityEngine;

public class StartToTips : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("tips");
        }
    }
}