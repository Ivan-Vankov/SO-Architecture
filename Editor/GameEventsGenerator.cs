#if ODIN_INSPECTOR && UNITY_EDITOR
using System;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Vaflov.FileUtil;
using static Vaflov.SingletonCodeGenerator;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using Sirenix.OdinInspector.Editor;

namespace Vaflov {
    public class GameEventsGenerator {

        public static readonly string GENERATED_GAME_EVENT_NAME_KEY = nameof(GENERATED_GAME_EVENT_NAME_KEY);
        public static readonly string GENERATED_GAME_EVENT_ARG_DATA_KEY = nameof(GENERATED_GAME_EVENT_ARG_DATA_KEY);

        public static SingletonCodeGenerator codegen = new SingletonCodeGenerator(singletonClassName: "GameEvents", singletonConceptName: "GameEvent")
            .SetSingletonFieldSuffix("Event");

        public static StringBuilder argsBuilder = new StringBuilder();

        public static string FormatArgs(List<GameEventArgData> args,
                                        bool showTypes = false,
                                        bool showNames = false,
                                        bool quoteNames = false,
                                        bool nicifyNames = false) {
            if (!showTypes && !showNames)
                return "";
            argsBuilder.Clear();
            for (int i = 0; i < args.Count; ++i) {
                var arg = args[i];
                var argTypeName = showTypes ? codegen.GetTruncatedTypeName(arg.argType) : "";

                var argName = showNames ? arg.argName : "";
                argName = nicifyNames ? ObjectNames.NicifyVariableName(argName) : argName;
                argName = quoteNames ? $"\"{argName}\"" : argName;
                if ((showTypes || i > 0) && argName.Length > 0) {
                    argName = " " + argName;
                }

                argsBuilder
                    .Append(argTypeName)
                    .Append(argName)
                    .Append(i < args.Count - 1 ? ", ": "");
            }
            return argsBuilder.ToString();
        }

        public static string EncodeArgData(List<GameEventArgData> argData) {
            var argDataStringBuilder = new StringBuilder();
            for (int i = 0; i < argData.Count; ++i) {
                var arg = argData[i];
                argDataStringBuilder
                    .Append(arg.argName)
                    .Append("|")
                    .Append(arg.argType.AssemblyQualifiedName);
                if (i < argData.Count - 1) {
                    argDataStringBuilder.Append("\\");
                }
            }
            return argDataStringBuilder.ToString();
        }

        public static List<GameEventArgData> DecodeArgData(string argDataString) {
            List<GameEventArgData> argDataDecoded = new List<GameEventArgData>();
            var argDataSlices = argDataString.Split('\\');
            foreach (var argDataSlice in argDataSlices) {
                var argDataPieces = argDataSlice.Split('|');
                if (argDataPieces.Length >= 2) {
                    argDataDecoded.Add(new GameEventArgData(argDataPieces[0], Type.GetType(argDataPieces[1])));
                }
            }
            return argDataDecoded;
        }

        public static void GenerateGameEventAsset(string name, List<GameEventArgData> argData, bool generateClass = true) {
            if (argData == null) {
                Debug.LogError("Trying to create game event with no arg data");
                return;
            }
            if (argData.Count > GameEventCreationData.MAX_ARG_COUNT) {
                Debug.LogError($"Trying to create game event with {argData?.Count} arguments (max {GameEventCreationData.MAX_ARG_COUNT})");
                return;
            }
            var formattedName = codegen.SingletonNameFilter(name, false);
            var gameEventClassName = formattedName + codegen.singletonConceptName;
            Type gameEventType = null;
            if (argData.Count == 0) {
                gameEventType = typeof(GameEventVoid);
            } else {
                gameEventType = TypeCache.GetTypesDerivedFrom(typeof(GameEventBase))
                    .Where(type => {
                        if (type.Name != gameEventClassName)
                            return false;
                        var genericTypeArgs = type.BaseType.GenericTypeArguments;
                        if (genericTypeArgs.Length - 1 != argData.Count)
                            return false;
                        for (int i = 1; i < genericTypeArgs.Length; ++i) {
                            if (genericTypeArgs[i] != argData[i - 1].argType)
                                return false;
                        }
                        return true;
                    })
                    .FirstOrDefault();
            }
            if (gameEventType == null) {
                if (generateClass) {
                    EditorPrefs.SetString(GENERATED_GAME_EVENT_NAME_KEY, name);
                    EditorPrefs.SetString(GENERATED_GAME_EVENT_ARG_DATA_KEY, EncodeArgData(argData));
                    GenerateGameEventClass(formattedName, argData);
                    GenerateGameEventListenerClass(formattedName, argData);
                    AssetDatabase.SaveAssets();
                }
                return;
            }

            AssetDatabaseUtil.SaveScriptableObject(gameEventType, "Assets/Resources/Game Events", name);
        }

