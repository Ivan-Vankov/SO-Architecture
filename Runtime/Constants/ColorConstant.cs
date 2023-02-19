#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using UnityEngine;
using static Vaflov.SOArchitectureConfig;

namespace Vaflov {
    public class ColorConstant : Constant<Color> {
        public static Action OnColorConstantChanged;

        public const int colorTexSize = 15;
        public const int transparentGridSize = 3;
        public static readonly Color[] gridColors = new Color[] {
            new Color(.8f, .8f, .8f, 1),
            new Color(.5f, .5f, .5f, 1),
        };
        public static Texture2D colorTex;

        #if ODIN_INSPECTOR
        [LabelText("Use palette")]
        [LabelWidth(preferedEditorLabelWidth)]
        [PropertyOrder(13)]
        [OnValueChanged(nameof(OnColorChanged))]
        #endif
        public bool useColorPalette = true;

        public override Texture GetEditorIcon() {
            colorTex = colorTex == null ? new Texture2D(colorTexSize, colorTexSize) : colorTex;
            var height = colorTex.height - 1;
            var width = colorTex.width - 1;
            Color color;
            Color gridColor;
            for (int y = 1; y < height; ++y) {
                for (int x = 1; x < width; ++x) {
                    color = Value;
                    if (colorTexSize - x - y < 0) {
                        gridColor = gridColors[(x / transparentGridSize + y / transparentGridSize) % 2];
                        color = Color.Lerp(gridColor, color, color.a);
                    } else {
                        color.a = 1;
                    }
                    colorTex.SetPixel(x, y, color);
                }
            }
            for (int i = 0; i <= height; ++i) {
                colorTex.SetPixel(i, 0, Color.black);
                colorTex.SetPixel(i, width, Color.black);
            }
            for (int i = 1; i < width; ++i) {
                colorTex.SetPixel(0, i, Color.black);
                colorTex.SetPixel(height, i, Color.black);
            }
            colorTex.Apply();
            return colorTex;
        }

        public void OnColorChanged() {
            OnColorConstantChanged?.Invoke();
        }

        public override string EditorToString() {
            return "#" + ColorUtility.ToHtmlStringRGBA(Value);
        }
    }
}
