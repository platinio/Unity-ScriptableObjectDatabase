using System;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ScriptableItemDatabaseSelector : PropertyAttribute
    {
        public Type DatabaseType;
        public bool CanBeNull;
        public string DatabaseName;

        public ScriptableItemDatabaseSelector(Type databaseType, bool canBeNull = false, string name = null)
        {
            DatabaseType = databaseType;
            DatabaseName = name;
            CanBeNull = canBeNull;
        }
    }
}

