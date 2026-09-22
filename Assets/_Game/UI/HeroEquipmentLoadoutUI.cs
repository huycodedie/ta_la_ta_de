using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WuxiaGame.Core;
using WuxiaGame.Items;
using WuxiaGame.Equipment;

namespace WuxiaGame.UI
{
    /// <summary>
    /// Data & visual representation of a single permanent equipment slot in the Hero Equipment Loadout.
    /// </summary>
    [Serializable]
    public class EquipmentSlotView
    {
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private GameObject slotRoot;
        [SerializeField] private Image slotBackground;
        [SerializeField] private Image slotBorder;
        [SerializeField] private TextMeshProUGUI slotTypeLabel;
        [SerializeField] private TextMeshProUGUI itemSymbolText;
        [SerializeField] private GameObject levelBadge;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject emptyIndicator;

        [System.NonSerialized] private EquipmentInstance currentItem;

        public EquipmentSlotType SlotType => slotType;
        public GameObject SlotRoot => slotRoot;
        public Image SlotBackground => slotBackground;
        public Image SlotBorder => slotBorder;
        public TextMeshProUGUI SlotTypeLabel => slotTypeLabel;
        public TextMeshProUGUI ItemSymbolText => itemSymbolText;
        public GameObject LevelBadge => levelBadge;
        public TextMeshProUGUI LevelText => levelText;
        public GameObject EmptyIndicator => emptyIndicator;
        public EquipmentInstance CurrentItem => currentItem;
        public bool IsEquipped => currentItem != null;

        public void BindReferences(
            EquipmentSlotType type,
            GameObject root,
            Image bg,
            Image border,
            TextMeshProUGUI label,
            TextMeshProUGUI symbol,
            GameObject lvlBadge,
            TextMeshProUGUI lvlText,
            GameObject emptyInd)
        {
            slotType = type;
            slotRoot = root;
            slotBackground = bg;
            slotBorder = border;
            slotTypeLabel = label;
            itemSymbolText = symbol;
            levelBadge = lvlBadge;
            levelText = lvlText;
            emptyIndicator = emptyInd;
        }

        public void SetEmpty(Color defaultBorderColor, Color defaultBgColor)
        {
            currentItem = null;
            if (emptyIndicator != null) emptyIndicator.SetActive(true);
            if (levelBadge != null) levelBadge.SetActive(false);
            if (slotBorder != null) slotBorder.color = defaultBorderColor;
            if (slotBackground != null) slotBackground.color = defaultBgColor;
            if (itemSymbolText != null) itemSymbolText.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }

        public void SetItem(EquipmentInstance item)
        {
            currentItem = item;
            if (item == null)
            {
                SetEmpty(new Color(0.35f, 0.32f, 0.28f, 0.6f), new Color(0.08f, 0.07f, 0.12f, 0.9f));
                return;
            }

            if (emptyIndicator != null) emptyIndicator.SetActive(false);
            if (levelBadge != null) levelBadge.SetActive(true);

            if (levelText != null)
            {
                levelText.text = $"Lv.{item.EquipmentLevel}";
            }

            Color rarityColor = item.Rarity != null ? item.Rarity.RarityColor : Color.white;
            if (slotBorder != null) slotBorder.color = rarityColor;
            if (itemSymbolText != null) itemSymbolText.color = rarityColor;
        }
    }

    /// <summary>
    /// Presentation-only component displaying the persistent Main Equipment Loadout on the Main Combat HUD.
    /// Shows currently worn equipment across all 12 supported EquipmentSlotType values.
    /// Strictly presentation layer; holds zero gameplay or mutation authority.
    /// </summary>
    public class HeroEquipmentLoadoutUI : MonoBehaviour
    {
        [Header("Visual Config")]
        [SerializeField] private Color emptyBorderColor = new Color(0.35f, 0.32f, 0.28f, 0.6f);
        [SerializeField] private Color emptyBgColor = new Color(0.08f, 0.07f, 0.12f, 0.9f);

        [Header("Loadout Slot Views")]
        [SerializeField] private List<EquipmentSlotView> slotViews = new List<EquipmentSlotView>();

        private readonly Dictionary<EquipmentSlotType, EquipmentSlotView> slotMap = new Dictionary<EquipmentSlotType, EquipmentSlotView>();

