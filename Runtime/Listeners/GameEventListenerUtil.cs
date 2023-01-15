using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vaflov {
    public static class GameEventListenerUtil {
        public static GameEventListenerBase GetListenerInstance(GameEventBase eventRef) {
            #if UNITY_EDITOR
            if (eventRef == null) {
                return null;
            }
            if (eventRef is GameEventVoid) {
                return ScriptableObject.CreateInstance<GameEventListenerVoid>();
            }
            var gameEventType = eventRef.GetType();
            var genericArgs = gameEventType.BaseType.GenericTypeArguments;
            var listenerType = TypeCache.GetTypesDerivedFrom<GameEventListenerBase>()
                .Where(type => {
                    var eventRefField = type.GetField("eventRef", BindingFlags.Public | BindingFlags.Instance);
                    if (eventRefField?.FieldType == gameEventType) {
                        return true;
                    }
                    if (type.IsGenericType || !type.BaseType.IsGenericType) {
                        return false;
                    }
                    var listenerGenericArgs = type.BaseType.GenericTypeArguments;
                    if (listenerGenericArgs.Length != genericArgs.Length) {
                        return false;
                    }
                    for (int i = 0; i < listenerGenericArgs.Length; ++i) {
                        if (listenerGenericArgs[i] != genericArgs[i]) {
                            return false;
                        }
                    }
                    return true;
                })
                .FirstOrDefault();
            if (listenerType == null) {
                Debug.LogError($"No listener type for event {eventRef}");
                return null;
            }
            return (GameEventListenerBase)ScriptableObject.CreateInstance(listenerType);
            #else
            return null;
            #endif
        }
    }
}
