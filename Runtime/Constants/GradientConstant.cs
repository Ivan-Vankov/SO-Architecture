using UnityEngine;

namespace Vaflov {
    public class GradientConstant : Constant<Gradient> {
        public const int gradientTexSize = 15;
        public const int transparentGridSize = 3;
        public static readonly Color[] gridColors = new Color[] {
            new Color(.8f, .8f, .8f, 1),
            new Color(.5f, .5f, .5f, 1),
        };
        public static readonly Color[] gradientColors = new Color[gradientTexSize];
        public static Texture2D gradientTex;

        public override Texture GetEditorIcon() {
            gradientTex = gradientTex == null ? new Texture2D(gradientTexSize, gradientTexSize) : gradientTex;
            for (int i = 0; i < gradientTexSize; ++i) {
                gradientColors[i] = Value.Evaluate((float)i / (gradientTexSize - 1));
            }
            Color color;
            Color gridColor;
            for (int y = 1; y < gradientTex.height - 1; ++y) {
                for (int x = 0; x < gradientTex.width; ++x) {
                    color = gradientColors[x];
                    if (color.a < 1) {
                        gridColor = gridColors[(x / transparentGridSize + y / transparentGridSize) % 2];
                        color = Color.Lerp(gridColor, color, color.a);
                    }
                    gradientTex.SetPixel(x, y, color);
                }
            }
            for (int x = 0; x < gradientTex.width; ++x) {
                gradientTex.SetPixel(x, 0, Color.black);
                gradientTex.SetPixel(x, gradientTex.height - 1, Color.black);
            }
            gradientTex.Apply();
            return gradientTex;
        }
    }
}