        public static void GenerateGameEventClass(string name, List<GameEventArgData> args) {
            string Args(bool showTypes = false, bool showNames = false) => 
                FormatArgs(args, showTypes, showNames);

            var gameEventCodeBuilder = new StringBuilder();
            var actionName = name + "Action";
            var className = name + codegen.singletonConceptName;

            gameEventCodeBuilder
                .AppendLine(SOArchitectureConfig.AUTO_GENERATED_HEADER)
                .AppendLine("#if ODIN_INSPECTOR")
                .AppendLine("using Sirenix.OdinInspector;")
                .AppendLine("#endif")
                .AppendLine()
                .AppendLine($"namespace {codegen.singletonNamespaceName} {{")
                .AppendLine($"\tpublic delegate void {actionName}({Args(true, true)});")
                .AppendLine()
                .AppendLine($"\tpublic class {className} : GameEvent{args.Count}Base<{className}, {Args(true)}> {{")
                .AppendLine($"\t\tpublic event {actionName} action;")
                .AppendLine()
                .AppendLine("\t\t#if ODIN_INSPECTOR")
                .AppendLine("\t\t[Button]")
                .AppendLine("\t\t[DrawButtonTypes]")
                .AppendLine("\t\t[PropertyOrder(15)]")
                .AppendLine("\t\t#endif")
                .AppendLine($"\t\tpublic override void Raise({Args(true, true)}) {{")
                .AppendLine($"\t\t\taction?.Invoke({Args(false, true)});")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine("\t\tpublic override string EditorToString() {")
                .AppendLine($"\t\t\treturn \"({Args(false, true)})\";")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine($"\t\tpublic override void AddListener(GameEventListener<{className}, {Args(true)}> listener) {{")
                .AppendLine("\t\t\t#if UNITY_EDITOR")
                .AppendLine("\t\t\tbase.AddListener(listener);")
                .AppendLine("\t\t\t#endif")
                .AppendLine("\t\t\taction -= listener.CallResponse;")
                .AppendLine("\t\t\taction += listener.CallResponse;")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine($"\t\tpublic override void RemoveListener(GameEventListener<{className}, {Args(true)}> listener) {{")
                .AppendLine("\t\t\t#if UNITY_EDITOR")
                .AppendLine("\t\t\tbase.RemoveListener(listener);")
                .AppendLine("\t\t\t#endif")
                .AppendLine("\t\t\taction -= listener.CallResponse;")
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}");

            TryCreateFileAsset(gameEventCodeBuilder.ToString(), $"{className}.cs",
                ImportAssetOptions.Default,
                Resources.Load<Texture2D>("Game Event"),
                Application.dataPath, SOArchitectureConfig.PACKAGE_NAME, "Generated", "Game Events");
        }

        public static void GenerateGameEventListenerClass(string name, List<GameEventArgData> args) {
            string Args(bool showTypes = false, bool showNames = false, bool quoteNames = false, bool nicifyNames = false) => 
                FormatArgs(args, showTypes, showNames, quoteNames, nicifyNames);

            var gameEventCodeBuilder = new StringBuilder();
            var gameEventClassName = name + codegen.singletonConceptName;
            var gameEventListenerClassName = gameEventClassName + "Listener";

            gameEventCodeBuilder
                .AppendLine(SOArchitectureConfig.AUTO_GENERATED_HEADER)
                .AppendLine("using ExtEvents;")
                .AppendLine()
                .AppendLine($"namespace {codegen.singletonNamespaceName} {{")
                .AppendLine($"\tpublic class {gameEventListenerClassName} : GameEventListener<{gameEventClassName}, {Args(true)}> {{")
                .AppendLine($"\t\t[EventArguments({Args(false, true, true, true)})]")
                .AppendLine($"\t\tpublic ExtEvent<{Args(true)}> response;")
                .AppendLine($"\t\tpublic override ExtEvent<{Args(true)}> Response => response;")
                .AppendLine("\t}")
                .AppendLine("}");

            TryCreateFileAsset(gameEventCodeBuilder.ToString(), $"{gameEventListenerClassName}.cs",
                ImportAssetOptions.Default,
                Resources.Load<Texture2D>("Listener"),
                Application.dataPath, SOArchitectureConfig.PACKAGE_NAME, "Generated", "Game Event Listeners");
        }

        [DidReloadScripts]
        private static void TryGenerateConstantAssetDelayed() {
            if (!EditorPrefs.HasKey(GENERATED_GAME_EVENT_NAME_KEY)
             || !EditorPrefs.HasKey(GENERATED_GAME_EVENT_ARG_DATA_KEY)) {
                //Debug.Log("early out");
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
                //Debug.Log("Delayed");
                UnityEditorEventUtility.DelayAction(TryGenerateConstantAssetDelayed);
                return;
            }
            //Debug.Log("Generating asset");
            var gameEventName = EditorPrefs.GetString(GENERATED_GAME_EVENT_NAME_KEY);
            var argData = DecodeArgData(EditorPrefs.GetString(GENERATED_GAME_EVENT_ARG_DATA_KEY));

            EditorPrefs.DeleteKey(GENERATED_GAME_EVENT_NAME_KEY);
            EditorPrefs.DeleteKey(GENERATED_GAME_EVENT_ARG_DATA_KEY);
            GenerateGameEventAsset(gameEventName, argData, false);
        }

