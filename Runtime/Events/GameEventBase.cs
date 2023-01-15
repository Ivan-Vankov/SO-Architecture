#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
#endif
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using static Vaflov.Config;
using System.Runtime.CompilerServices;

namespace Vaflov {
    [HideInPlayMode]
    [HideLabel]
    [Serializable]
    public class GameEventEditMenu { }

    [Serializable]
    public class ObjSet<T> where T: UnityEngine.Object {
        [HideInInspector]
        public string name = "Objects";

        //[ReadOnly]
        [LabelText("$" + nameof(name))]
        [ListDrawerSettings(
            DraggableItems = false,
            //Expanded = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 10
            //ShowItemCount = false
            //HideRemoveButton = true
            )]
        public List<T> objects = new List<T>();
        public Dictionary<T, int> objToIdxs = new Dictionary<T, int>();

        public void Add(T listener) {
            if (!objToIdxs.ContainsKey(listener)) {
                objects.Add(listener);
                objToIdxs[listener] = objects.Count - 1;
            }
        }

        public void Remove(T obj) {
            if (!objToIdxs.ContainsKey(obj))
                return;
            var idx = objToIdxs[obj];
            objects[idx] = objects[objects.Count - 1];
            objects.RemoveAt(objects.Count - 1);
            if (idx < objects.Count) {
                objToIdxs[objects[idx]] = idx;
            }
        }

        public int Count => objects.Count;
    }

    public class GameEventBase : ScriptableObject, ISortKeyObject, IEditorObject {
        [HideInInspector]
        public string editorGroup;
#if ODIN_INSPECTOR
        [ShowInInspector]
        [LabelText("Group")]
        [ValueDropdown(nameof(GetDropdownItems), AppendNextDrawer = true)]
        [FoldoutGroup("Editor Props", Expanded = true)]
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
        [FoldoutGroup("Editor Props", Expanded = true)]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(5)]
        [DelayedProperty]
        [OnValueChanged(nameof(OnEditorPropChanged))]
#endif
        public int SortKey { get => sortKey; set => sortKey = value; }

#if ODIN_INSPECTOR
        [LabelText("Comment")]
        [TextArea(1, 5)]
        [FoldoutGroup("Editor Props", Expanded = true)]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(10)]
#endif
        public string comment;
        public string EditorComment { get => comment; set => comment = value; }

        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(30)]
        public ObjSet<Component> componentListeners = new ObjSet<Component>() { name = "Component Listeners" };

        [HideLabel]
        [FoldoutGroup("Listeners", true)]
        [PropertyOrder(31)]
        public ObjSet<ScriptableObject> SOListeners = new ObjSet<ScriptableObject>() { name = "SO Listeners" };

        public void AddListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour) {
                componentListeners.Add(Unsafe.As<MonoBehaviour>(listener.parent));
            } else {
                SOListeners.Add(Unsafe.As<ScriptableObject>(listener.parent));
            }
        }

        public void RemoveListener(GameEventListenerBase listener) {
            if (listener.parent is MonoBehaviour) {
                componentListeners.Remove(Unsafe.As<MonoBehaviour>(listener.parent));
            } else {
                SOListeners.Remove(Unsafe.As<ScriptableObject>(listener.parent));
            }
        }

        public virtual Texture GetEditorIcon() => null;

#if ODIN_INSPECTOR && UNITY_EDITOR
        [FoldoutGroup("Editor Props", Expanded = true)]
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

        [PropertyOrder(40)]
        public GameEventEditMenu editMenu;

        public virtual string EditorToString() => null;

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
