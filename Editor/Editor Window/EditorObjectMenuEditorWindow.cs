using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Vaflov.ContextMenuItemShortcutHandler;
using System;
using static Vaflov.EditorStringUtil;

namespace Vaflov {
    public abstract class EditorObjectMenuEditorWindow : OdinMenuEditorWindow {
        public IEditorObjectCreator editorObjectCreator;
        public abstract Type EditorObjBaseType { get; }
        public Type forcedDefaultType;

        public static T Open<T>(string title, string icon = null) where T : EditorObjectMenuEditorWindow {
            return Open<T>(title, icon, new Vector2Int(600, 400));
        }

        public static T Open<T>(string title, string icon, Vector2Int size) where T : EditorObjectMenuEditorWindow {
            var wasOpen = HasOpenInstances<T>();
            var window = GetWindow<T>();
            if (!wasOpen) {
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(size.x, size.y);
                window.MenuWidth = size.x / 2;
                var tex = string.IsNullOrEmpty(icon) ? null : Resources.Load<Texture2D>(icon);
                window.titleContent = new GUIContent(title, tex);
            }
            return window;
        }

        public void RebuildEditorGroupsOnPropChanged(ScriptableObject editorObject) {
            if (TypeUtil.IsInheritedFrom(editorObject.GetType(), EditorObjBaseType)) {
                RebuildEditorGroups();
            }
        }

        public void RebuildEditorGroups() {
            var oldSelectedObj = MenuTree.Selection?.FirstOrDefault()?.Value;
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(oldSelectedObj);
        }

        public virtual IEditorObjectCreator CreateEditorObjectCreator() => null;

        protected override void OnEnable() {
            base.OnEnable();
            editorObjectCreator = CreateEditorObjectCreator();
            EditorObject.OnEditorPropChanged += RebuildEditorGroupsOnPropChanged;
        }

        protected override void OnDisable() {
            base.OnDisable();
            EditorObject.OnEditorPropChanged -= RebuildEditorGroupsOnPropChanged;
        }

        public void TryOpenEditorObjectCreationMenu(Type defaultType = null) {
            if (editorObjectCreator == null)
                return;
            var selected = MenuTree?.Selection?.FirstOrDefault();
            forcedDefaultType = defaultType;
            if (selected == null || selected.Value is not IEditorObjectCreator) {
                editorObjectCreator.Reset(forcedDefaultType);
            }
            TrySelectMenuItemWithObject(editorObjectCreator);
        }

        protected override OdinMenuTree BuildMenuTree() {
            editorObjectCreator?.Reset(forcedDefaultType);
            if (forcedDefaultType != null) {
                forcedDefaultType = null;
            }

            var tree = new OdinMenuTree(true);
            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            tree.Config.AutoFocusSearchBar = false;
            var menuStyle = new OdinMenuStyle() {
                Borders = false,
                Height = 18,
                IconSize = 15f,
                TrianglePadding = 1.50f,
                AlignTriangleLeft = true,
            };
            tree.Config.DefaultMenuStyle = menuStyle;
            tree.DefaultMenuStyle = menuStyle;

            var types = TypeUtil.GetFlatTypesDerivedFrom(EditorObjBaseType);
            //var constantTypes = AppDomain.CurrentDomain.GetAssemblies()
            //    .SelectMany(assembly => assembly.GetTypes())
            //    .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract && IsInheritedFrom(type, typeof(Constant<>)))
            //    .ToList();

            var groups = new SortedDictionary<string, HashSet<UnityEngine.Object>>();
            foreach (var type in types) {
                var assetGuids = AssetDatabase.FindAssets($"t: {type}");
                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                    var groupName = (asset as IEditorObject).EditorGroup;
                    groupName = groupName == null || groupName == "" ? "Default" : groupName;

                    if (!groups.TryGetValue(groupName, out HashSet<UnityEngine.Object> groupResult)) {
                        groupResult = new HashSet<UnityEngine.Object>();
                        groups[groupName] = groupResult;
                    }
                    groupResult.Add(asset);
                }
            }

