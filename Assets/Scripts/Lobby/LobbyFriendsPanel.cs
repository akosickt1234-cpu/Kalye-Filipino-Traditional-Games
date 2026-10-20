using UnityEngine;
using UnityEngine.UI;

public static class LobbyFriendsPanel
{
    const string ICON_PATH = "Icons/Play/IconPlayers";
    const string HEADER_TEXT = "FRIENDS";
    internal const string EMPTY_TEXT = "No friends yet";
    const float CONTENT_LEFT = 0.07f;
    const float CONTENT_RIGHT = 0.93f;
    const float CONTENT_BOTTOM = 0.05f;
    const float CONTENT_TOP = 0.95f;
    const float HEADER_BOTTOM = 0.88f;
    const float UNDERLINE_Y = 0.865f;
    const float UNDERLINE_THICKNESS = 2f;
    const float LIST_TOP = 0.84f;
    const float HEADER_TEXT_START = 0.17f;
    const int HEADER_MIN_SIZE = 18;
    const int HEADER_MAX_SIZE = 44;
    internal const float ROW_HEIGHT = 96f;
    const float UNDERLINE_ALPHA = 0.45f;
    internal const float EMPTY_ALPHA = 0.8f;
    internal const string EMPTY_HINT_TEXT = "Add friends and play together!";
    internal const string ONLINE_SUFFIX = " ONLINE";
    const float PILL_LEFT = 0.6f;
    const float PILL_BOTTOM = 0.2f;
    const float PILL_TOP = 0.8f;
    const float PILL_RING_THICKNESS = 2f;
    const float PILL_RING_ALPHA = 0.7f;
    const float PILL_DOT_LEFT = 0.1f;
    const float PILL_DOT_BOTTOM = 0.34f;
    const float PILL_DOT_TOP = 0.66f;
    const float PILL_TEXT_LEFT = 0.26f;
    const float PILL_TEXT_RIGHT = 0.94f;
    const float TITLE_PILL_GAP = 0.02f;
    const int PILL_MIN_SIZE = 10;
    const int PILL_MAX_SIZE = 24;

    public static void build(RectTransform friends_panel)
    {
        if (friends_panel == null) return;

        var content = PlayUi.rect("friends_content", friends_panel);
        LobbyUi.place_fraction(content, CONTENT_LEFT, CONTENT_BOTTOM, CONTENT_RIGHT, CONTENT_TOP);

        var online_text = build_header(content);

        var list_area = PlayUi.rect("friends_list_area", content);
        LobbyUi.place_fraction(list_area, 0f, 0f, 1f, LIST_TOP);

        var view = list_area.gameObject.AddComponent<LobbyFriendsView>();
        view.setup(list_area, online_text);
    }

    static Text build_header(RectTransform content)
    {
        var header = PlayUi.rect("friends_header", content);
        LobbyUi.place_fraction(header, 0f, HEADER_BOTTOM, 1f, 1f);

        var icon_slot = PlayUi.rect("friends_icon_slot", header);
        icon_slot.anchorMin = new Vector2(0f, 0.15f);
        icon_slot.anchorMax = new Vector2(0f, 0.85f);
        icon_slot.pivot = new Vector2(0f, 0.5f);
        icon_slot.offsetMin = Vector2.zero;
        icon_slot.offsetMax = Vector2.zero;
        var icon_fit = icon_slot.gameObject.AddComponent<AspectRatioFitter>();
        icon_fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        icon_fit.aspectRatio = 1f;

        var icon = PlayUi.image("friends_icon", icon_slot, PlayUi.sprite(ICON_PATH));
        icon.preserveAspect = true;
        icon.color = PlayTheme.lobby_gold_icon;
        icon.enabled = icon.sprite != null;
        PlayUi.stretch(icon.rectTransform);

        var title = PlayUi.text("friends_title", header, HEADER_TEXT, HEADER_MAX_SIZE, PlayTheme.lobby_text, TextAnchor.MiddleLeft);
        LobbyUi.place_fraction(title.rectTransform, HEADER_TEXT_START, 0f, PILL_LEFT - TITLE_PILL_GAP, 1f);
        LobbyFriendRow.fit_text(title, HEADER_MIN_SIZE, HEADER_MAX_SIZE);
        PlayUi.add_shadow(title, 2f);

        var underline = PlayOrnament.line(content, "friends_underline", 100f, UNDERLINE_THICKNESS, PlayTheme.with_alpha(PlayTheme.divider_line, UNDERLINE_ALPHA), true, true);
        PlayUi.place(underline.rectTransform, new Vector2(0f, UNDERLINE_Y), new Vector2(1f, UNDERLINE_Y), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, UNDERLINE_THICKNESS));

