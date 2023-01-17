#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public static class ConstantEditorEvents {
        public static Action OnConstantEditorPropChanged;
        public static Action<ScriptableObject> OnConstantDuplicated;
    }

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

        public OdinContextMenuItem(string name, Action action,
                               KeyCode shortcut = KeyCode.None,
                               EventModifiers modifiers = EventModifiers.None,
                               #if ODIN_INSPECTOR && UNITY_EDITOR
                               SdfIconType icon = SdfIconType.None
                               #endif
                               ) {
            this.name = name;
            this.action = action;
            this.shortcut = shortcut;
            this.modifiers = modifiers;
            this.icon = icon;
            this.shortcutFormated = FormatShortcut();
            this.tooltip = FormatTooltip();
        }

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

        public static List<OdinContextMenuItem> GetDefaultContextMenuItems(ScriptableObject so, Action<ScriptableObject> onSODuplicated) {
            #if ODIN_INSPECTOR && UNITY_EDITOR
            return new List<OdinContextMenuItem> {
                new OdinContextMenuItem("Duplicate", () => {
                    var copy = UnityEngine.Object.Instantiate(so);
                    var path = AssetDatabase.GetAssetPath(so);
                    var newPath = AssetDatabase.GenerateUniqueAssetPath(path);
                    copy.name = newPath[(newPath.LastIndexOf('/') + 1) .. newPath.LastIndexOf('.')];

                    AssetDatabase.CreateAsset(copy, newPath);
                    AssetDatabase.SaveAssets();

                    onSODuplicated?.Invoke(copy);
                }, KeyCode.D, EventModifiers.Control, SdfIconType.Stickies),
                new OdinContextMenuItem("Rename", () => {
                    EditorObject.FocusEditorObjName();
                    //EditorGUI.FocusTextInControl("EditorObjName");
                }, icon: SdfIconType.Pen),
                new OdinContextMenuItem("Delete", () => {
                    EditorUtil.TryDeleteAsset(so);
                }, KeyCode.Delete, icon: SdfIconType.Trash),
            };
            #else
            return null
        #endif
        }
    }

    [Serializable]
    public class EditorObject {
        #if ODIN_INSPECTOR && UNITY_EDITOR
        [LabelText("Group")]
        [ValueDropdown(nameof(GetEditorObjGroups), AppendNextDrawer = true)]
        [FoldoutGroup("Editor Props", true)]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(0)]
        [DelayedProperty]
        [OnValueChanged(nameof(EditorPropChanged))]
        #endif
        public string editorGroup;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(5)]
        [DelayedProperty]
        [OnValueChanged(nameof(EditorPropChanged))]
        #endif
        public int sortKey;

        #if ODIN_INSPECTOR
        [LabelText("Comment")]
        [TextArea(1, 5)]
        [FoldoutGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(10)]
        #endif
        public string editorComment;

        [HideInInspector] 
        public ScriptableObject editorObjParent;
        [HideInInspector] 
        public Type editorObjParentBaseType;
        [HideInInspector] 
        public Action onEditorPropChanged;

        public static int focusEditorObjName = 0;

        public void Init(ScriptableObject editorObjParent, Type editorObjParentBaseType, Action onEditorPropChanged) {
            this.editorObjParent = editorObjParent;
            this.editorObjParentBaseType = editorObjParentBaseType;
            //Debug.Assert(editorObjParentBaseType.IsAssignableFrom(editorObjParent.GetType()));
            this.onEditorPropChanged = onEditorPropChanged;
        }

        public static void FocusEditorObjName() {
            focusEditorObjName = 4;
        }

        #if ODIN_INSPECTOR && UNITY_EDITOR
        [FoldoutGroup("Editor Props")]
        [OnInspectorGUI]
        public void ShowName() {
            // TODO: Add name validation, move this to an attribute drawer https://www.youtube.com/watch?v=v9yNUctD4Qg
            // Or at liest it should become multi-editing aware
            // It should use the same logic as the "new constant" name
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
            onEditorPropChanged?.Invoke();
        }

        public IEnumerable<string> GetEditorObjGroups() {
            if (editorObjParentBaseType == null)
                Array.Empty<string>();
            var editorObjTypes = TypeCache.GetTypesDerivedFrom(editorObjParentBaseType)
                .Where(type => !type.IsGenericType)
                .ToList();
            if (!editorObjParentBaseType.IsGenericType)
                editorObjTypes.Add(editorObjParentBaseType);

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
    }

    public class Constant<T> : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector] public EditorObject editorObj = null;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker, DisableContextMenu]
        public EditorObject EditorObject {
            get {
                editorObj ??= new EditorObject();
                editorObj.Init(this, typeof(Constant<>), ConstantEditorEvents.OnConstantEditorPropChanged);
                return editorObj;
            }
            set => Debug.Assert(true, "Editing editor object internals");
        }

        public string EditorGroup { get => EditorObject.editorGroup; set => EditorObject.editorGroup = value; }
        public int SortKey { get => EditorObject.sortKey; set => EditorObject.sortKey = value; }
        public string EditorComment { get => EditorObject.editorComment; set => EditorObject.editorComment = value; }

        public virtual Texture GetEditorIcon() => null;

        [SerializeField]
        #if ODIN_INSPECTOR
        [HideLabel]
        [PropertyOrder(20)]
        #endif
        private T value = default;
        public T Value => value;

        public static readonly CodeTypeReference typeRef = new CodeTypeReference(typeof(T));

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [ReadOnly]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(11)]
        #endif
        public string Type => codeProvider.GetTypeOutput(typeRef);

        public virtual string EditorToString() {
            if (Value == null) {
                return "null";
            }
            var str = Value.ToString();
            if (str.Length > 100) {
                str = "...";
            } else {
                str = str.Replace("\n", " ");
            }
            return str;
        }

        public virtual List<OdinContextMenuItem> GetContextMenuItems() {
            return OdinContextMenuItem.GetDefaultContextMenuItems(this, ConstantEditorEvents.OnConstantDuplicated);
        }
    }
}