        public IReadOnlyList<EquipmentSlotView> SlotViews => slotViews;

        private void Awake()
        {
            BuildSlotMap();
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentEquipped += HandleEquipmentEquipped;
            RefreshAllSlotsFromAuthority();
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;
        }

        private void Start()
        {
            BuildSlotMap();
            RefreshAllSlotsFromAuthority();
        }

        public void SetReferences(List<EquipmentSlotView> views)
        {
            slotViews = views ?? new List<EquipmentSlotView>();
            BuildSlotMap();
            RefreshAllSlotsFromAuthority();
        }

        public void BuildSlotMap()
        {
            slotMap.Clear();
            if (slotViews == null) return;

            foreach (var view in slotViews)
            {
                if (view != null && !slotMap.ContainsKey(view.SlotType))
                {
                    slotMap[view.SlotType] = view;
                }
            }
        }

        public EquipmentSlotView GetSlotView(EquipmentSlotType slot)
        {
            if (slotMap.Count == 0) BuildSlotMap();
            slotMap.TryGetValue(slot, out var view);
            return view;
        }

        /// <summary>
        /// Reads authoritative EquipmentManager state and populates all 12 slots.
        /// </summary>
        public void RefreshAllSlotsFromAuthority()
        {
            if (slotMap.Count == 0) BuildSlotMap();

            var eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();

            foreach (EquipmentSlotType slot in SupportedSlots)
            {
                if (slotMap.TryGetValue(slot, out var view) && view != null)
                {
                    EquipmentInstance item = eqMgr != null ? eqMgr.GetEquippedItem(slot) : null;
                    if (item != null)
                    {
                        view.SetItem(item);
                    }
                    else
                    {
                        view.SetEmpty(emptyBorderColor, emptyBgColor);
                    }
                }
            }
        }

        private void HandleEquipmentEquipped(EquipmentSlotType slot, EquipmentInstance item)
        {
            if (slotMap.Count == 0) BuildSlotMap();

            if (slotMap.TryGetValue(slot, out var view) && view != null)
            {
                if (item != null)
                {
                    view.SetItem(item);
                }
                else
                {
                    view.SetEmpty(emptyBorderColor, emptyBgColor);
                }
            }
        }

        public static readonly EquipmentSlotType[] SupportedSlots = new EquipmentSlotType[]
        {
            EquipmentSlotType.Weapon,
            EquipmentSlotType.Armor,
            EquipmentSlotType.Helmet,
            EquipmentSlotType.Gloves,
            EquipmentSlotType.Boots,
            EquipmentSlotType.LeftRing,
            EquipmentSlotType.RightRing,
            EquipmentSlotType.Necklace,
            EquipmentSlotType.Accessory,
            EquipmentSlotType.Belt,
            EquipmentSlotType.Talisman,
            EquipmentSlotType.Shoulder
        };

        public static string GetSlotDisplayName(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return "Vũ Khí";
                case EquipmentSlotType.Armor: return "Áo Giáp";
                case EquipmentSlotType.Helmet: return "Mũ";
                case EquipmentSlotType.Gloves: return "Hộ Thủ";
                case EquipmentSlotType.Boots: return "Hài";
                case EquipmentSlotType.LeftRing: return "Giới Chỉ (T)";
                case EquipmentSlotType.RightRing: return "Giới Chỉ (P)";
                case EquipmentSlotType.Necklace: return "Hạng Liên";
                case EquipmentSlotType.Accessory: return "Pháp Bảo";
                case EquipmentSlotType.Belt: return "Yêu Đái";
                case EquipmentSlotType.Talisman: return "Bùa Chú";
                case EquipmentSlotType.Shoulder: return "Kiên Giáp";
                default: return slot.ToString();
            }
        }