        return build_online_pill(header);
    }

    static Text build_online_pill(RectTransform header)
    {
        var root = PlayUi.rect("online_pill", header);
        LobbyUi.place_fraction(root, PILL_LEFT, PILL_BOTTOM, 1f, PILL_TOP);

        var ring = PlayUi.image("online_ring", root, PlayUi.pill());
        ring.type = Image.Type.Sliced;
        ring.color = PlayTheme.with_alpha(PlayTheme.lobby_ready, PILL_RING_ALPHA);
        PlayUi.stretch(ring.rectTransform);

        var fill = PlayUi.image("online_fill", root, PlayUi.pill());
        fill.type = Image.Type.Sliced;
        fill.color = PlayTheme.lobby_inner_bg;
        PlayUi.stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(PILL_RING_THICKNESS, PILL_RING_THICKNESS);
        fill.rectTransform.offsetMax = new Vector2(-PILL_RING_THICKNESS, -PILL_RING_THICKNESS);

        var dot_slot = PlayUi.rect("online_dot_slot", root);
        dot_slot.anchorMin = new Vector2(PILL_DOT_LEFT, PILL_DOT_BOTTOM);
        dot_slot.anchorMax = new Vector2(PILL_DOT_LEFT, PILL_DOT_TOP);
        dot_slot.pivot = new Vector2(0f, 0.5f);
        dot_slot.offsetMin = Vector2.zero;
        dot_slot.offsetMax = Vector2.zero;
        var dot_fit = dot_slot.gameObject.AddComponent<AspectRatioFitter>();
        dot_fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        dot_fit.aspectRatio = 1f;

        var dot = PlayUi.circle("online_dot", dot_slot, PlayTheme.lobby_ready);
        PlayUi.stretch(dot.rectTransform);

        var label = PlayUi.text("online_text", root, "0" + LobbyFriendsPanel.ONLINE_SUFFIX, PILL_MAX_SIZE, PlayTheme.lobby_text, TextAnchor.MiddleLeft);
        LobbyUi.place_fraction(label.rectTransform, PILL_TEXT_LEFT, 0f, PILL_TEXT_RIGHT, 1f);
        LobbyFriendRow.fit_text(label, PILL_MIN_SIZE, PILL_MAX_SIZE);
        return label;
    }
}

public class LobbyFriendsView : MonoBehaviour
{
    RectTransform viewport_rect;
    RectTransform content_rect;
    ScrollRect scroll;
    RectTransform empty_root;
    Text online_text;

    public void setup(RectTransform list_area, Text online_label)
    {
        online_text = online_label;

        var viewport = PlayUi.image("friends_viewport", list_area, null, true);
        PlayUi.stretch(viewport.rectTransform);
        viewport.gameObject.AddComponent<RectMask2D>();
        viewport_rect = viewport.rectTransform;

        content_rect = PlayUi.rect("friends_scroll_content", viewport_rect);
        content_rect.anchorMin = new Vector2(0f, 1f);
        content_rect.anchorMax = new Vector2(1f, 1f);
        content_rect.pivot = new Vector2(0.5f, 1f);
        content_rect.offsetMin = Vector2.zero;
        content_rect.offsetMax = Vector2.zero;

        var layout = content_rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        var fitter = content_rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport_rect;
        scroll.content = content_rect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = LobbyUi.SCROLL_SENSITIVITY;

        rebuild();
    }

    void OnEnable()
    {
        LobbyState.friends_changed += rebuild;
    }

    void OnDisable()
    {
        LobbyState.friends_changed -= rebuild;
    }

