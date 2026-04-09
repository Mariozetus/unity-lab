using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SingleLayerAttribute))]
public class SingleLayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        if (property.propertyType == SerializedPropertyType.Integer)
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        else
            EditorGUI.PropertyField(position, property, label);
            
        EditorGUI.EndProperty();
    }
}