        public static string GetSlotShortLabel(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return "VŨ";
                case EquipmentSlotType.Helmet: return "MŨ";
                case EquipmentSlotType.Armor: return "GIÁP";
                case EquipmentSlotType.Gloves: return "THỦ";
                case EquipmentSlotType.Boots: return "HÀI";
                case EquipmentSlotType.Belt: return "ĐÁI";
                case EquipmentSlotType.Necklace: return "HẠNG";
                case EquipmentSlotType.LeftRing: return "CHỈ T";
                case EquipmentSlotType.RightRing: return "CHỈ P";
                case EquipmentSlotType.Accessory: return "BẢO";
                case EquipmentSlotType.Talisman: return "BÙA";
                case EquipmentSlotType.Shoulder: return "KIÊN";
                default: return slot.ToString().Substring(0, Math.Min(3, slot.ToString().Length)).ToUpper();
            }
        }

        public static string GetSlotSymbolGlyph(EquipmentSlotType slot)
        {
            switch (slot)
            {
                case EquipmentSlotType.Weapon: return "⚔";
                case EquipmentSlotType.Helmet: return "⛑";
                case EquipmentSlotType.Armor: return "🛡";
                case EquipmentSlotType.Gloves: return "🧤";
                case EquipmentSlotType.Boots: return "🥾";
                case EquipmentSlotType.Belt: return "🎗";
                case EquipmentSlotType.Necklace: return "📿";
                case EquipmentSlotType.LeftRing: return "💍";
                case EquipmentSlotType.RightRing: return "💎";
                case EquipmentSlotType.Accessory: return "🔮";
                case EquipmentSlotType.Talisman: return "📜";
                case EquipmentSlotType.Shoulder: return "🔰";
                default: return "◈";
            }
        }
    }

    /// <summary>
    /// Legacy compatibility component retained for historical test suite invariant.
    /// </summary>
    [Obsolete("Replaced by HeroEquipmentLoadoutUI for persistent main loadout.")]
    public class LatestEquipmentHUDUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject emptyStateRoot;
        [SerializeField] private GameObject itemInfoRoot;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI itemLevelText;
        [SerializeField] private TextMeshProUGUI rarityText;

        [System.NonSerialized] private EquipmentInstance latestEquipment;

        public EquipmentInstance LatestEquipment => latestEquipment;
        public GameObject EmptyStateRoot => emptyStateRoot;
        public GameObject ItemInfoRoot => itemInfoRoot;
        public TextMeshProUGUI ItemNameText => itemNameText;
        public TextMeshProUGUI ItemTypeText => itemTypeText;
        public TextMeshProUGUI ItemLevelText => itemLevelText;
        public TextMeshProUGUI RarityText => rarityText;

        public static string GetSlotDisplayName(EquipmentSlotType slot)
        {
            return HeroEquipmentLoadoutUI.GetSlotDisplayName(slot);
        }

        public void ClearEquipment()
        {
            latestEquipment = null;
            UpdateDisplay();
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentDropped += HandleEquipmentDropped;
            UpdateDisplay();
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentDropped -= HandleEquipmentDropped;
        }

        public void SetReferences(
            GameObject emptyState,
            GameObject itemInfo,
            TextMeshProUGUI nameTmp,
            TextMeshProUGUI typeTmp,
            TextMeshProUGUI levelTmp,
            TextMeshProUGUI rarityTmp)
        {
            emptyStateRoot = emptyState;
            itemInfoRoot = itemInfo;
            itemNameText = nameTmp;
            itemTypeText = typeTmp;
            itemLevelText = levelTmp;
            rarityText = rarityTmp;
            UpdateDisplay();
        }

        private void HandleEquipmentDropped(EquipmentInstance item)
        {
            if (item == null) return;
            latestEquipment = item;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (latestEquipment == null || string.IsNullOrEmpty(latestEquipment.ItemName))
            {
                if (emptyStateRoot != null) emptyStateRoot.SetActive(true);
                if (itemInfoRoot != null) itemInfoRoot.SetActive(false);
                return;
            }

            if (emptyStateRoot != null) emptyStateRoot.SetActive(false);
            if (itemInfoRoot != null) itemInfoRoot.SetActive(true);

            if (itemNameText != null) itemNameText.text = latestEquipment.ItemName;
            if (itemTypeText != null) itemTypeText.text = HeroEquipmentLoadoutUI.GetSlotDisplayName(latestEquipment.SlotType);
            if (itemLevelText != null) itemLevelText.text = $"Lv.{latestEquipment.EquipmentLevel} / {latestEquipment.MaxLevel}";
            if (rarityText != null)
            {
                string rName = latestEquipment.Rarity != null ? latestEquipment.Rarity.DisplayName : "Thường";
                Color rColor = latestEquipment.Rarity != null ? latestEquipment.Rarity.RarityColor : Color.white;
                rarityText.text = rName;
                rarityText.color = rColor;
            }
        }
    }
}