            foreach ((var groupName, var group) in groups) {
                var groupList = group.ToList();
                groupList.Sort((UnityEngine.Object obj1, UnityEngine.Object obj2) => {
                    var sortKey1 = (obj1 as ISortKeyObject).SortKey;
                    var sortKey2 = (obj2 as ISortKeyObject).SortKey;
                    if (sortKey1 != sortKey2) {
                        return sortKey1.CompareTo(sortKey2);
                    } else {
                        return obj1.name.CompareTo(obj2.name);
                    }
                });
                var groupResult = new HashSet<OdinMenuItem>();
                foreach (var editorObjAsset in groupList) {
                    var menuItem = new EditorObjectOdinMenuItem(tree, editorObjAsset.name, editorObjAsset);
                    menuItem.IconGetter = (editorObjAsset as IEditorObject).GetEditorIcon;
                    groupResult.Add(menuItem);
                    tree.AddMenuItemAtPath(groupResult, groupName, menuItem);
                }
                var groupMenuItem = tree.GetMenuItem(groupName);
                if (groupMenuItem != null) {
                    groupMenuItem.OnDrawItem += x => {
                        var itemCountLabel = $" {x.ChildMenuItems.Count}";
                        var labelStyle = x.IsSelected ? x.Style.SelectedLabelStyle : x.Style.DefaultLabelStyle;
                        var nameLabelSize = labelStyle.CalcSize(GUIHelper.TempContent(x.SmartName));
                        var valueRect = new Rect(x.LabelRect.x + nameLabelSize.x, x.LabelRect.y, x.LabelRect.width - nameLabelSize.x, x.LabelRect.height);
                        var valueContent = GUIHelper.TempContent(itemCountLabel);

                        GUIHelper.PushColor(Color.green);
                        GUI.Label(valueRect, valueContent, labelStyle);
                        GUIHelper.PopColor();
                    };
                }
            }

            tree.EnumerateTree().ForEach(menuItem => menuItem.Toggled = true);

            if (editorObjectCreator != null) {
                var editorObjectCreatorMenuItem = new EmptyOdinMenuItem(tree, editorObjectCreator.Description, editorObjectCreator);
                //tree.Selection.SelectionChanged += _ => {
                //    tree.Config.AutoFocusSearchBar = !editorObjectCreatorMenuItem.IsSelected;
                //    Debug.Log(tree.Config.AutoFocusSearchBar);
                //};
                tree.AddMenuItemAtPath("", editorObjectCreatorMenuItem);
            }

            return tree;
        }

