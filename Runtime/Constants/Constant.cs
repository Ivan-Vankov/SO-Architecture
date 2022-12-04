﻿#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Vaflov.TypeUtil;

namespace Vaflov {
    public class Constant<T> : ScriptableObject {
        #if ODIN_INSPECTOR
        [LabelText("Group")]
        [ValueDropdown(nameof(GetDropdownItems), AppendNextDrawer = true)]
        [BoxGroup("Editor Props")]
        #endif
        public string editorGroup;

        #if ODIN_INSPECTOR
        [BoxGroup("Editor Props")]
        #endif
        public string comment;

        [SerializeField] private T value = default;
        public T Value => value;


        #if ODIN_INSPECTOR
        public IEnumerable<string> GetDropdownItems() {
            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                .Where(type => !type.IsGenericType)
                .ToList();

            var seenGroups = new HashSet<string>();
            foreach (var constantType in constantTypes) {
                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
                var groupField = GetFieldRecursive(constantType, nameof(editorGroup), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var constantAssetGuid in constantAssetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);

                    var groupName = groupField.GetValue(constantAsset) as string;
                    if (groupName == null || groupName == "") {
                        continue;
                    }
                    if (!seenGroups.Contains(groupName)) {
                        seenGroups.Add(groupName);
                        yield return groupName;
                    }
                }
            }
        }
        #endif
    }
}