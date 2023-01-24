#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
#endif
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Vaflov.Config;

namespace Vaflov {
    public class ExampleAchievement : EditorScriptableObject {
        #if ODIN_INSPECTOR
        [FoldoutGroup("Achievement")]
        [HorizontalGroup("Achievement/Split", width: 80)]
        [PreviewField(Height = 80)]
        [HideLabel]
        [PropertyOrder(50)]
        #endif
        public Sprite achievementImage;


        #if ODIN_INSPECTOR
        [FoldoutGroup("Achievement")]
        [HorizontalGroup("Achievement/Split")]
        [VerticalGroup("Achievement/Split/Names")]
        [LabelText("Name")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(55)]
        #endif
        public string achievementName;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Achievement")]
        [HorizontalGroup("Achievement/Split")]
        [VerticalGroup("Achievement/Split/Names")]
        [LabelText("Description")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(60)]
        #endif
        public string achievementDescription;

        #if ODIN_INSPECTOR
        [FoldoutGroup("Achievement")]
        [HorizontalGroup("Achievement/Split")]
        [VerticalGroup("Achievement/Split/Names")]
        [ShowInInspector]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(62)]
        #endif
        public bool Unlocked { 
            get {
                var unlocker = ExampleAchievementUnlocker.Instance;
                if (unlocker) {
                    return unlocker.unlockedAchievements.Contains(this);
                }
                return false;
            }
        }

        #if ODIN_INSPECTOR
        [FoldoutGroup("Achievement")]
        [PropertyOrder(65)]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(
            Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10,
            CustomAddFunction = nameof(AddListener),
            CustomRemoveElementFunction = nameof(RemoveListener)
        )]
        #endif
        public List<GameEventListenerSO> listeners = new List<GameEventListenerSO>();

        public GameEventListenerSO AddListener() {
            var listener = CreateInstance<GameEventListenerSO>();
            listener.name = $"{name} Listener {listeners.Count}";
            listener.EditorGroup = "Achievements";

            AssetDatabase.AddObjectToAsset(listener, this);
            AssetDatabase.SaveAssets();
            return listener;
        }

        public void RemoveListener(GameEventListenerSO listener) {
            if (listeners.Remove(listener) && listener) {
                AssetDatabase.RemoveObjectFromAsset(listener);
                AssetDatabase.SaveAssets();
            }
        }
    }
}