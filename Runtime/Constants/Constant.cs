#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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
        #if ODIN_INSPECTOR
        [ShowInInspector]
        [LabelText("Group")]
        [ValueDropdown(nameof(GetDropdownItems), AppendNextDrawer = true)]
        [BoxGroup("Editor Props")]
        [PropertyOrder(0)]
        [OnValueChanged(nameof(OnEditorPropChanged))]
        #endif
        public string EditorGroup { get; set; }

        [ShowInInspector]
        [BoxGroup("Editor Props")]
        [PropertyOrder(5)]
        [OnValueChanged(nameof(OnEditorPropChanged))]
        public int SortKey { get; set; }

        #if ODIN_INSPECTOR
        [ShowInInspector]
        [BoxGroup("Editor Props")]
        [PropertyOrder(10)]
        [OnValueChanged(nameof(OnEditorPropChanged))]
        #endif
        public string Comment { get; set; }

        [SerializeField]
        [PropertyOrder(20)]
        private T value = default;
        public T Value => value;

        #if ODIN_INSPECTOR
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