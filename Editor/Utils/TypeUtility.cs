//using JetBrains.Annotations;
//using SolidUtilities;
//using System;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//namespace Vaflov {
//    public static class TypeUtility {
//        public static Type GetEmptyTypeDerivedDirectlyFrom(Type parentType) {
//#if UNITY_EDITOR
//            return TypeCache.GetTypesDerivedFrom(parentType)
//                .FirstOrDefault(type => type.IsEmpty() && type.BaseType == parentType);
//#else
//            return parentType.Assembly.GetTypes()
//                .Where(parentType.IsAssignableFrom)
//                .FirstOrDefault(type => type.IsEmpty() && type.BaseType == parentType);
//#endif
//        }

//        [CanBeNull]
//        public static Type GetEmptyTypeDerivedFrom(Type parentType) {
//#if UNITY_EDITOR
//            return TypeCache.GetTypesDerivedFrom(parentType)
//                .FirstOrDefault(type => type.IsEmpty());
//#else
//            return parentType.Assembly.GetTypes()
//                .Where(parentType.IsAssignableFrom)
//                .FirstOrDefault(type => type.IsEmpty());
//#endif
//        }

//        public static string GetTypeNameAndAssembly(Type type) {
//            if (type == null)
//                return string.Empty;

//            if (type.FullName == null)
//                throw new ArgumentException($"'{type}' does not have full name.", nameof(type));

//            return GetTypeNameAndAssembly(type.FullName, type.Assembly.GetName().Name);
//        }

//        public static string GetTypeNameAndAssembly(string typeFullName, string assemblyName) =>
//            $"{typeFullName}, {assemblyName}";

//        public static string GetNiceNameOfGenericType(Type genericType, bool fullName = false) {
//            return TypeHelper.GetNiceNameOfGenericType(genericType, fullName).Replace('.', '/');
//        }
//    }
//}