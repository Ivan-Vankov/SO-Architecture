﻿#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public static class ContextMenuItemShortcutHandler {
        public static void HandleContextMenuItemShortcuts(IEnumerable<OdinContextMenuItem> contextMenuItems,
                                                          Action onShortcutPressed = null) {
            #if ODIN_INSPECTOR && UNITY_EDITOR
            // If a field is currently being edited then context menu shortcuts are skipped
            if (contextMenuItems == null || GUIUtility.keyboardControl != 0)
                return;
            var _event = Event.current;
            foreach (var contextMenuItem in contextMenuItems) {
                if (contextMenuItem.shortcut == KeyCode.None)
                    continue;
                if (_event.modifiers.HasFlag(contextMenuItem.modifiers)
                &&  _event.OnKeyDown(contextMenuItem.shortcut)) {
                    contextMenuItem.action?.Invoke();
                    onShortcutPressed?.Invoke();
                    break;
                }
            }
            #endif
        }
    }
}
