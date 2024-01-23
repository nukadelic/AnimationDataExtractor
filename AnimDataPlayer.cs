
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimDataPlayer : MonoBehaviour
{
    [System.Serializable] 
    public class AnimDataBindLink
    {
        public string path;
        public string property;
        public Transform target;

        internal AnimDataTypes.DataCurve data;
    }

    [System.Serializable] 
    public class AnimDataBinds
    {
        [HideInInspector] public AnimDataCurve link;

        public List<AnimDataBindLink> data;
    }

    public AnimDataCurve animationData;

    public Transform root;

    [HideInInspector] public string prefix = "mixamorig:";
    
    public AnimDataBinds binds;

    [Range(0,1)] public float time = 0;


    private void OnValidate()
    {
        if( animationData == null ) binds = null;

        if( animationData != null && binds.link != animationData )
        {
            binds = new AnimDataBinds();
            binds.link = animationData;
            binds.data = new List<AnimDataBindLink>();

            for( var i = 0; i < animationData.data.Count; ++i )
            {
                binds.data.Add( new AnimDataBindLink { 
                    path = animationData.data[i].path ,
                    property = animationData.data[i].propertyName,
                    data = animationData.data[i]
                } );
            }
        }

        if( root != null && animationData != null && binds.link == animationData )
        {
            foreach( var bind in binds.data )
            {
                if( bind.target == null || bind.data == null ) continue;

                var t = bind.data.TotalTime() * time; 
                
                if( bind.data.dataType == AnimDataTypes.DataType.LocalPosition )
                {
                    bind.target.localPosition = bind.data.ToPosition( bind.data.Evaluate( t ) );
                }
                else if( bind.data.dataType == AnimDataTypes.DataType.LocalRotation )
                {
                    bind.target.localRotation = bind.data.ToRotation( bind.data.Evaluate( t ) );
                }
            }
        }
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(AnimDataPlayer))]
    class Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = (AnimDataPlayer) target;

            if( target == null ) return;

            GUILayout.Space(10);

            if( script.root != null && script.binds.data != null && script.binds.data.Count > 0 )
            {
                if( GUILayout.Button("Try auto bind") )
                {
                    var transforms = new List<Transform>();

                    transforms.Add(script.root);

                    transforms.AddRange(script.root.GetComponentsInChildren<Transform>());

                    for( var i = 0; i < script.binds.data.Count; ++i )
                    {
                        var link = script.binds.data[i];

                        if( link.target != null ) continue;

                        var path = link.path;

                        var paths = path.Split("/");

                        path = paths[ paths.Length - 1 ];

                        for( var j = 0; j < transforms.Count; ++j )
                        {
                            if( path == transforms[j].name )
                            {
                                link.target = transforms[j];
                            }
                        }
                    }
                }

                EditorGUILayout.HelpBox("The string match will be exact and it will ignore path hirarchy, simply the first gameobject who's name matches the last path item in the list will be linked", MessageType.None);

                using( new GUILayout.HorizontalScope() )
                {
                    GUILayout.Label("Prefix:" , GUILayout.Width(60) );

                    script.prefix = GUILayout.TextField( script.prefix );

                    if( script.prefix.Length > 0 )
                    {
                        if( GUILayout.Button("Remove prefix from children") )
                        {
                            var transforms = new List<Transform>();

                            transforms.Add(script.root);

                            transforms.AddRange(script.root.GetComponentsInChildren<Transform>());

                            Undo.RecordObjects( transforms.ToArray() , "rename" );

                            for( var i = 0; i < transforms.Count; ++i )
                            {
                                var t = transforms[i];

                                if( t.name.IndexOf(script.prefix) > -1 )
                                {
                                    t.name = t.name.Replace( script.prefix, "" );
                                }
                            }

                            script.prefix = "";
                        }
                    }
                }
            }

        }
    }

#endif

}