    void rebuild()
    {
        if (content_rect == null) return;

        var scroll_position = scroll.verticalNormalizedPosition;

        for (var i = content_rect.childCount - 1; i >= 0; i--)
        {
            var child = content_rect.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        if (empty_root != null)
        {
            Destroy(empty_root.gameObject);
            empty_root = null;
        }

        var friends = LobbyState.friends;
        var count = 0;
        var online_count = 0;
        for (var i = 0; friends != null && i < friends.Count; i++)
        {
            if (friends[i] == null) continue;
            LobbyFriendRow.create(content_rect, friends[i], LobbyFriendsPanel.ROW_HEIGHT);
            count++;
            if (friends[i].online) online_count++;
        }

        if (online_text != null) online_text.text = online_count + LobbyFriendsPanel.ONLINE_SUFFIX;

        if (count == 0)
        {
            build_empty(viewport_rect);
            return;
        }

        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll_position);
    }

    void build_empty(RectTransform parent)
    {
        var tint = PlayTheme.with_alpha(PlayTheme.lobby_text_muted, LobbyFriendsPanel.EMPTY_ALPHA);

        empty_root = PlayUi.rect("friends_empty", parent);
        LobbyUi.place_fraction(empty_root, 0f, 0.2f, 1f, 0.85f);

        var icon_slot = PlayUi.rect("friends_empty_icon_slot", empty_root);
        icon_slot.anchorMin = new Vector2(0.5f, 0.62f);
        icon_slot.anchorMax = new Vector2(0.5f, 1f);
        icon_slot.pivot = new Vector2(0.5f, 0.5f);
        icon_slot.offsetMin = Vector2.zero;
        icon_slot.offsetMax = Vector2.zero;
        var icon_fit = icon_slot.gameObject.AddComponent<AspectRatioFitter>();
        icon_fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        icon_fit.aspectRatio = 1f;

        var icon = PlayUi.image("friends_empty_icon", icon_slot, PlayUi.sprite(LobbyUi.ICON_USER_PLUS_PATH));
        icon.preserveAspect = true;
        icon.color = PlayTheme.friends_icon;
        icon.enabled = icon.sprite != null;
        PlayUi.stretch(icon.rectTransform);

        var title = PlayUi.text("friends_empty_title", empty_root, LobbyFriendsPanel.EMPTY_TEXT, 28, PlayTheme.lobby_text, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(title.rectTransform, 0f, 0.42f, 1f, 0.58f);
        LobbyFriendRow.fit_text(title, 14, 30);

        var divider = PlayOrnament.line(empty_root, "friends_empty_divider", 100f, 2f, PlayTheme.with_alpha(PlayTheme.divider_line, 0.45f), true, true);
        PlayUi.place(divider.rectTransform, new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.38f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 2f));

        var hint = PlayUi.text("friends_empty_hint", empty_root, LobbyFriendsPanel.EMPTY_HINT_TEXT, 22, tint, TextAnchor.UpperCenter);
        LobbyUi.place_fraction(hint.rectTransform, 0.05f, 0f, 0.95f, 0.34f);
        hint.horizontalOverflow = HorizontalWrapMode.Wrap;
        LobbyFriendRow.fit_text(hint, 12, 24);
    }
}

public class LobbyFriendRow
{
    const string INVITE_LABEL = "Invite";
    const string INVITED_LABEL = "Invited";
    const string ONLINE_LABEL = "Online";
    const string OFFLINE_LABEL = "Offline";
    const float OFFLINE_ALPHA = 0.55f;
    const float DISABLED_ALPHA = 0.6f;
    const float INNER_LEFT = 0.02f;
    const float INNER_RIGHT = 0.98f;
    const float RING_THICKNESS = 2f;
    const float DIVIDER_THICKNESS = 2f;
    const float DIVIDER_ALPHA = 0.3f;
    const float NAME_LEFT = 0.26f;
    const float NAME_RIGHT = 0.66f;
    const float BUTTON_LEFT = 0.68f;
    const float BUTTON_BOTTOM = 0.27f;
    const float BUTTON_TOP = 0.73f;
    const int NAME_MIN_SIZE = 14;
    const int NAME_MAX_SIZE = 30;
    const int STATUS_MIN_SIZE = 12;
    const int STATUS_MAX_SIZE = 22;
    const int BUTTON_MIN_SIZE = 12;
    const int BUTTON_MAX_SIZE = 24;
    const int INITIAL_MIN_SIZE = 12;
    const int INITIAL_MAX_SIZE = 40;

