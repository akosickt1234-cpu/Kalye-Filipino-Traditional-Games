using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
public class NetworkManagerUI : MonoBehaviour
{
    
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    [SerializeField] private Button createBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField code;
    [SerializeField] private TestRelay testRelay;


    private void Awake()
    {   
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        createBtn.onClick.AddListener(() =>
        {
            testRelay.CreateRelay();
        });

        joinBtn.onClick.AddListener(() =>
        {
            string joinCode = code.text.Trim();

            if (string.IsNullOrEmpty(joinCode))
            {
                
                return;
            }

            testRelay.JoinRelay(joinCode);


        });
    }
}
