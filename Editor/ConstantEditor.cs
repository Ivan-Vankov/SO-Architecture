using System;
using UnityEditor;

namespace Vaflov {
    [CustomEditor(typeof(Constant<>), true)]
    public class ConstantEditor : ClassChangeEditor {
        public override Type ZeroGenericArgTargetType => throw new NotImplementedException();

        public override string BaseClassName => "Constant";

        public override string ClassDirectory => "Constants";

        public override string FoldoutLabel => "Change Constant Type";

        public override int ForcedTypeCount => 1;
    }
}
