namespace Vaflov {
    public class FloatConstant : ClampedConstant<float> {
        public override string EditorToString() {
            return Value.ToString("0.0000");
        }
    }
}
