using System;

namespace Vaflov {
    public static class TypeUtil {
        public static bool IsInheritedFrom(Type type, Type targetType) {
            var baseType = type.BaseType;
            if (baseType == null) {
                return false;
            }

            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == targetType) {
                return true;
            }

            return IsInheritedFrom(baseType, targetType);
        }
    }
}
