#if ODIN_INSPECTOR
using Microsoft.CSharp;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
#endif
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    #if ODIN_INSPECTOR
    public static class ConstantEditorEvents {
        public static Action OnConstantEditorPropChanged;
    }
    #endif

    public interface ISortKeyObject {
        public int SortKey { get; set; }
    }

    public interface IEditorObject {
        public string EditorGroup { get; set; }
        public string Comment { get; set; }
    }


    public class Constant<T> : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector]
        public string editorGroup;

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [LabelText("Group")]
        [ValueDropdown(nameof(GetDropdownItems), AppendNextDrawer = true)]
        [BoxGroup("Editor Props")]
        [LabelWidth(editorLabelWidth)]
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
        [LabelWidth(editorLabelWidth)]
        [PropertyOrder(5)]
        [DelayedProperty]
        [OnValueChanged(nameof(OnEditorPropChanged))]
        #endif
        public int SortKey { get => sortKey; set => sortKey = value; }

        [HideInInspector]
        public string comment;
        #if ODIN_INSPECTOR
        [ShowInInspector]
        [BoxGroup("Editor Props")]
        [LabelWidth(editorLabelWidth)]
        [PropertyOrder(10)]
        #endif
        public string Comment { get => comment; set => comment = value; }

        [SerializeField]
        #if ODIN_INSPECTOR
        [LabelWidth(editorLabelWidth)]
        [InlineProperty]
        [PropertyOrder(20)]
        #endif
        private T value = default;
        public T Value => value;

        public static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        [HideInInspector]
        [NonSerialized]
        public CodeTypeReference typeRef = new CodeTypeReference(typeof(T));

        [ShowInInspector]
        [ReadOnly]
        [LabelWidth(editorLabelWidth)]
        [PropertyOrder(11)]
        public string Type => codeProvider.GetTypeOutput(typeRef);

#if ODIN_INSPECTOR
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
            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();

            var seenGroups = new HashSet<string>();
            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);

                    var groupName = (constantAsset as IEditorObject).EditorGroup;
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
    }
}