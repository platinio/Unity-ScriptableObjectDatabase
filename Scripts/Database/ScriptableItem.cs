using UnityEngine;

namespace ScriptableObjectDatabase
{
    public class ScriptableItem : ScriptableObject
    {
        public string Name;
        [HideInInspector] public uint Id;

        public virtual void OnSave()
        {
            name = Name;
        }
    }
}