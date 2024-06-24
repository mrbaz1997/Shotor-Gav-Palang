using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MultiSelectTagsAttribute))]
public class MultiSelectTagsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        if (property.propertyType == SerializedPropertyType.String)
        {
            int selectedMask = GetSelectedMask(property.stringValue, tags);
            int newMask = EditorGUI.MaskField(position, label, selectedMask, tags);
            property.stringValue = GetSelectedString(newMask, tags);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use MultiSelectTags with a string.");
        }
    }

    private int GetSelectedMask(string selectedOptions, string[] options)
    {
        int mask = 0;
        string[] selected = selectedOptions.Split(',');
        for (int i = 0; i < options.Length; i++)
        {
            if (selected.Contains(options[i]))
            {
                mask |= 1 << i;
            }
        }
        return mask;
    }

    private string GetSelectedString(int mask, string[] options)
    {
        string[] selectedOptions = options.Where((option, index) => (mask & (1 << index)) != 0).ToArray();
        return string.Join(",", selectedOptions);
    }
}

public class MultiSelectTagsAttribute : PropertyAttribute
{
    public MultiSelectTagsAttribute() { }
}