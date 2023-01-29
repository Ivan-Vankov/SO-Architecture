#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

namespace Vaflov {
    public class ColorPropertyProcessor : ReloadingPropertyProcessor<ColorConstant>, IDisposable {
        public override ref Action ReloadAction => ref ColorConstant.OnColorConstantChanged;

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos) {
            var usePallette = ValueEntry.SmartValue.useColorPalette;
            if (!usePallette) { return; }
            var valueAttributes = propertyInfos.Find("value").GetEditableAttributesList();
            valueAttributes.Add(new ColorPaletteAttribute());
        }
    }
}
#endif