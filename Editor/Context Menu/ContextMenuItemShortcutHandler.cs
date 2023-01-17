using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vaflov {
    public static class ContextMenuItemShortcutHandler {
        public static void HandleContextMenuItemShortcuts(IEnumerable<OdinContextMenuItem> contextMenuItems,
                                                          Action onShortcutPressed = null) {
            if (contextMenuItems == null)
                return;
            var _event = Event.current;
            foreach (var contextMenuItem in contextMenuItems) {
                if (_event.modifiers.HasFlag(contextMenuItem.modifiers)
                &&  _event.OnKeyDown(contextMenuItem.shortcut)) {
                    contextMenuItem.action?.Invoke();
                    onShortcutPressed?.Invoke();
                    break;
                }
            }
        }
    }
}
