using UnityEngine;

public class TipsToCharacterCreation : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("characterCreation");
        }
    }
}
