using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace KeceK.UI
{
    public class NetworkDebugCanvas : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private Button _hostButton;
        [SerializeField] [Required] private Button _clientButton;
        [SerializeField] [Required] private GameObject _networkPanel;
        
        private void Awake()
        {
            _hostButton.onClick.AddListener(OnHostButtonClicked);
            _clientButton.onClick.AddListener(OnClientButtonClicked);
        }
        
        private void OnDestroy()
        {
            _hostButton.onClick.RemoveListener(OnHostButtonClicked);
            _clientButton.onClick.RemoveListener(OnClientButtonClicked);
        }
        
        private void OnHostButtonClicked()
        {
            NetworkManager.Singleton.StartHost();
			_networkPanel.SetActive(false);
        }
        
        private void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
			_networkPanel.SetActive(false);
        }
    }
}
