namespace Vaflov {
    public class DecimalConstant : Constant<decimal> {
        public override string EditorToString() {
            return Value.ToString("0.0000");
        }
    }
}
