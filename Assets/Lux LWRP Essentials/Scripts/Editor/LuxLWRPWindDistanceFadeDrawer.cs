using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxLWRPDistanceFadeDrawer : MaterialPropertyDrawer {

	public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
		
	//	Needed since Unity 2019
		EditorGUIUtility.labelWidth = 0;

		Vector2 vec2value = prop.vectorValue;
		
		vec2value.x = prop.vectorValue.x;
		vec2value.x = Mathf.Sqrt(vec2value.x);

		vec2value.y = 0.1f / vec2value.y;
		vec2value.y = Mathf.Sqrt(vec2value.y);

		GUILayout.Space(-18);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginVertical();
			vec2value.x = EditorGUILayout.FloatField("Max Distance", vec2value.x);
			vec2value.y = EditorGUILayout.FloatField("Fade Range", vec2value.y);
		EditorGUILayout.EndVertical();
		if (EditorGUI.EndChangeCheck ()) {
			vec2value.x *= vec2value.x;
			vec2value.y *= vec2value.y;
			vec2value.y = 0.1f / vec2value.y;
			prop.vectorValue = vec2value;
		}
		//EditorGUILayout.Vector2Field("test", prop.vectorValue);
	}
}