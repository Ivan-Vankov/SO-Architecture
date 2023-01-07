using Microsoft.CSharp;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using System.CodeDom;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static Vaflov.Config;

namespace Vaflov {
    public class GameEventBase : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector]
        public string editorGroup;
#if ODIN_INSPECTOR
        [ShowInInspector]
        [LabelText("Group")]
        [ValueDropdown(nameof(GetDropdownItems), AppendNextDrawer = true)]
        [BoxGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(0)]
        [DelayedProperty]
        [OnValueChanged(nameof(OnEditorPropChanged))]
#endif
        public string EditorGroup { get => editorGroup; set => editorGroup = value; }

        [HideInInspector]
        public int sortKey;
#if ODIN_INSPECTOR
        [ShowInInspector]
        [BoxGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(5)]
        [DelayedProperty]
        [OnValueChanged(nameof(OnEditorPropChanged))]
#endif
        public int SortKey { get => sortKey; set => sortKey = value; }

        #if ODIN_INSPECTOR
        [LabelText("Comment")]
        [TextArea(1, 5)]
        [BoxGroup("Editor Props")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(10)]
        #endif
        public string comment;
        public string EditorComment { get => comment; set => comment = value; }

        #if ODIN_INSPECTOR
        [ReadOnly]
        [PropertyOrder(30)]
        [ListDrawerSettings(
            DraggableItems = false,
            Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10,
            ShowItemCount = false,
            HideRemoveButton = true)]
        #endif
        public List<GameEventListenerBase> listeners;

        public virtual Texture GetEditorIcon() => null;

        #if ODIN_INSPECTOR && UNITY_EDITOR
        [BoxGroup("Editor Props")]
        [OnInspectorGUI]
        public void ShowName() {
            // TODO: Add name validation, move this to an attribute drawer https://www.youtube.com/watch?v=v9yNUctD4Qg
            // Or at liest it should become multi-editing aware
            // It should use the same logic as the "new constant" name
            var oldName = name;
            GUIHelper.PushLabelWidth(70);
            var newName = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), oldName);
            GUIHelper.PopLabelWidth();
            if (oldName != newName) {
                var assetPath = AssetDatabase.GetAssetPath(this);
                AssetDatabase.RenameAsset(assetPath, newName);
                OnEditorPropChanged();
            }
        }

        public void OnEditorPropChanged() {
            ConstantEditorEvents.OnConstantEditorPropChanged?.Invoke();
        }

        public IEnumerable<string> GetDropdownItems() {
            var eventTypes = TypeCache.GetTypesDerivedFrom(typeof(GameEventBase))
                .Where(type => !type.IsGenericType)
                .ToList();

            var seenGroups = new HashSet<string>();
            foreach (var type in eventTypes) {
                var assetGuids = AssetDatabase.FindAssets($"t: {type}");
                foreach (var constantAssetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);

                    var groupName = (asset as IEditorObject).EditorGroup;
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
            foreach (var groupName in groupsList) {
                yield return groupName;
            }
        }
        #endif

        public virtual string EditorToString() {
            return ToString();
        }

        public virtual List<ContextMenuItem> GetContextMenuItems() {
            #if ODIN_INSPECTOR && UNITY_EDITOR
            return new List<ContextMenuItem> {
                new ContextMenuItem("Duplicate", () => {
                    var copy = Instantiate(this);
                    var path = AssetDatabase.GetAssetPath(this);
                    var newPath = AssetDatabase.GenerateUniqueAssetPath(path);
                    copy.name = newPath[(newPath.LastIndexOf('/') + 1) .. newPath.LastIndexOf('.')];

                    AssetDatabase.CreateAsset(copy, newPath);
                    AssetDatabase.SaveAssets();

                    ConstantEditorEvents.OnConstantDuplicated?.Invoke(copy);
                }, KeyCode.D, EventModifiers.Control, SdfIconType.Stickies),
                new ContextMenuItem("Delete", () => {
                    EditorUtil.TryDeleteAsset(this);
                }, KeyCode.Delete, icon: SdfIconType.Trash),
            };
            #else
            return null
            #endif
        }
    }
}
