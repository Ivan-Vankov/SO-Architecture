using System;

namespace Vaflov {
    public interface ITypeSelectionWindow  {
        public void CreateTypeSelectionWindow(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints);
    }
}