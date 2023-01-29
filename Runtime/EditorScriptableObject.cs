#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public interface ISortKeyObject {
        public int SortKey { get; set; }
    }

    public interface IEditorObject {
        public string EditorGroup { get; set; }
        public string EditorComment { get; set; }
        public Texture GetEditorIcon();
        public string EditorToString();
        public List<OdinContextMenuItem> GetContextMenuItems();
    }

    [Serializable]
    public class EditorObject {
        #if UNITY_EDITOR
        public static event Action<ScriptableObject> OnEditorPropChanged;
        #endif

        #if ODIN_INSPECTOR && UNITY_EDITOR
        [LabelText("Group")]
        [ValueDropdown(nameof(GetEditorObjGroups), AppendNextDrawer = true)]
        [FoldoutGroup("Editor Props", true)]
        [LabelWidth(preferedEditorLabelWidth)]
        [HideInInlineEditors]
        [PropertyOrder(0)]
        [DelayedProperty]
        [OnValueChanged(nameof(EditorPropChanged))]
        #endif
        public string editorGroup;

        //#if ODIN_INSPECTOR
        //[FoldoutGroup("Editor Props")]
        //[LabelWidth(preferedEditorLabelWidth)]
        //[HideInInlineEditors]
        //[PropertyOrder(5)]
        //[DelayedProperty]
        //[OnValueChanged(nameof(EditorPropChanged))]
        //#endif
        [HideInInspector]
        public int sortKey;

        #if ODIN_INSPECTOR
        [LabelText("Comment")]
        [TextArea(1, 5)]
        [FoldoutGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [HideInInlineEditors]
        [PropertyOrder(10)]
        #endif
        public string editorComment;

        [HideInInspector]
        public ScriptableObject editorObjParent;
        [HideInInspector]
        public Type editorObjParentBaseType;

        public static int focusEditorObjName = 0;

        public void Init(ScriptableObject editorObjParent, Type editorObjParentBaseType) {
            this.editorObjParent = editorObjParent;
            this.editorObjParentBaseType = editorObjParentBaseType;
            //Debug.Assert(TypeUtil.IsInheritedFrom(editorObjParent.GetType(), editorObjParentBaseType));
        }

        public static void FocusEditorObjName() {
            focusEditorObjName = 4;
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        [FoldoutGroup("Editor Props")]
        [HideInInlineEditors]
        [OnInspectorGUI]
        public void ShowName() {
            // TODO: Make this multi-editing aware
            if (editorObjParent == null)
                return;
            var oldName = editorObjParent.name;
            GUIHelper.PushLabelWidth(70);
            GUI.SetNextControlName("EditorObjName");
            var newName = EditorGUILayout.DelayedTextField(GUIHelper.TempContent("Name"), oldName);
            if (focusEditorObjName > 0) {
                if (GUI.GetNameOfFocusedControl() == "EditorObjName") {
                    focusEditorObjName = 0;
                } else {
                    EditorGUI.FocusTextInControl("EditorObjName");
                    --focusEditorObjName;
                }
            }
            GUIHelper.PopLabelWidth();
            if (oldName != newName) {
                var assetPath = AssetDatabase.GetAssetPath(editorObjParent);
                AssetDatabase.RenameAsset(assetPath, newName);
                EditorPropChanged();
            }
        }

        public void EditorPropChanged() {
            #if UNITY_EDITOR
            OnEditorPropChanged?.Invoke(editorObjParent);
            #endif
        }

        public IEnumerable<string> GetEditorObjGroups() {
            var editorObjTypes = TypeUtil.GetFlatTypesDerivedFrom(editorObjParentBaseType);
            if (editorObjTypes == null)
                Array.Empty<string>();

            var seenGroups = new HashSet<string>();
            foreach (var editorObjType in editorObjTypes) {
                var editorObjAssetGuids = AssetDatabase.FindAssets($"t: {editorObjType}");
                foreach (var editorObjAssetGuid in editorObjAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(editorObjAssetGuid);
                    var editorObjAsset = AssetDatabase.LoadAssetAtPath(assetPath, editorObjType);

                    var groupName = (editorObjAsset as IEditorObject).EditorGroup;
                    if (groupName == null) {
                        continue;
                    }
                    if (!seenGroups.Contains(groupName)) {
                        seenGroups.Add(groupName);
                    }
                }
            }
            var groupsList = new List<string>(seenGroups);
            groupsList.Sort();
            return groupsList;
        }
        #endif

        public List<OdinContextMenuItem> GetDefaultContextMenuItems() {
            #if ODIN_INSPECTOR && UNITY_EDITOR
            return new List<OdinContextMenuItem> {
                new OdinContextMenuItem("Rename", () => {
                    FocusEditorObjName();
                }, icon: SdfIconType.Pen) {
                    showInToolbar = false,
                },
                new OdinContextMenuItem("Duplicate", () => {
                    var copy = UnityEngine.Object.Instantiate(editorObjParent);
                    var path = AssetDatabase.GetAssetPath(editorObjParent);
                    var newPath = AssetDatabase.GenerateUniqueAssetPath(path);
                    copy.name = newPath[(newPath.LastIndexOf('/') + 1) .. newPath.LastIndexOf('.')];

                    AssetDatabase.CreateAsset(copy, newPath);
                    AssetDatabase.SaveAssets();
                }, KeyCode.D, EventModifiers.Control, SdfIconType.Stickies),
                new OdinContextMenuItem("Delete", () => {
                    EditorUtil.TryDeleteAsset(editorObjParent);
                }, KeyCode.Delete, icon: SdfIconType.Trash),
            };
            #else
            return null;
            #endif
        }
    }

    public class OdinContextMenuItem {
        public string name;
        public Action action;
        public KeyCode shortcut;
        public EventModifiers modifiers;
        #if ODIN_INSPECTOR && UNITY_EDITOR
        public SdfIconType icon;
        #endif
        public string shortcutFormated;
        public string tooltip;
        public bool showInToolbar = true;

        public OdinContextMenuItem(string name, Action action,
                                   KeyCode shortcut = KeyCode.None,
                                   EventModifiers modifiers = EventModifiers.None) {
            this.name = name;
            this.action = action;
            this.shortcut = shortcut;
            this.modifiers = modifiers;
            this.shortcutFormated = FormatShortcut();
            this.tooltip = FormatTooltip();
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        public OdinContextMenuItem(string name, Action action,
                                   KeyCode shortcut = KeyCode.None,
                                   EventModifiers modifiers = EventModifiers.None,
                                   SdfIconType icon = SdfIconType.None) {
            this.name = name;
            this.action = action;
            this.shortcut = shortcut;
            this.modifiers = modifiers;
            this.icon = icon;
            this.shortcutFormated = FormatShortcut();
            this.tooltip = FormatTooltip();
        }
        #endif

        public string FormatShortcut() {
            if (shortcut == KeyCode.None)
                return null;
            var shortcutStrBuilder = new StringBuilder();
            if (modifiers != EventModifiers.None) {
                if (modifiers.HasFlag(EventModifiers.Control)) {
                    shortcutStrBuilder.Append("Ctrl+");
                }
                if (modifiers.HasFlag(EventModifiers.Alt)) {
                    shortcutStrBuilder.Append("Alt+");
                }
                if (modifiers.HasFlag(EventModifiers.Shift)) {
                    shortcutStrBuilder.Append("Shift+");
                }
            }
            shortcutStrBuilder.Append(shortcut);
            return shortcutStrBuilder.ToString();
        }

        public string FormatTooltip() {
            if (string.IsNullOrEmpty(shortcutFormated)) {
                return name;
            } else {
                return $"{name} ({shortcutFormated})";
            }
        }
    }

    public abstract class EditorScriptableObject : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector] public EditorObject editorObj = null;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker, DisableContextMenu]
        public EditorObject EditorObject {
            get {
                editorObj ??= new EditorObject();
                editorObj.Init(this, EditorObjectBaseType);
                return editorObj;
            }
            set => Debug.Assert(true, "Editing editor object internals");
        }

        public virtual Type EditorObjectBaseType => GetType();
        public string EditorGroup { get => EditorObject.editorGroup; set => EditorObject.editorGroup = value; }
        public int SortKey { get => EditorObject.sortKey; set => EditorObject.sortKey = value; }
        public string EditorComment { get => EditorObject.editorComment; set => EditorObject.editorComment = value; }

        public virtual Texture GetEditorIcon() => null;
        public virtual string EditorToString() => null;

        public virtual List<OdinContextMenuItem> GetContextMenuItems() {
            return editorObj?.GetDefaultContextMenuItems();
        }

        [PropertyOrder(10000)]
        [HideInInlineEditors]
        public OpenInEditorWindowButton openInEditorWindowButton;
    }

    [Serializable]
    public class OpenInEditorWindowButton { }
}
