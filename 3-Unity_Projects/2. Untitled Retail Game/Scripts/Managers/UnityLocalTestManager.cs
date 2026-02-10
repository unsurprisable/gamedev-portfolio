using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityLocalTestManager : MonoBehaviour
{
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("ExperimentalScene", LoadSceneMode.Single);
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
