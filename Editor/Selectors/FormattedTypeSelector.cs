#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Vaflov {
    public class FormattedTypeSelector : OdinSelector<Type> {
        public static readonly HashSet<Type> builtinTypes = new HashSet<Type>() {
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
        };

        public const string builtinNamespace = "Built-in";
        public const string globalNamespace = "Global";

        private static Dictionary<AssemblyTypeFlags, List<OdinMenuItem>> cachedAllTypesMenuItems = new Dictionary<AssemblyTypeFlags, List<OdinMenuItem>>();

        private IEnumerable<Type> types;

        private AssemblyTypeFlags assemblyTypeFlags;

        private bool supportsMultiSelect;

        [HideInInspector]
        public bool HideNamespaces;

        [HideInInspector]
        public bool FlattenTree;

        private Type lastType;

        public override string Title => null;

        public FormattedTypeSelector(AssemblyTypeFlags assemblyFlags, bool supportsMultiSelect) {
            types = null;
            this.supportsMultiSelect = supportsMultiSelect;
            assemblyTypeFlags = assemblyFlags;
        }

        public FormattedTypeSelector(IEnumerable<Type> types, bool supportsMultiSelect) {
            this.types = (types != null) ? OrderTypes(types) : types;
            this.supportsMultiSelect = supportsMultiSelect;
        }

        private static string GetFromattedNamespace(Type type) {
            if (builtinTypes.Contains(type)) {
                return builtinNamespace;
            }
            return type.Namespace.IsNullOrWhitespace()
                ? globalNamespace
                : type.Namespace;
        }

        private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types) {
            return Enumerable.OrderBy(types, type => {
                var formattedNamespace = GetFromattedNamespace(type);
                return formattedNamespace == builtinNamespace ? "aaaaaaaa"
                     : formattedNamespace == globalNamespace  ? "aaaaaaab"
                     : formattedNamespace;
            }).ThenBy(type => type.Name);
            //return from x in types
            //       orderby GetFromattedNamespace(x) descending, x.Name
            //       select x;
        }

        public override bool IsValidSelection(IEnumerable<Type> collection) {
            return collection.Any();
        }

        protected override void BuildSelectionTree(OdinMenuTree tree) {
            tree.Config.UseCachedExpandedStates = false;
            tree.DefaultMenuStyle.NotSelectedIconAlpha = 1f;
            if (types == null) {
                if (cachedAllTypesMenuItems.TryGetValue(assemblyTypeFlags, out var value)) {
                    AddRecursive(tree, value, tree.MenuItems);
                } else {
                    IEnumerable<Type> enumerable = OrderTypes(AssemblyUtilities.GetTypes(assemblyTypeFlags).Where(x => {
                        if (x.Name == null) {
                            return false;
                        }

                        string text = x.Name.TrimStart(Array.Empty<char>());
                        return text.Length != 0 && char.IsLetter(text[0]);
                    }));
                    foreach (Type item in enumerable) {
                        string niceName = item.GetNiceName();
                        string typeNamePath = GetTypeNamePath(item, niceName);
                        //OdinMenuItem odinMenuItem = tree.AddObjectAtPath(typeNamePath, item).AddThumbnailIcons().Last();
                        OdinMenuItem odinMenuItem = tree.AddObjectAtPath(typeNamePath, item).Last();
                        odinMenuItem.SearchString = (niceName == typeNamePath) ? typeNamePath : (niceName + "|" + typeNamePath);
                    }

                    cachedAllTypesMenuItems[assemblyTypeFlags] = tree.MenuItems;
                }
            } else {
                foreach (Type t2 in types) {
                    string niceName2 = t2.GetNiceName();
                    string typeNamePath2 = GetTypeNamePath(t2, niceName2);
                    OdinMenuItem odinMenuItem2 = tree.AddObjectAtPath(typeNamePath2, t2).Last();
                    odinMenuItem2.SearchString = (niceName2 == typeNamePath2) ? typeNamePath2 : (niceName2 + "|" + typeNamePath2);
                    if (FlattenTree && GetFromattedNamespace(t2) != null) {
                        odinMenuItem2.OnDrawItem += x => {
                            GUI.Label(x.Rect.Padding(10f, 0f).AlignCenterY(16f), GetFromattedNamespace(t2), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                        };
                    }
                }

                tree.EnumerateTree(x => {
                    return x.Value is Type type 
                        && typeof(UnityEngine.Object).IsAssignableFrom(type);
                }, includeRootNode: false).AddThumbnailIcons();
            }

            tree.Selection.SupportsMultiSelect = supportsMultiSelect;
            tree.Selection.SelectionChanged += _ => {
                lastType = SelectionTree.Selection.Select(x => x.Value).OfType<Type>().LastOrDefault() ?? lastType;
            };
        }

        private string GetTypeNamePath(Type t, string niceName) {
            string text = niceName;
            if (!FlattenTree && !string.IsNullOrEmpty(GetFromattedNamespace(t)) && !HideNamespaces) {
                text = string.Concat(str1: (FlattenTree ? '.' : '/').ToString(), str0: GetFromattedNamespace(t), str2: text);
            }

            return text;
        }

        private static void AddRecursive(OdinMenuTree tree, List<OdinMenuItem> source, List<OdinMenuItem> destination) {
            destination.Capacity = source.Count;
            for (int i = 0; i < source.Count; i++) {
                OdinMenuItem odinMenuItem = source[i];
                OdinMenuItem odinMenuItem2 = new OdinMenuItem(tree, odinMenuItem.Name, odinMenuItem.Value).AddThumbnailIcon(preferAssetPreviewAsIcon: false);
                odinMenuItem2.SearchString = odinMenuItem.SearchString;
                destination.Add(odinMenuItem2);
                if (odinMenuItem.ChildMenuItems.Count > 0) {
                    AddRecursive(tree, odinMenuItem.ChildMenuItems, odinMenuItem2.ChildMenuItems);
                }
            }
        }

        protected override float DefaultWindowWidth() {
            return 450;
        }

        [OnInspectorGUI]
        [PropertyOrder(10f)]
        private void ShowTypeInfo() {
            string text = "";
            string text2 = "";
            string text3 = "";
            int num = 16;
            Rect rect = GUILayoutUtility.GetRect(0f, num * 3 + 8).Padding(10f, 4f).AlignTop(num);
            int num2 = 75;
            if (lastType != null) {
                text = lastType.GetNiceFullName();
                text2 = lastType.Assembly.GetName().Name;
                text3 = (lastType.BaseType == null) ? "" : lastType.BaseType.GetNiceFullName();
            }

            GUIStyle leftAlignedGreyMiniLabel = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
            GUI.Label(rect.AlignLeft(num2), "Type Name", leftAlignedGreyMiniLabel);
            GUI.Label(rect.AlignRight(rect.width - num2), text, leftAlignedGreyMiniLabel);
            rect.y += num;
            GUI.Label(rect.AlignLeft(num2), "Base Type", leftAlignedGreyMiniLabel);
            GUI.Label(rect.AlignRight(rect.width - num2), text3, leftAlignedGreyMiniLabel);
            rect.y += num;
            GUI.Label(rect.AlignLeft(num2), "Assembly", leftAlignedGreyMiniLabel);
            GUI.Label(rect.AlignRight(rect.width - num2), text2, leftAlignedGreyMiniLabel);
        }

        public override void SetSelection(Type selected) {
            base.SetSelection(selected);
            SelectionTree.Selection
                .SelectMany(x => x.GetParentMenuItemsRecursive(includeSelf: false))
                .ForEach(x => x.Toggled = true);
        }
    }
}
#endif