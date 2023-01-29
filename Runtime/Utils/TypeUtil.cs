using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Vaflov {
    public static partial class TypeUtil {
        public static bool IsInheritedFrom(Type type, Type targetType) {
            if (type == null) {
                return false;
            }

            if (type.IsGenericType) {
                if (type.GetGenericTypeDefinition() == targetType) {
                    return true;
                }
            } else if (type == targetType) {
                return true;
            }

            return IsInheritedFrom(type.BaseType, targetType);
        }

        public static FieldInfo GetFieldRecursive(Type type, string name, BindingFlags flags) {
            while (true) {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                    return field;

                Type baseType = type.BaseType;
                if (baseType == null)
                    return null;

                type = baseType;
            }
        }

        public static PropertyInfo GetPropertyRecursive(Type type, string name, BindingFlags flags) {
            while (true) {
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null)
                    return property;

                Type baseType = type.BaseType;
                if (baseType == null)
                    return null;

                type = baseType;
            }
        }

        public static List<Type> GetFlatTypesDerivedFrom(Type baseType, bool includeBaseType = true) {
            #if UNITY_EDITOR
            if (baseType == null)
                return null;
            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(type => !type.IsGenericType && !type.IsAbstract)
                .ToList();
            if (includeBaseType && !baseType.IsGenericType)
                types.Add(baseType);
            return types;
            #else
            return null;
            #endif
        }
    }
}
