using UnityEditor;
using Vaflov;
using System;
using UnityEngine;

[CustomEditor(typeof(ClampedConstant<>), true)]
[CanEditMultipleObjects]
public class ClampedConstantEditor : ConstantEditor {
    SerializedProperty valueProperty;
    SerializedProperty isClampedProperty;
    SerializedProperty minProperty;
    SerializedProperty maxProperty;

    public new void OnEnable() {
        valueProperty = serializedObject.FindProperty("value");
        isClampedProperty = serializedObject.FindProperty("isClamped");
        minProperty = serializedObject.FindProperty("min");
        maxProperty = serializedObject.FindProperty("max");
        base.OnEnable();
    }

    public enum NumericType {
        INT,
        FLOAT,
        NONE,
    }

    public static (NumericType numericType, IComparable value) GetNumericTypeData(object obj) {
        switch (obj) {
            case sbyte   num: return (NumericType.INT,          (int)num);
            case byte    num: return (NumericType.INT,          (int)num);
            case short   num: return (NumericType.INT,          (int)num);
            case ushort  num: return (NumericType.INT,          (int)num);
            case int     num: return (NumericType.INT,               num);
            case uint    num: return (NumericType.INT,          (int)num);
            case long    num: return (NumericType.INT,          (int)num);
            case ulong   num: return (NumericType.INT,          (int)num);
            case float   num: return (NumericType.FLOAT,             num);
            case double  num: return (NumericType.FLOAT,      (float)num);
            case decimal num: return (NumericType.FLOAT,      (float)num);
            default:          return (NumericType.NONE, (IComparable)obj);
        }
    }

    public static object BoxIntToSpecificType(int value, object originalValue) {
        switch (originalValue) {
            case sbyte   _: return (sbyte)value;
            case byte    _: return (byte)value;
            case short   _: return (short)value;
            case ushort  _: return (ushort)value;
            case uint    _: return (uint)value;
            case long    _: return (long)value;
            case ulong   _: return (ulong)value;
            default: return value;
        }
    }

    public static object BoxFloatToSpecificType(float value, object originalValue) {
        switch (originalValue) {
            case double _: return (double)value;
            case decimal _: return (decimal)value;
            default: return value;
        }
    }

    public override void OnInspectorGUI() {
        DrawClampedConstant();
        DrawChangeClassFoldoutSafe();
    }

    public void DrawClampedConstant() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(isClampedProperty);
        var isClamped = isClampedProperty.boolValue;
        if (isClamped) {
            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.PropertyField(minProperty);
                EditorGUILayout.PropertyField(maxProperty);
            }
            (var _, var min) = GetNumericTypeData(minProperty.GetRawValue());
            (var _, var max) = GetNumericTypeData(maxProperty.GetRawValue());

            var originalValue = valueProperty.GetRawValue();
            (var numericType, var value) = GetNumericTypeData(originalValue);
            switch (numericType) {
                case NumericType.INT:
                    var intValue = EditorGUILayout.IntSlider((int)value, (int)min, (int)max);
                    valueProperty.SetRawValue(BoxIntToSpecificType(intValue, originalValue)); 
                    break;
                case NumericType.FLOAT:
                    var floatValue = EditorGUILayout.Slider((float)value, (float)min, (float)max);
                    valueProperty.SetRawValue(BoxFloatToSpecificType(floatValue, originalValue));
                    break;
                default:
                    EditorGUILayout.PropertyField(valueProperty);
                    if (min.CompareTo(max) > 0) {
                        (min, max) = (max, min);
                    }
                    if (value.CompareTo(min) < 0) {
                        value = min;
                    } else if (value.CompareTo(max) > 0) {
                        value = max;
                    }
                    valueProperty.SetRawValue(value);
                    break;
            }
        } else {
            EditorGUILayout.PropertyField(valueProperty);
        }
        serializedObject.ApplyModifiedProperties();
    }
}