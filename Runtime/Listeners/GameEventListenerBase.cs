using Sirenix.OdinInspector;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    [DefaultExecutionOrder(-2000)]
    public abstract class GameEventListenerBase : MonoBehaviour {
        #if UNITY_EDITOR && ODIN_INSPECTOR

        //[HorizontalGroup("Event Ref", MaxWidth = 15)]
        //[ValueDropdown(nameof(valueList), DropdownWidth = 100)]
        //[HideLabel]
        //public bool useValue = true;

        [HorizontalGroup("Event Ref")]
        [AssetsOnly]
        [OnValueChanged(nameof(AdaptGameEventListenerClassToGameEvent))]
        [Required]
        [LabelText("Event Ref")]
        public GameEventBase gameEvent;

        //private ValueDropdownList<bool> valueList = new ValueDropdownList<bool>() {
        //    { "Value", true },
        //    { "Reference", false },
        //};

        public void AdaptGameEventListenerClassToGameEvent() {
            Type listenerType;
            if (gameEvent == null) {
                AssignGameEvent();
                return;
            }
            var gameEventType = gameEvent.GetType();
            if (gameEventType == typeof(GameEventVoid)) {
                listenerType = typeof(GameEventListenerVoid);
            } else {
                var gameEventGenericArgs = gameEventType.BaseType.GenericTypeArguments;
                listenerType = TypeCache.GetTypesDerivedFrom<GameEventListenerBase>()
                    .Where(type => {
                        var eventRefField = type.GetField("eventRef", BindingFlags.Public | BindingFlags.Instance);
                        if (eventRefField?.FieldType == gameEventType) {
                            return true;
                        }
                        if (type.IsGenericType || !type.BaseType.IsGenericType) {
                            return false;
                        }
                        var listenerGenericArgs = type.BaseType.GenericTypeArguments;
                        if (listenerGenericArgs.Length != gameEventGenericArgs.Length) {
                            return false;
                        }
                        for (int i = 0; i < listenerGenericArgs.Length; ++i) {
                            if (listenerGenericArgs[i] != gameEventGenericArgs[i]) {
                                return false;
                            }
                        }
                        return true;
                    })
                    .FirstOrDefault();
            }
            if (listenerType == null) {
                return;
            }
            if (listenerType == GetType()) {
                AssignGameEvent();
                return;
            }
            var newListener = gameObject.AddComponent(listenerType) as GameEventListenerBase;
            newListener.gameEvent = gameEvent;
            newListener.AssignGameEvent();
            this.ReplaceComponentWith(newListener);
        }

        public virtual void AssignGameEvent() {
            Debug.Assert(true, "Override this!");
        }
        #endif
    }
}
