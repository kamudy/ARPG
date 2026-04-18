using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform root;   // el panel del tooltip
    public TMP_Text titleText;
    public TMP_Text bodyText;

    [Header("Follow Mouse")]
    public Vector2 offset = new Vector2(0, -18);       // debajo del mouse
    public Vector2 paddingFromScreen = new Vector2(10, 10);

    RectTransform canvasRect;

    void Awake()
    {
        canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
        Hide();
    }

    void Update()
    {
        if (root != null && root.gameObject.activeSelf)
            FollowMouseClamped();
    }

    public void Show(ItemData item, PlayerStats stats, PlayerLevel level)
    {
        if (item == null || root == null) return;

        root.gameObject.SetActive(true);

        if (titleText != null) titleText.text = item.itemName;
        if (bodyText != null) bodyText.text = BuildTooltip(item, stats, level);

        Canvas.ForceUpdateCanvases();
        FollowMouseClamped();
    }

    public void Hide()
    {
        if (root != null) root.gameObject.SetActive(false);
    }

    void FollowMouseClamped()
    {
        if (canvasRect == null || root == null) return;
        if (Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        // Convertir Screen -> Local dentro del Canvas (Overlay => cam = null)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, mouseScreen, null, out Vector2 mouseLocal);

        Vector2 size = root.rect.size;

        // debajo del mouse (centrado en X)
        float x = mouseLocal.x - (size.x * 0.5f) + offset.x;
        float y = mouseLocal.y - size.y + offset.y;

        Rect c = canvasRect.rect;

        float minX = c.xMin + paddingFromScreen.x;
        float maxX = c.xMax - paddingFromScreen.x - size.x;
        float minY = c.yMin + paddingFromScreen.y;
        float maxY = c.yMax - paddingFromScreen.y - size.y;

        x = Mathf.Clamp(x, minX, maxX);
        y = Mathf.Clamp(y, minY, maxY);

        root.anchoredPosition = new Vector2(x, y);
    }

    string BuildTooltip(ItemData item, PlayerStats stats, PlayerLevel level)
    {
        var sb = new StringBuilder();

        // ===== REQUERIMIENTOS =====
        sb.AppendLine("<b>Requerimientos</b>");

        int playerLvl = (level != null) ? level.level : 1;
        int str = (stats != null) ? stats.strength : 0;
        int agi = (stats != null) ? stats.agility : 0;
        int vit = (stats != null) ? stats.vitality : 0;
        int ene = (stats != null) ? stats.energy : 0;

        bool anyReq = false;
        anyReq |= AppendReq(sb, item.reqLevel, playerLvl, "Nivel");
        anyReq |= AppendReq(sb, item.reqStrength, str, "STR");
        anyReq |= AppendReq(sb, item.reqAgility, agi, "AGI");
        anyReq |= AppendReq(sb, item.reqVitality, vit, "VIT");
        anyReq |= AppendReq(sb, item.reqEnergy, ene, "ENE");

        if (!anyReq)
            sb.AppendLine("<color=#9BD67A>Sin requerimientos</color>");

        // ===== STATS DEL ITEM =====
        sb.AppendLine();
        sb.AppendLine("<b>Stats del item</b>");

        bool anyStat = false;

        if (item.itemType == ItemType.Armor && item.defense != 0)
        {
            sb.AppendLine($"+{item.defense} Defensa");
            anyStat = true;
        }

        if (item.itemType == ItemType.Potion && item.healAmount > 0)
        {
            sb.AppendLine($"+{item.healAmount} HP");
            anyStat = true;
        }

        if (!anyStat)
            sb.AppendLine("<color=#bfbfbf>(No da stats)</color>");

        return sb.ToString();
    }

    bool AppendReq(StringBuilder sb, int req, int current, string label)
    {
        if (req <= 0) return false;

        bool ok = current >= req;

        if (ok)
            sb.AppendLine($"{label}: {req}  <color=#9BD67A>OK</color>");
        else
            sb.AppendLine($"<color=#ff4040>{label}: {req} (tenés {current})</color>");

        return true;
    }
}
