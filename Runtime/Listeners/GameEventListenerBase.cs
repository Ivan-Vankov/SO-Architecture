using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Vaflov {
    public class GameEventListenerBase : MonoBehaviour {
        [AssetsOnly]
        [OnValueChanged(nameof(AdaptGameEventListenerClassToGameEvent))]
        [Required]
        public GameEventBase gameEvent;

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
            DestroyImmediate(this);
        }

        public virtual void AssignGameEvent() {
            Debug.Assert(true, "Override this!");
        }
    }
}
