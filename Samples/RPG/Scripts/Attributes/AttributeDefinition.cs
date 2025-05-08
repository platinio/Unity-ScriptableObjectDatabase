using ArcaneOnyx.ScriptableObjectDatabase;
using UnityEngine;

namespace ArcaneOnyx.RPGSample
{
    public class AttributeDefinition : ScriptableItem
    {
        [SerializeField] private Color color;

        public Color Color => color;
    }
}