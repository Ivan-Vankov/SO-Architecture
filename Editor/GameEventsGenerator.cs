using System;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Vaflov.TypeUtil;
using static Vaflov.SingletonCodeGenerator;

namespace Vaflov {
    public class GameEventsGenerator {
        [MenuItem("Tools/SO Architecture/Generate Game Events")]
        public static void GenerateGameEvents() {
            new SingletonCodeGenerator(singletonClassName: "GameEvents", singletonConceptName: "GameEvent")
            .StartSingletonCodegenTimer()
            .AddSingletonHeader()
            .AddSingletonCustomCode(codegen => {
                (var namespaceName, var className        ) = (codegen.singletonNamespaceName, codegen.singletonClassName  );
                (var instanceName , var conceptName      ) = (codegen.singletonInstanceName , codegen.singletonConceptName);
                (var codeBuilder  , NameFilter nameFilter) = (codegen.singletonCodeBuilder  , codegen.SingletonNameFilter );

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract);

                var gameEventTypes = new[] {
                    typeof(GameEvent),
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

                            var eventFieldTypeName = gameEventSubType.Name;
                            var eventFieldName = nameFilter(gameEventAsset.name, true);

                            var resourcesPathName = "Resources";
                            var resourcesIdx = assetPath.IndexOf(resourcesPathName);
                            if (resourcesIdx == -1) {
                                Debug.LogError($"{conceptName} {eventFieldName} at path {assetPath} is not in the {resourcesPathName} folder");
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

    public class GameEventsUpdater : AssetPostprocessor {
        public static bool CheckUpdateGameEvents(string[] modifiedAssetPaths) {
            for (int i = 0; i < modifiedAssetPaths.Length; ++i) {
                if (modifiedAssetPaths[i].EndsWith("Event.asset")) {
                    GameEventsGenerator.GenerateGameEvents();
                    return true;
                }
            }
            return false;
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (CheckUpdateGameEvents(importedAssets)) { return; }
            if (CheckUpdateGameEvents(deletedAssets)) { return; }
            if (CheckUpdateGameEvents(movedAssets)) { return; }
            if (CheckUpdateGameEvents(movedFromAssetPaths)) { return; }
        }
    }
}