        public virtual List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            if (editorObjectCreator != null) {
                items.Add(new OdinContextMenuItem(editorObjectCreator.Description, () => {
                    TryOpenEditorObjectCreationMenu();
                    // EditorIconsOverview.OpenEditorIconsOverview();
                }, KeyCode.N, EventModifiers.Control | EventModifiers.Shift, SdfIconType.PlusCircle));
            }
            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected != null && selected.Value is IEditorObject editorObject) {
                items.AddRange(editorObject.GetContextMenuItems().Where(cmi => cmi.showInToolbar));
            }
            return items;
        }

        protected override void OnBeginDrawEditors() {
            if (MenuTree == null)
                return;
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            if (MenuTree.Selection != null) {
                var selectedNames = MenuTree.Selection?.Select(selected => selected.Name);
                var selectionLabel = string.Join(", ", selectedNames);
                if (selectionLabel?.Length > 40) {
                    selectionLabel = selectionLabel.Substring(0, 40) + "...";
                }
                GUILayout.Label(selectionLabel);
            }

            var toolbarItems = GetToolbarItems();
            foreach (var contextMenuItem in toolbarItems) {
                if (SirenixEditorGUIUtil.ToolbarSDFIconButton(contextMenuItem.icon, toolbarHeight, tooltip: contextMenuItem.tooltip)) {
                    contextMenuItem.action?.Invoke();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

            HandleContextMenuItemShortcuts(toolbarItems);
        }
    }

    public interface IEditorObjectCreator {
        string Description { get; }
        void Reset(Type defaultType = null);
    }

    public class DefaultEditorObjectCreator : IEditorObjectCreator {
        [HideInInspector] public Type baseType;
        [HideInInspector] public string defaultName;
        [HideInInspector] public string description;
        [HideInInspector] public Action<string, Type> generatorFunc;
        [HideInInspector] public Func<Type, bool> typeFilter;
        [HideInInspector] public Type defaultType = typeof(int);
        [HideInInspector] public TypeDropdownFieldDrawer typeDropdownFieldDrawer;
        [HideInInspector] public string name;
        [HideInInspector] public string nameError;
        [HideInInspector] public List<string> assetNames;

        public const int labelWidth = 40;

        public DefaultEditorObjectCreator(Type baseType,
                                          string defaultName,
                                          string description,
                                          Action<string, Type> generatorFunc) {
            this.baseType = baseType;
            this.defaultName = defaultName;
            this.description = description;
            this.generatorFunc = generatorFunc;
            this.name = defaultName;
        }

        public DefaultEditorObjectCreator SetTypeFilter(Func<Type, bool> typeFilter, Type defaultType) {
            this.typeFilter = typeFilter;
            this.defaultType = defaultType;
            return this;
        }

        public string Description => description;

        public void Reset(Type defaultType = null) {
            var types = EditorTypeUtil.GatherPublicTypes();
            if (typeFilter != null) {
                types = types.Where(typeFilter).ToList();
            }
            typeDropdownFieldDrawer = new TypeDropdownFieldDrawer(types, defaultType ?? this.defaultType);
            assetNames = AssetUtil.GetAssetPathsForType(baseType);
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
                nameError = ValidateAssetName(name, assetNames);
            }

            var targetType = typeDropdownFieldDrawer.TypeField();
            var targetTypeError = targetType == null ? "Type is empty" : null;
            if (!string.IsNullOrEmpty(targetTypeError)) {
                SirenixEditorGUI.ErrorMessageBox(targetTypeError);
            }
            GUIHelper.PopLabelWidth();

            if (!string.IsNullOrEmpty(nameError) || !string.IsNullOrEmpty(targetTypeError)) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                generatorFunc?.Invoke(name, targetType);
            }
        }
    }

    public class EditorObjectMenuImportHook : AssetPostprocessor {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            var activeEditorObjMenus = EditorObjectMenuHook.GetActiveEditorObjectMenus();
            if (activeEditorObjMenus?.Count == 0)
                return;
            for (int i = 0; i < importedAssets.Length; ++i) {
                EditorObjectMenuHook.TrySelectMenuItemFromPath(importedAssets[i], activeEditorObjMenus);
            }
        }
    }

    public class EditorObjectMenuHook : AssetModificationProcessor {
        public static List<Type> editorObjectMenuTypes = null;
        public static List<Type> EditorObjectMenuTypes {
            get {
                editorObjectMenuTypes ??= TypeCache.GetTypesDerivedFrom<EditorObjectMenuEditorWindow>().ToList();
                return editorObjectMenuTypes;
            }
        }

        public static List<EditorObjectMenuEditorWindow> GetActiveEditorObjectMenus() {
            List<EditorObjectMenuEditorWindow> activeEditorObjMenus = null;
            foreach (var editorMenuType in EditorObjectMenuTypes) {
                var openMenus = Resources.FindObjectsOfTypeAll(editorMenuType);
                if (openMenus?.Length > 0) {
                    activeEditorObjMenus ??= new List<EditorObjectMenuEditorWindow>();
                    activeEditorObjMenus.Add((EditorObjectMenuEditorWindow)openMenus[0]);
                }
            }
            return activeEditorObjMenus;
        }

        public static void TrySelectMenuItemFromPath(string path, List<EditorObjectMenuEditorWindow> activeEditorObjMenus) {
            if (activeEditorObjMenus?.Count == 0)
                return;
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type == null)
                return;
            foreach (var editorObjMenu in activeEditorObjMenus) {
                if (TypeUtil.IsInheritedFrom(type, editorObjMenu.EditorObjBaseType)) {
                    //Debug.Log("Selecting " + path);
                    editorObjMenu.TrySelectMenuItemWithObject(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
                }
            }
        }

        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _) {
            TrySelectMenuItemFromPath(path, GetActiveEditorObjectMenus());
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
