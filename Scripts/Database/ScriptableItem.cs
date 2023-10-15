using UnityEngine;

namespace ScriptableObjectDatabase
{
    public class ScriptableItem : ScriptableObject
    {
        public string Name;
        [HideInInspector] public uint Id;
    }
}