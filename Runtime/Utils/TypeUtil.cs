using System;
using System.Reflection;

namespace Vaflov {
    public static class TypeUtil {
        public static bool IsInheritedFrom(Type type, Type targetType) {
            var baseType = type.BaseType;
            if (baseType == null) {
                return false;
            }

            if (baseType.IsGenericType) {
                if (baseType.GetGenericTypeDefinition() == targetType) {
                    return true;
                }
            } else if (baseType == targetType) {
                return true;
            }

            return IsInheritedFrom(baseType, targetType);
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
    }
}
