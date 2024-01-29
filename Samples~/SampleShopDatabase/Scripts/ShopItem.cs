using UnityEngine;

namespace Platinio.ScriptableObjectDatabase.Sample
{
    public class ShopItem : ScriptableItem
    {
        [SerializeField] private int cost;
        [SerializeField, TextArea] private string description;

        public int Cost => cost;
        public string Description => description;
    }
}

