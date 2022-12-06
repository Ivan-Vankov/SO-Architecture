using System;
using System.Linq;
//using SelectionWindow;

namespace Vaflov {
    public static class TypeSelectionWindow {
        public static void CreateSelectionWindow(Type genericTypeWithoutArgs, Action<Type[]> onTypesSelected) {
            var genericParamConstraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            //var genericArgNames = TypeHelper.GetNiceArgsOfGenericType(genericTypeWithoutArgs);

            //ITypeSelectionWindow window;
            //Action<Action<Type[]>, string[], Type[][]> func;

            //if (genericParamConstraints.Length == 1) {
            //    window = new OneTypeSelectionWindow();
            //} else {
            //    window = ScriptableObject.CreateInstance<MultipleTypeSelectionWindow>();
            //}

            //ScriptableObject.CreateInstance<MultipleTypeSelectionWindow>()
            //    .CreateTypeSelectionWindow(onTypesSelected, genericArgNames, genericParamConstraints);
        }
    }
}