    readonly LobbyFriend friend;
    Button button;
    Image button_ring;
    Text button_label;
    CanvasGroup button_group;
    PlayButtonFx button_fx;

    LobbyFriendRow(LobbyFriend source)
    {
        friend = source;
    }

    public static LobbyFriendRow create(RectTransform parent, LobbyFriend source, float height)
    {
        var row = new LobbyFriendRow(source);
        row.build(parent, height);
        row.refresh();
        return row;
    }

    public static void fit_text(Text label, int min_size, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
        label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    public static Color status_color(bool online)
    {
        return online ? PlayTheme.lobby_ready : PlayTheme.lobby_text_muted;
    }

    public void refresh()
    {
        if (button == null) return;

        button_label.text = friend.invited ? INVITED_LABEL : INVITE_LABEL;
        set_enabled(!friend.invited);
    }

    void build(RectTransform parent, float height)
    {
        var row = PlayUi.rect("friend_row", parent);
        var layout = row.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        layout.minHeight = height;
        var group = row.gameObject.AddComponent<CanvasGroup>();
        group.alpha = friend.online ? 1f : OFFLINE_ALPHA;

        var inner = PlayUi.rect("friend_inner", row);
        LobbyUi.place_fraction(inner, INNER_LEFT, 0f, INNER_RIGHT, 1f);

        build_avatar(inner);
        build_texts(inner);
        build_button(inner);

        var divider = PlayOrnament.line(row, "friend_divider", 100f, DIVIDER_THICKNESS, PlayTheme.with_alpha(PlayTheme.divider_line, DIVIDER_ALPHA), true, true);
        PlayUi.place(divider.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, DIVIDER_THICKNESS));
    }

    void build_avatar(RectTransform inner)
    {
        var slot = PlayUi.rect("avatar_slot", inner);
        slot.anchorMin = new Vector2(0f, 0.12f);
        slot.anchorMax = new Vector2(0f, 0.88f);
        slot.pivot = new Vector2(0f, 0.5f);
        slot.offsetMin = Vector2.zero;
        slot.offsetMax = Vector2.zero;
        var fit = slot.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fit.aspectRatio = 1f;

        var ring = PlayUi.circle("avatar_ring", slot, PlayTheme.avatar_ring);
        PlayUi.stretch(ring.rectTransform);

        var face = PlayUi.circle("avatar_face", slot, PlayTheme.lobby_inner_bg);
        PlayUi.stretch(face.rectTransform);
        face.rectTransform.offsetMin = new Vector2(RING_THICKNESS, RING_THICKNESS);
        face.rectTransform.offsetMax = new Vector2(-RING_THICKNESS, -RING_THICKNESS);
        face.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        var art = string.IsNullOrEmpty(friend.avatar_path) ? null : Resources.Load<Sprite>(friend.avatar_path);
        if (art != null)
        {
            var picture = PlayUi.image("avatar_art", face.rectTransform, art);
            picture.color = PlayTheme.white;
            picture.preserveAspect = false;
            PlayUi.stretch(picture.rectTransform);
            return;
        }

        var initial = PlayUi.text("avatar_initial", face.rectTransform, initial_of(friend.name), INITIAL_MAX_SIZE, PlayTheme.lobby_text, TextAnchor.MiddleCenter);
        PlayUi.stretch(initial.rectTransform);
        fit_text(initial, INITIAL_MIN_SIZE, INITIAL_MAX_SIZE);
    }

