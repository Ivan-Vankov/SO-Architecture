#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Mathf;
using UnityEditor;
using UnityEngine;
using static Vaflov.EditorStringUtil;

namespace Vaflov {
    public class GameEventsEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(GameEventBase);

        [MenuItem("Tools/SO Architecture/Game Events Editor")]
        public static GameEventsEditorWindow Open() {
            return Open<GameEventsEditorWindow>("Game Events", "Game Events");
        }

        public override IEditorObjectCreator CreateEditorObjectCreator() => new CreateNewGameEvent();

        public override List<OdinContextMenuItem> GetToolbarItems() {
            var items = new List<OdinContextMenuItem>();
            items.AddRange(base.GetToolbarItems());
            items.Add(new OdinContextMenuItem("Regenerate game events", () => {
                GameEventsGenerator.GenerateGameEvents();
                //ForceMenuTreeRebuild();
            }, KeyCode.S, EventModifiers.Control, SdfIconType.ArrowRepeat));
            return items;
        }
    }

    public class GameEventArgData {
        public string argName;
        public Type argType;

        public GameEventArgData(string argName, Type argType) {
            this.argName = argName;
            this.argType = argType;
        }

        public GameEventArgData(GameEventArgDrawData drawData) {
            argName = drawData.argName;
            argType = drawData.typeDropdownFieldDrawer.targetType;
        }
    }

    public class GameEventArgDrawData {
        public string argName;
        public TypeDropdownFieldDrawer typeDropdownFieldDrawer;
    }

    public class GameEventCreationData {
        public const int MAX_ARG_COUNT = 6;

        [HideInInspector]
        public readonly List<GameEventArgDrawData> argData = new List<GameEventArgDrawData>(MAX_ARG_COUNT) {
            new GameEventArgDrawData(),
            new GameEventArgDrawData(),
            new GameEventArgDrawData(),
            new GameEventArgDrawData(),
            new GameEventArgDrawData(),
            new GameEventArgDrawData(),
        };

        [HideInInspector]
        public int argCount;

        public const string DEFAULT_GAME_EVENT_NAME = "New Game Event";

        [HideInInspector]
        public string name = DEFAULT_GAME_EVENT_NAME;

        [HideInInspector]
        public string nameError;

        [HideInInspector]
        public List<Type> types;

        public void Reset() {
            types = EditorTypeUtil.GatherPublicTypes();
            for (int i = 0; i < argData.Count; i++) {
                var arg = argData[i];
                arg.argName = $"Arg{i}";
                arg.typeDropdownFieldDrawer = new TypeDropdownFieldDrawer(types);
            }
        }
    }

    public class CreateNewGameEvent : IEditorObjectCreator {
        public GameEventCreationData creationData = new GameEventCreationData();

        [HideInInspector]
        public List<string> assetNames;

        public const int labelWidth = 40;

        public string Description => "Add a new game event";

        public void ResetName() {
            creationData.name = GameEventCreationData.DEFAULT_GAME_EVENT_NAME;
        }

        public void Reset() {
            creationData.Reset();
            assetNames = AssetUtil.GetAssetPathsForType(typeof(GameEventBase));
        }

        [OnInspectorGUI]
        private void OnInspectorGUI() {
            var error = false;
            void ErrorMessageBox(string errorMessage) {
                if (string.IsNullOrEmpty(errorMessage))
                    return;
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                error = true;
            }
            ErrorMessageBox(creationData.nameError);
            GUIHelper.PushLabelWidth(labelWidth);
            var oldName = creationData.name;
            //name = SirenixEditorFields.DelayedTextField(GUIHelper.TempContent("Name"), name);
            creationData.name = SirenixEditorFields.TextField(GUIHelper.TempContent("Name"), creationData.name);
            if (creationData.name != oldName) {
                creationData.nameError = EditorStringUtil.ValidateAssetName(creationData.name, assetNames);
            }
            GUIHelper.PopLabelWidth();
            GUIHelper.PushLabelWidth(70);
            creationData.argCount = EditorGUILayout.IntSlider("Arg Count", creationData.argCount, 0, GameEventCreationData.MAX_ARG_COUNT);
            GUIHelper.PopLabelWidth();
            for (int i = 0; i < creationData.argCount; ++i) {
                var arg = creationData.argData[i];
                SirenixEditorGUI.BeginBox(null);
                GUIHelper.PushLabelWidth(labelWidth);

                ErrorMessageBox(ValidateArgName(arg.argName));

                arg.argName = SirenixEditorFields.TextField(GUIHelper.TempContent($"Arg {i}"), arg.argName);
                var targetType = arg.typeDropdownFieldDrawer.TypeField();
                var targetTypeError = targetType == null ? "Type is empty" : null;
                if (!string.IsNullOrEmpty(targetTypeError)) {
                    ErrorMessageBox(targetTypeError);
                }

                GUIHelper.PopLabelWidth();
                SirenixEditorGUI.EndBox();
            }

            if (error) {
                using (new EditorGUI.DisabledScope(true)) {
                    GUILayout.Button(new GUIContent("Create Asset", "Fix all errors first"));
                }
            } else if (GUILayout.Button("Create Asset")) {
                var passedArgData = new List<GameEventArgData>(creationData.argCount);
                for (int i = 0; i < creationData.argCount; ++i) {
                    passedArgData.Add(new GameEventArgData(creationData.argData[i]));
                }
                GameEventsGenerator.GenerateGameEventAsset(creationData.name, passedArgData);
            }
        }
    }
}
#endif