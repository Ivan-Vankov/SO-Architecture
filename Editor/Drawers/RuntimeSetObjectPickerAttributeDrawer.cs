using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    [DrawerPriority(0.0, 0.0, 2003.0)]
    public class RuntimeSetObjectPickerAttributeDrawer : OdinAttributeDrawer<RuntimeSetObjectPickerAttribute> {
        private string error;
        public ObjectPicker objectPicker;
        private ValueResolver<Type> typeGetter;

        protected override bool CanDrawAttributeProperty(InspectorProperty property) {
            return property.ValueEntry != null;
        }

        //
        // Summary:
        //     Initializes this instance.
        protected override void Initialize() {
            typeGetter = ValueResolver.Get<Type>(Property, Attribute.type);
            error = typeGetter.ErrorMessage;
        }

        //
        // Summary:
        //     Draws the property with GUILayout support. This method is called by DrawPropertyImplementation
        //     if the GUICallType is set to GUILayout, which is the default.
        protected override void DrawPropertyLayout(GUIContent label) {
            if (Property.ValueEntry == null) {
                CallNextDrawer(label);
            } else if (error != null) {
                SirenixEditorGUI.ErrorMessageBox(error);
                CallNextDrawer(label);
            } else {
                CollectionDrawerStaticInfo.NextCustomAddFunction = OpenObjectPicker;
                CallNextDrawer(label);
                if (objectPicker != null && Event.current.commandName == "ObjectSelectorSelectionDone") {
                    var result = EditorGUIUtility.GetObjectPickerObject();
                    (Property.ChildResolver as ICollectionResolver).QueueAdd(new[] { result });
                    objectPicker = null;
                }
                CollectionDrawerStaticInfo.NextCustomAddFunction = null;
            }
        }

        private void OpenObjectPicker() {
            var runtimeSetInnerType = typeGetter.GetValue();
            if (runtimeSetInnerType == null) {
                Debug.Log("Runtime set inner type not specified");
                return;
            }
            var runtimeSetType = TypeCache.GetTypesDerivedFrom<RuntimeSetBase>()
                .Where(type => {
                    if (type.IsGenericType || !type.BaseType.IsGenericType) {
                        return false;
                    }
                    var listenerGenericArgs = type.BaseType.GenericTypeArguments;
                    if (listenerGenericArgs.Length == 0) {
                        return false;
                    }
                    return listenerGenericArgs[0] == runtimeSetInnerType;
                })
                .FirstOrDefault();
            if (runtimeSetType == null) {
                Debug.Log($"Runtime set for {runtimeSetInnerType.Name} not defined");
                RuntimeSetEditorWindow.Open()
                    .TryOpenEditorObjectCreationMenu(runtimeSetInnerType);
                return;
            }
            objectPicker = ObjectPicker.GetObjectPicker(Property.ValueEntry.WeakSmartValue, runtimeSetType);
            objectPicker.ShowObjectPicker(true);
        }
    }
}
