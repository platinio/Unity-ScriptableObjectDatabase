using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableItem : ScriptableObject
    {
        public string Name;

        [SerializeField] private Sprite icon;
        [SerializeField, HideInInspector] protected uint id;
       
        public uint Id => id;
        public Sprite Icon => icon;

        public virtual void OnSave()
        {
            name = Name;
        }

        public void SetId(uint id)
        {
            this.id = id;
        }
    }
}