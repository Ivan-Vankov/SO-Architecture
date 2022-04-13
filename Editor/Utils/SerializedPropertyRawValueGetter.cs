using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

// From https://gist.github.com/yasirkula/9a00f988cdc7354eef52d46d8db2fe3b
// Credit: http://answers.unity.com/answers/425602/view.html (I've only slightly modified the code)
public static class SerializedPropertyRawValueGetter {
	public static object GetRawValue(this SerializedProperty property) {
		object result = property.serializedObject.targetObject;
		string[] path = property.propertyPath.Replace(".Array.data[", "[").Split('.');
		for (int i = 0; i < path.Length; i++) {
			string pathElement = path[i];

			int arrayStartIndex = pathElement.IndexOf('[');
			if (arrayStartIndex < 0)
				result = GetFieldValue(result, pathElement);
			else {
				string variableName = pathElement.Substring(0, arrayStartIndex);

				int arrayEndIndex = pathElement.IndexOf(']', arrayStartIndex + 1);
				int arrayElementIndex = int.Parse(pathElement.Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1));
				result = GetFieldValue(result, variableName, arrayElementIndex);
			}
		}

		return result;
	}

	private static object GetFieldValue(object source, string fieldName) {
		if (source == null)
			return null;

		FieldInfo fieldInfo = null;
		Type type = source.GetType();
		while (fieldInfo == null && type != typeof(object)) {
			fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			type = type.BaseType;
		}

		if (fieldInfo != null)
			return fieldInfo.GetValue(source);

		PropertyInfo propertyInfo = null;
		type = source.GetType();
		while (propertyInfo == null && type != typeof(object)) {
			propertyInfo = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
			type = type.BaseType;
		}

		if (propertyInfo != null)
			return propertyInfo.GetValue(source, null);

		if (fieldName.Length > 2 && fieldName.StartsWith("m_", StringComparison.OrdinalIgnoreCase))
			return GetFieldValue(source, fieldName.Substring(2));

		return null;
	}

	private static object GetFieldValue(object source, string fieldName, int arrayIndex) {
		IEnumerable enumerable = GetFieldValue(source, fieldName) as IEnumerable;
		if (enumerable == null)
			return null;

		if (enumerable is IList)
			return ((IList)enumerable)[arrayIndex];

		IEnumerator enumerator = enumerable.GetEnumerator();
		for (int i = 0; i <= arrayIndex; i++)
			enumerator.MoveNext();

		return enumerator.Current;
	}

	public static void SetRawValue(this SerializedProperty property, object value) {
		// Assume we have component A which has a struct variable called B and we want to change B's C variable's value
		// with this function. If all we do is get B's corresponding FieldInfo for C and call its SetValue function, we
		// won't really change the value of A.B.C because B is a struct which was boxed when we called SetValue and we
		// essentially changed a copy of B, not B itself. So, we need to keep a reference to our boxed B variable, change
		// its C variable and then assign the boxed B value back to A. This way, we will in fact change the value of A.B.C
		// 
		// In this code, there are 2 for loops. In the first loop, we are basically storing the boxed values (B) in setValues
		// and at the end of the loop, we change B.C's value. In the second loop, we assign boxed values back to their parent
		// variables (assigning boxed B value back to A)
		string[] path = property.propertyPath.Replace(".Array.data[", "[").Split('.');
		object[] setValues = new object[path.Length];
		setValues[0] = property.serializedObject.targetObject;
		for (int i = 0; i < path.Length; i++) {
			string pathElement = path[i];

			int arrayStartIndex = pathElement.IndexOf('[');
			if (arrayStartIndex < 0) {
				if (i < path.Length - 1)
					setValues[i + 1] = GetFieldValue(setValues[i], pathElement);
				else
					SetFieldValue(setValues[i], pathElement, value);
			} else {
				string variableName = pathElement.Substring(0, arrayStartIndex);

				int arrayEndIndex = pathElement.IndexOf(']', arrayStartIndex + 1);
				int arrayElementIndex = int.Parse(pathElement.Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1));
				if (i < path.Length - 1)
					setValues[i + 1] = GetFieldValue(setValues[i], pathElement, arrayElementIndex);
				else
					SetFieldValue(setValues[i], variableName, arrayElementIndex, value);
			}
		}

		for (int i = path.Length - 2; i >= 0; i--) {
			string pathElement = path[i];

			int arrayStartIndex = pathElement.IndexOf('[');
			if (arrayStartIndex < 0)
				SetFieldValue(setValues[i], pathElement, setValues[i + 1]);
			else {
				string variableName = pathElement.Substring(0, arrayStartIndex);

				int arrayEndIndex = pathElement.IndexOf(']', arrayStartIndex + 1);
				int arrayElementIndex = int.Parse(pathElement.Substring(arrayStartIndex + 1, arrayEndIndex - arrayStartIndex - 1));
				SetFieldValue(setValues[i], variableName, arrayElementIndex, setValues[i + 1]);
			}
		}
	}

	private static void SetFieldValue(object source, string fieldName, object value) {
		if (source == null)
			return;

		FieldInfo fieldInfo = null;
		Type type = source.GetType();
		while (fieldInfo == null && type != typeof(object)) {
			fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			type = type.BaseType;
		}

		if (fieldInfo != null) {
			fieldInfo.SetValue(source, value);
			return;
		}

		PropertyInfo propertyInfo = null;
		type = source.GetType();
		while (propertyInfo == null && type != typeof(object)) {
			propertyInfo = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
			type = type.BaseType;
		}

		if (propertyInfo != null) {
			propertyInfo.SetValue(source, value, null);
			return;
		}

		if (fieldName.Length > 2 && fieldName.StartsWith("m_", StringComparison.OrdinalIgnoreCase))
			SetFieldValue(source, fieldName.Substring(2), value);
	}

	private static void SetFieldValue(object source, string fieldName, int arrayIndex, object value) {
		IEnumerable enumerable = GetFieldValue(source, fieldName) as IEnumerable;
		if (enumerable is IList)
			((IList)enumerable)[arrayIndex] = value;
	}
}