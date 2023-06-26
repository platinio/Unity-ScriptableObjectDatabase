using System;
using UnityEngine;

namespace ScriptableObjectDatabase
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ScriptableItemDatabaseSelector : PropertyAttribute
    {
        public Type DatabaseType;

        public ScriptableItemDatabaseSelector(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}

