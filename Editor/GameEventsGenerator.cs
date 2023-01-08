using System;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Vaflov.TypeUtil;
using static Vaflov.FileUtil;
using static Vaflov.SingletonCodeGenerator;
using System.Collections.Generic;
using System.IO;

namespace Vaflov {
    public class GameEventsGenerator {

        public static readonly string GENERATED_GAME_EVENT_NAME_KEY = nameof(GENERATED_GAME_EVENT_NAME_KEY);
        public static readonly string GENERATED_GAME_EVENT_TYPE_KEY = nameof(GENERATED_GAME_EVENT_TYPE_KEY);

        public static SingletonCodeGenerator codegen = new SingletonCodeGenerator(singletonClassName: "GameEvents", singletonConceptName: "GameEvent")
            .SetSingletonFieldSuffix("Event");

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
                    argDataDecoded.Add(new GameEventArgData() {
                        argName = argDataPieces[0],
                        argType = Type.GetType(argDataPieces[1]),
                    });
                }
            }
            return argDataDecoded;
        }

        public static void GenerateGameEventAsset(string name, List<GameEventArgData> argData) {
            if (argData?.Count > CreateNewGameEvent.MAX_ARG_COUNT) {
                Debug.LogError($"Trying to create game event with {argData?.Count} arguments (max {CreateNewGameEvent.MAX_ARG_COUNT})");
                return;
            }
            var formattedName = codegen.SingletonNameFilter(name, false);
            var gameEventClassName = formattedName + codegen.singletonConceptName;
            Type gameEventType = null;
            if (argData.Count == 0) {
                gameEventType = typeof(GameEventVoid);
            } else {
                gameEventType = TypeCache.GetTypesDerivedFrom(typeof(GameEventBase))
                    .Where(type => type.Name == gameEventClassName)
                    .FirstOrDefault();
            }
            if (gameEventType == null) {
                EditorPrefs.SetString(GENERATED_GAME_EVENT_NAME_KEY, gameEventClassName);
                GenerateGameEventClass(formattedName, argData);
                return;
            }

            var gameEvent = ScriptableObject.CreateInstance(gameEventType);
            gameEvent.name = name;
            AssetDatabase.CreateAsset(gameEvent, $"Assets/Resources/Game Events/{name}.asset");
            AssetDatabase.SaveAssets();
        }

        public static void GenerateGameEventClass(string name, List<GameEventArgData> args) {
            var argsBuilder = new StringBuilder();
            string Args(bool showTypes = false, bool showNames = false) {
                if (!showTypes && !showNames)
                    return "";
                argsBuilder.Clear();
                for (int i = 0; i < args.Count; ++i) {
                    var arg = args[i];
                    if (showTypes) {
                        argsBuilder.Append(codegen.GetTruncatedTypeName(arg.argType));
                        if (showNames) {
                            argsBuilder.Append(" ");
                        }
                    }
                    if (showNames) {
                        argsBuilder.Append(arg.argName);
                    }
                    if (i < args.Count - 1) {
                        argsBuilder.Append(", ");
                    }
                }
                return argsBuilder.ToString();
            }

            var gameEventCodeBuilder = new StringBuilder();
            var actionName = name + "Action";
            var className = name + codegen.singletonConceptName;

            gameEventCodeBuilder
                .AppendLine(Config.AUTO_GENERATED_HEADER)
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
                .AppendLine("\t\t[Button(Expanded = true)]")
                .AppendLine("\t\t[PropertyOrder(15)]")
                .AppendLine("\t\t#endif")
                .AppendLine($"\t\tpublic override void Raise({Args(true, true)}) {{")
                .AppendLine($"\t\t\taction?.Invoke({Args(false, true)});")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine($"\t\tpublic override void AddListener(GameEventListener{args.Count}Base<{className}, {Args(true)}> listener) {{")
                .AppendLine("\t\t\tbase.AddListener(listener);")
                .AppendLine("\t\t\taction += listener.CallResponse;")
                .AppendLine("\t\t}")
                .AppendLine()
                .AppendLine($"\t\tpublic override void RemoveListener(GameEventListener{args.Count}Base<{className}, {Args(true)}> listener) {{")
                .AppendLine("\t\t\tbase.RemoveListener(listener);")
                .AppendLine("\t\t\taction -= listener.CallResponse;")
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}");

            var codeDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, Config.PACKAGE_NAME, "Generated", "Game Events"));
            if (!Directory.Exists(codeDirectory)) {
                Directory.CreateDirectory(codeDirectory);
            }

            var codePath = Path.Combine(codeDirectory, $"{className}.cs");
            TryCreateFileAsset(gameEventCodeBuilder.ToString(), codePath);
        }

        //[DidReloadScripts]
        //private static void TryGenerateConstantAssetDelayed() {
        //    if (!EditorPrefs.HasKey(GENERATED_GAME_EVENT_NAME_KEY)
        //     || !EditorPrefs.HasKey(GENERATED_GAME_EVENT_TYPE_KEY)) {
        //        //Debug.Log("early out");
        //        return;
        //    }
        //    if (EditorApplication.isCompiling || EditorApplication.isUpdating) {
        //        //Debug.Log("Delayed");
        //        UnityEditorEventUtility.DelayAction(TryGenerateConstantAssetDelayed);
        //        return;
        //    }
        //    //Debug.Log("Generating asset");
        //    var constantName = EditorPrefs.GetString(GENERATED_GAME_EVENT_NAME_KEY);
        //    var wrappedConstantType = Type.GetType(EditorPrefs.GetString(GENERATED_GAME_EVENT_TYPE_KEY));
        //    //Debug.Log($"{EditorPrefs.GetString(GENERATED_CONSTANT_TYPE_KEY)}, {wrappedConstantType}");

        //    EditorPrefs.DeleteKey(GENERATED_GAME_EVENT_NAME_KEY);
        //    EditorPrefs.DeleteKey(GENERATED_GAME_EVENT_TYPE_KEY);
        //    GenerateConstantAsset(constantName, wrappedConstantType);
        //}

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

                // TODO: Optimize this with var types = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
                // .Where(type => type.IsClass && !type.IsGenericType && !type.IsAbstract)
                // .ToList();

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract);

                var gameEventTypes = new[] {
                    typeof(GameEventVoid),
                    typeof(GameEvent1Arg<>),
                    typeof(GameEvent2Arg<,>)
                };

                var eventsCodeBuilder = new StringBuilder();
                foreach (var gameEventType in gameEventTypes) {
                    var gameEventSubtypes = types
                        .Where(type => IsInheritedFrom(type, gameEventType))
                        .ToList();
                    gameEventSubtypes.Add(gameEventType);
                    foreach (var gameEventSubType in gameEventSubtypes) {
                        var gameEventGuids = AssetDatabase.FindAssets($"t: {gameEventSubType}");
                        foreach (var gameEventGuid in gameEventGuids) {
                            var assetPath = AssetDatabase.GUIDToAssetPath(gameEventGuid);
                            var gameEventAsset = AssetDatabase.LoadAssetAtPath(assetPath, gameEventSubType);
                            if (!gameEventAsset) {
                                continue;
                            }

                            var eventFieldTypeName = gameEventSubType.Name;
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