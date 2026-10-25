
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerIdentitySettings : MonoBehaviour
{
    private const string AvatarKey = "KALYE_Avatar";
    private const string UsernameKey = "KALYE_Username";

    // Avatar IDs: 0 = Timmy, 1 = Amy
    [Header("Avatar Buttons")]
    [SerializeField] private Button timmyButton;
    [SerializeField] private Button amyButton;
    [SerializeField] private Button saveAvatarButton;

    [Header("Current Avatar Images")]
    [SerializeField] private GameObject timmyImage;
    [SerializeField] private GameObject amyImage;

    [Header("Username")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button usernameSaveButton;

    private int selectedAvatar;
    private int savedAvatar;

    public static int SavedAvatar =>
        Mathf.Clamp(PlayerPrefs.GetInt(AvatarKey, 0), 0, 1);

    public static string SavedUsername
    {
        get
        {
            string username = PlayerPrefs.GetString(
                UsernameKey, "Player"
            ).Trim();

            return string.IsNullOrEmpty(username)
                ? "Player"
                : username;
        }
    }

    private void Start()
    {
        savedAvatar = SavedAvatar;
        selectedAvatar = savedAvatar;

        UpdateAvatarPreview();

        if (usernameInput != null)
            usernameInput.text = SavedUsername;

        if (timmyButton != null)
            timmyButton.onClick.AddListener(SelectTimmy);

        if (amyButton != null)
            amyButton.onClick.AddListener(SelectAmy);

        if (saveAvatarButton != null)
            saveAvatarButton.onClick.AddListener(SaveAvatar);

        if (usernameSaveButton != null)
            usernameSaveButton.onClick.AddListener(SaveUsername);
    }

    private void OnDestroy()
    {
        if (timmyButton != null)
            timmyButton.onClick.RemoveListener(SelectTimmy);

        if (amyButton != null)
            amyButton.onClick.RemoveListener(SelectAmy);

        if (saveAvatarButton != null)
            saveAvatarButton.onClick.RemoveListener(SaveAvatar);

        if (usernameSaveButton != null)
            usernameSaveButton.onClick.RemoveListener(SaveUsername);
    }

    private void SelectTimmy()
    {
        selectedAvatar = 0;
        UpdateAvatarPreview();
    }

    private void SelectAmy()
    {
        selectedAvatar = 1;
        UpdateAvatarPreview();
    }

    private void UpdateAvatarPreview()
    {
        if (timmyImage != null)
            timmyImage.SetActive(selectedAvatar == 0);

        if (amyImage != null)
            amyImage.SetActive(selectedAvatar == 1);
    }

    private void SaveAvatar()
    {
        savedAvatar = selectedAvatar;

        PlayerPrefs.SetInt(AvatarKey, savedAvatar);
        PlayerPrefs.Save();

        Debug.Log("Saved avatar: " +
            (savedAvatar == 0 ? "Timmy" : "Amy"));
    }

    private void SaveUsername()
    {
        if (usernameInput == null)
            return;

        string username = usernameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(username))
            username = "Player";

        if (username.Length > 16)
            username = username.Substring(0, 16);

        usernameInput.text = username;

        PlayerPrefs.SetString(UsernameKey, username);
        PlayerPrefs.Save();

        Debug.Log("Saved username: " + username);
    }
}
