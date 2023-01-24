using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;

namespace Vaflov {
    public class ExampleAchievementsEditorWindow : EditorObjectMenuEditorWindow {
        public override Type EditorObjBaseType => typeof(ExampleAchievement);

        public override IEditorObjectCreator CreateEditorObjectCreator() =>
            new DefaultEditorObjectCreator<ExampleAchievement>("Add a new achievement", "Achievements", "Achievement");

        [MenuItem("Tools/" + Config.PACKAGE_NAME + "/Demos/Achievements Editor")]
        public static ExampleAchievementsEditorWindow Open() => Open<ExampleAchievementsEditorWindow>("Achievements", "Example Trophy");
    }
}