    void build_texts(RectTransform inner)
    {
        var block = PlayUi.rect("friend_texts", inner);
        LobbyUi.place_fraction(block, NAME_LEFT, 0f, NAME_RIGHT, 1f);

        var name_text = PlayUi.text("friend_name", block, display_name(friend.name), NAME_MAX_SIZE, PlayTheme.lobby_text, TextAnchor.LowerLeft);
        LobbyUi.place_fraction(name_text.rectTransform, 0f, 0.5f, 1f, 0.9f);
        fit_text(name_text, NAME_MIN_SIZE, NAME_MAX_SIZE);

        var status_row = PlayUi.rect("friend_status_row", block);
        LobbyUi.place_fraction(status_row, 0f, 0.1f, 1f, 0.5f);

        var color = status_color(friend.online);

        var dot_slot = PlayUi.rect("status_dot_slot", status_row);
        dot_slot.anchorMin = new Vector2(0f, 0.3f);
        dot_slot.anchorMax = new Vector2(0f, 0.7f);
        dot_slot.pivot = new Vector2(0f, 0.5f);
        dot_slot.offsetMin = Vector2.zero;
        dot_slot.offsetMax = Vector2.zero;
        var dot_fit = dot_slot.gameObject.AddComponent<AspectRatioFitter>();
        dot_fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        dot_fit.aspectRatio = 1f;

        var dot = PlayUi.image("status_dot", dot_slot, PlayUi.pill());
        dot.type = Image.Type.Sliced;
        dot.color = color;
        PlayUi.stretch(dot.rectTransform);

        var status = PlayUi.text("status_text", status_row, friend.online ? ONLINE_LABEL : OFFLINE_LABEL, STATUS_MAX_SIZE, color, TextAnchor.MiddleLeft);
        LobbyUi.place_fraction(status.rectTransform, 0.14f, 0f, 1f, 1f);
        fit_text(status, STATUS_MIN_SIZE, STATUS_MAX_SIZE);
    }

    void build_button(RectTransform inner)
    {
        var root = PlayUi.rect("invite_button", inner);
        LobbyUi.place_fraction(root, BUTTON_LEFT, BUTTON_BOTTOM, 1f, BUTTON_TOP);

        button_group = root.gameObject.AddComponent<CanvasGroup>();

        button_ring = root.gameObject.AddComponent<Image>();
        button_ring.sprite = PlayUi.pill();
        button_ring.type = Image.Type.Sliced;
        button_ring.color = PlayTheme.friends_border;
        button_ring.raycastTarget = true;

        var fill = PlayUi.image("invite_fill", root, PlayUi.pill());
        fill.type = Image.Type.Sliced;
        fill.color = PlayTheme.lobby_inner_bg;
        PlayUi.stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(RING_THICKNESS, RING_THICKNESS);
        fill.rectTransform.offsetMax = new Vector2(-RING_THICKNESS, -RING_THICKNESS);

        var glow_rect = PlayUi.rect("invite_glow", root);
        PlayUi.stretch(glow_rect);
        var glow = glow_rect.gameObject.AddComponent<Image>();
        glow.sprite = PlayUi.pill();
        glow.type = Image.Type.Sliced;
        glow.color = PlayTheme.slot_glow;
        glow.raycastTarget = false;
        var glow_group = glow_rect.gameObject.AddComponent<CanvasGroup>();
        glow_group.alpha = 0f;
        glow_group.blocksRaycasts = false;
        glow_group.interactable = false;

        button_label = PlayUi.text("invite_label", root, INVITE_LABEL, BUTTON_MAX_SIZE, PlayTheme.pill_text, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(button_label.rectTransform, 0.08f, 0f, 0.92f, 1f);
        fit_text(button_label, BUTTON_MIN_SIZE, BUTTON_MAX_SIZE);

        button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = button_ring;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(on_click);

        button_fx = root.gameObject.AddComponent<PlayButtonFx>();
        button_fx.setup(root, glow_group);
    }

    void on_click()
    {
        if (friend.invited) return;

        LobbyState.invite(friend);
    }

    void set_enabled(bool value)
    {
        button.interactable = value;
        button_ring.raycastTarget = value;
        button_fx.enabled = value;
        button_group.alpha = value ? 1f : DISABLED_ALPHA;
    }

    static string initial_of(string value)
    {
        var clean = LobbyUi.sanitize(value);
        return clean.Length == 0 ? "?" : clean.Substring(0, 1).ToUpperInvariant();
    }

    static string display_name(string value)
    {
        var clean = LobbyUi.sanitize(value);
        return clean.Length == 0 ? "Unknown" : clean;
    }
}