        //[MenuItem("Tools/SO Architecture/Generate Game Events")]
        public static void GenerateGameEvents() {
            codegen
            .Clear()
            .StartSingletonCodegenTimer()
            .AddSingletonHeader()
            .AddSingletonCustomCode(codegen => {
                (var namespaceName, var className        ) = (codegen.singletonNamespaceName, codegen.singletonClassName  );
                (var instanceName , var conceptName      ) = (codegen.singletonInstanceName , codegen.singletonConceptName);
                (var codeBuilder  , NameFilter nameFilter) = (codegen.singletonCodeBuilder  , codegen.SingletonNameFilter );
                var gameEventTypes = TypeUtil.GetFlatTypesDerivedFrom(typeof(GameEventBase), false);

                var eventsCodeBuilder = new StringBuilder();
                foreach (var gameEventType in gameEventTypes) {
                    var gameEventGuids = AssetDatabase.FindAssets($"t: {gameEventType}");
                    foreach (var gameEventGuid in gameEventGuids) {
                        var assetPath = AssetDatabase.GUIDToAssetPath(gameEventGuid);
                        var gameEventAsset = AssetDatabase.LoadAssetAtPath(assetPath, gameEventType);
                        if (!gameEventAsset) {
                            continue;
                        }

                        var eventFieldTypeName = gameEventType.Name;
                        var eventFieldName = "_" + nameFilter(gameEventAsset.name, true);

                        var resourcesPathName = "Resources";
                        var resourcesIdx = assetPath.IndexOf(resourcesPathName);
                        if (resourcesIdx == -1) {
                            Debug.LogError($"{conceptName} {eventFieldName} at path {assetPath} is not in a {resourcesPathName} folder or subfolder");
                            continue;
                        }
                        var resourcesAssetPath = assetPath.Substring(resourcesIdx + resourcesPathName.Length + 1);
                        resourcesAssetPath = resourcesAssetPath.Substring(0, resourcesAssetPath.LastIndexOf(".asset"));
                        codeBuilder
                            .AppendLine($"\t\t\t{eventFieldName} = {resourcesPathName}.Load<{eventFieldTypeName}>(\"{resourcesAssetPath}\");");

                        eventsCodeBuilder
                            .AppendLine($"\t\tpublic {eventFieldTypeName} {eventFieldName};")
                            .AppendLine($"\t\tpublic static {eventFieldTypeName} {nameFilter(gameEventAsset.name, false)} => {instanceName}.{eventFieldName};")
                            .AppendLine();
                    }
                }

                codeBuilder
                    .AppendLine("\t\t}")
                    .AppendLine()
                    .Append(eventsCodeBuilder.ToString());
            })
            .AddSingletonFooter()
            .GenerateSingletonAssets()
            .EndSingletonCodegenTimerAndPrint();
        }
    }

    public class GameEventClassRemover : AssetModificationProcessor {
        public static HashSet<Type> deletedGameEventTypes = new HashSet<Type>();

        public static void DeleteGameEventClassesDelayed() {
            foreach (var gameEventType in deletedGameEventTypes) {
                //Debug.Log(gameEventType.Name);
                var assets = AssetDatabase.FindAssets($"t: {gameEventType}");
                if (assets?.Length > 0)
                    continue;

                var gameEventClassName = gameEventType.Name;
                var gameEventListenerClassName = gameEventClassName + "Listener";
                AssetDatabase.DeleteAsset($"Assets/{SOArchitectureConfig.PACKAGE_NAME}/Generated/Game Event Listeners/{gameEventListenerClassName}.cs");
                AssetDatabase.DeleteAsset($"Assets/{SOArchitectureConfig.PACKAGE_NAME}/Generated/Game Events/{gameEventClassName}.cs");
            }
            deletedGameEventTypes.Clear();
        }

        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _) {
            var gameEventAsset = AssetDatabase.LoadAssetAtPath<GameEventBase>(path);
            if (gameEventAsset) {
                var gameEventType = gameEventAsset.GetType();
                if (gameEventType == typeof(GameEventVoid))
                    return AssetDeleteResult.DidNotDelete;
                deletedGameEventTypes.Add(gameEventType);
                if (deletedGameEventTypes.Count == 1) {
                    UnityEditorEventUtility.DelayAction(DeleteGameEventClassesDelayed);
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }

    //public class GameEventsUpdater : AssetPostprocessor {
    //    public static bool CheckUpdateGameEvents(string[] modifiedAssetPaths) {
    //        for (int i = 0; i < modifiedAssetPaths.Length; ++i) {
    //            if (modifiedAssetPaths[i].EndsWith("Event.asset")) {
    //                GameEventsGenerator.GenerateGameEvents();
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
    //        if (CheckUpdateGameEvents(importedAssets)) { return; }
    //        if (CheckUpdateGameEvents(deletedAssets)) { return; }
    //        if (CheckUpdateGameEvents(movedAssets)) { return; }
    //        if (CheckUpdateGameEvents(movedFromAssetPaths)) { return; }
    //    }
    //}
}
#endif