#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ArcaneOnyx.ScriptableObjectDatabase
{
    public class ScriptableItem : ScriptableObject
    {
        public string Name;

        [SerializeField] private Sprite icon;
        [SerializeField] protected string id;
       
        public string Id => id;
        public Sprite Icon => icon;

        public virtual void OnSave()
        {
            name = Name;
        }

        public void SetId(string id)
        {
            this.id = id;
        }
    }
}