using UnityEditor;
using UnityEngine;

[System.Serializable]
public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public NamedArrayAttribute(string[] names) { this.names = names; }
}
