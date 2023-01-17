#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using System;
using Sirenix.OdinInspector;
using static Vaflov.StringUtil;

namespace Vaflov {
    public class ConstantsEditorWindow : EditorObjectMenuEditorWindow {
        public static readonly Vector2Int DEFAULT_EDITOR_SIZE = new Vector2Int(600, 400);

        public override Type EditorObjBaseType => typeof(Constant<>);

        public CreateNewConstant newConstantCreator;

        [MenuItem("Tools/SO Architecture/Constants Editor")]
        public static ConstantsEditorWindow Open() {
            return Open<ConstantsEditorWindow>("Constants", DEFAULT_EDITOR_SIZE, "pi");
        }

        protected override void OnEnable() {
            newConstantCreator = new CreateNewConstant();
            base.OnEnable();
            ConstantEditorEvents.OnConstantEditorPropChanged += RebuildEditorGroups;
            ConstantEditorEvents.OnConstantDuplicated += TrySelectMenuItemWithObject;
            ConstantsGenerator.OnConstantAssetGenerated += TrySelectMenuItemWithObject;
        }

        protected override void OnDisable() {
            base.OnDisable();
            ConstantEditorEvents.OnConstantEditorPropChanged -= RebuildEditorGroups;
            ConstantEditorEvents.OnConstantDuplicated -= TrySelectMenuItemWithObject;
            ConstantsGenerator.OnConstantAssetGenerated -= TrySelectMenuItemWithObject;
        }

        public void OpenConstantCreationMenu() {
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected == null || selected.Value is not CreateNewConstant) {
                newConstantCreator.name = CreateNewConstant.DEFAULT_CONSTANT_NAME;
            }
            TrySelectMenuItemWithObject(newConstantCreator);
        }

        protected override OdinMenuTree BuildMenuTree() {
            var tree = base.BuildMenuTree();
            newConstantCreator.ResetCachedTypes();
            var constantCreatorMenuItem = new EmptyOdinMenuItem(tree, "Add a new constant", newConstantCreator);
            //tree.Selection.SelectionChanged += _ => {
            //    tree.Config.AutoFocusSearchBar = !constantCreatorMenuItem.IsSelected;
            //    Debug.Log(tree.Config.AutoFocusSearchBar);
            //};
            tree.AddMenuItemAtPath("", constantCreatorMenuItem);

            return tree;
        }

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.Add(new OdinContextMenuItem("Add a new constant", () => {
                OpenConstantCreationMenu();
                // EditorIconsOverview.OpenEditorIconsOverview();
            }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            items.AddRange(base.GetToolbarItems());
            items.Add(new OdinContextMenuItem("Regenerate constants", () => {
                ConstantsGenerator.GenerateConstants();
                //ForceMenuTreeRebuild();
            }, KeyCode.S, EventModifiers.Control, SdfIconType.ArrowRepeat));
            return items;
        }
    }

    public class CreateNewConstant {
        [HideInInspector]
        public Type targetType = typeof(int);

        public const string DEFAULT_CONSTANT_NAME = "New Constant";

        [HideInInspector]
        public string name = DEFAULT_CONSTANT_NAME;

        [HideInInspector]
        public string nameError;

        [HideInInspector]
        public List<string> constantNames;

        [HideInInspector]
        public List<Type> types;

        [HideInInspector]
        public VaflovTypeSelector typeSelector;

        public const int labelWidth = 40;

        public void ResetCachedTypes() {
            types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.GameTypes | AssemblyTypeFlags.PluginEditorTypes).Where(x => {
                if (x.Name == null || x.IsGenericType || x.IsNotPublic)
                    return false;
                string text = x.Name.TrimStart(Array.Empty<char>());
                return text.Length != 0 && char.IsLetter(text[0]);
            }).ToList();
            typeSelector = new VaflovTypeSelector(types, supportsMultiSelect: false) {
                //FlattenTree = true,
            };
            typeSelector.SelectionChanged += types => {
                targetType = types.FirstOrDefault();
            };

            constantNames = new List<string>();
            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();
            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);
                    constantNames.Add(constantAsset.name);
                }
            }
        }

        public string ValidateConstantNameUniqueness(string targetName) {
            for (int i = 0; i < constantNames.Count; ++i) {
                if (string.Compare(constantNames[i], targetName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return "Name is not unique";
                }
            }
            targetName = targetName.RemoveWhitespaces();
            if (targetName.Length == 0)
                return "Name is empty";
            if (targetName[0] != '_' && !char.IsLetter(targetName[0]))
                return "First character should be _ or a letter";
            for (int i = 1; i < targetName.Length; ++i) {
                var c = targetName[i];
                if (!char.IsLetter(c) && !char.IsDigit(c) && c != '_')
                    return "Name contains a character that is not \'_\', a letter or a digit";
            }
            return null;
        }

        private OdinSelector<Type> SelectType(Rect arg) {
            typeSelector.SetSelection(targetType);
            typeSelector.ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
            return typeSelector;
        }

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            if (!string.IsNullOrEmpty(nameError)) {
                SirenixEditorGUI.ErrorMessageBox(nameError);
            }
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), name);
            if (name != oldName) {
                nameError = ValidateConstantNameUniqueness(name);
            }
            GUIHelper.PopLabelWidth();

            var targetTypeError = targetType == null ? "Type is empty" : null;
            if (!string.IsNullOrEmpty(targetTypeError)) {
                SirenixEditorGUI.ErrorMessageBox(targetTypeError);
            }
            var typeText = targetType == null ? "Select Type" : targetType.GetNiceFullName();
            var typeTextContent = new GUIContent(typeText);
            var typeTextStyle = EditorStyles.layerMaskField;
            var rect = GUILayoutUtility.GetRect(typeTextContent, typeTextStyle);
            var typeLabelRect = rect.SetSize(labelWidth, rect.height);
            var typeSelectorRect = new Rect(rect.x + labelWidth + 2, rect.y, Max(rect.width - labelWidth - 2, 0), rect.height);
            EditorGUI.LabelField(typeLabelRect, GUIHelper.TempContent("Type"));
            OdinSelector<Type>.DrawSelectorDropdown(typeSelectorRect, typeTextContent, SelectType, typeTextStyle);

            if (!string.IsNullOrEmpty(nameError) || !string.IsNullOrEmpty(targetTypeError)) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                ConstantsGenerator.GenerateConstantAsset(name, targetType);
            }
        }
    }
}

#endif