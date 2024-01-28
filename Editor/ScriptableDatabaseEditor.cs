using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Platinio.ScriptableObjectDatabase
{
    public class ScriptableDatabaseEditor<T> : UnityEditor.Editor where T : ScriptableItem
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ScriptableDatabase<T> db = target as ScriptableDatabase<T>;
            var subclasses = GetSubclasses(typeof(T));

            foreach (var classType in subclasses)
            {
                DrawCreateButton(db, classType);
            }

            db.Clean();
            db.UpdateItemsCopy();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCreateButton(ScriptableDatabase<T> db, Type type)
        {
            if (GUILayout.Button($"Create {type.Name}"))
            {
                var item = CreateInstance(type.Name) as T;
                db.AddItem(item);

                EditorUtility.SetDirty(db);
                AssetDatabase.SaveAssets();
            }
        }

        public List<Type> GetSubclasses(Type t)
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && t.IsAssignableFrom(myType) && !myType.IsGenericType))
                {
                    types.Add(type);
                }
            }

            return types;
        }
    }
}