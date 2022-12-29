namespace Vaflov {
    public class DoubleConstant : ClampedConstant<double> {
        public override string EditorToString() {
            return Value.ToString("0.0000");
        }
    }
}
