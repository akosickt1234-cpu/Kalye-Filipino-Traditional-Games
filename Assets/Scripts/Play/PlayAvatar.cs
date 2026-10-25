using UnityEngine;

public static class PlayAvatar
{
    const string AVATAR_BOY_ID = "boy1";
    const string AVATAR_GIRL_ID = "girl1";
    const string AVATAR_BOY_PATH = "Avatars/AvatarBoy1";
    const string AVATAR_GIRL_PATH = "Avatars/AvatarGirl1";
    const string DEFAULT_AVATAR_PATH = "Icons/Lobby/IconAvatarDefault";

    public static Sprite avatar_sprite(string avatar_id)
    {
        string path;
        if (avatar_id == AVATAR_BOY_ID) path = AVATAR_BOY_PATH;
        else if (avatar_id == AVATAR_GIRL_ID) path = AVATAR_GIRL_PATH;
        else path = DEFAULT_AVATAR_PATH;

        var sprite = PlayUi.sprite(path);
        if (sprite == null && path != DEFAULT_AVATAR_PATH) sprite = PlayUi.sprite(DEFAULT_AVATAR_PATH);
        return sprite;
    }
}
