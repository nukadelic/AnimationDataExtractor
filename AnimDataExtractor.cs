#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEditor;

public class AnimDataExtractor : EditorWindow
{
    [MenuItem("Tools/Animation Data Extractor")]
    public static void ShowExample()
    {
        AnimDataExtractor wnd = GetWindow<AnimDataExtractor>();

        wnd.titleContent = new GUIContent("Animation Data Extractor");
    }

    public AnimationClip clip;

    public enum PreviewMode { Raw, Data, Keys, Curves }

    public bool ignore2KeysOrLess = true;
    public bool applyFilter = true;
    public PreviewMode previewData = PreviewMode.Data;
    public string removePathString = "mixamorig:";

    Vector2 scroll;

    public Dictionary<string, AnimDataTypes.DataCurve> data = new Dictionary<string, AnimDataTypes.DataCurve>();

    int preview_index_i = 0;
    int preview_index_j = 0;

    string[] data_keys;

    private void OnGUI()
    {
        var W_2 = GUILayout.Width(Screen.width / 2f);

        clip = (AnimationClip) EditorGUILayout.ObjectField( new GUIContent("Clip") , clip, typeof(AnimationClip) , false );

        using (new GUILayout.HorizontalScope() )
        {
            ignore2KeysOrLess = GUILayout.Toggle(ignore2KeysOrLess, "Ignore 2 keyframes or less" , W_2);

            GUILayout.Space(10);

            applyFilter = GUILayout.Toggle(applyFilter, "Filter rotation and positions" );
        }

        using( new GUILayout.HorizontalScope() )
        {
            removePathString = EditorGUILayout.TextField("Remove Path String" , removePathString , W_2);

            GUILayout.Space(10);

            previewData = (PreviewMode) EditorGUILayout.EnumPopup( "Preview" , previewData );
        }

        data.Clear();

        if( clip == null )
        {
            return;
        }

        var curve_bindings = AnimationUtility.GetCurveBindings( clip );

        #region Extract Data
        {
            for ( var i = 0; i < curve_bindings.Length; ++i )
            {
                var bind = curve_bindings [ i ];
                var curve = AnimationUtility.GetEditorCurve( clip, curve_bindings[ i ] );
                
                var path_name = bind.path;
                path_name = path_name.Replace(removePathString, "");

                if( ignore2KeysOrLess && curve.length <= 2 )
                {
                    continue;
                }

                if( applyFilter )
                {
                    if( ! bind.propertyName.ToLower().Contains("m_localrotation") && 
                        ! bind.propertyName.ToLower().Contains("m_localposition") ) continue;
                }


                if( bind.propertyName.IndexOf("m_LocalRotation.") == 0 )
                {
                    var dict_path = path_name + "_rotation";

                    if ( ! data.ContainsKey( dict_path ) )
                    {
                        data.Add( dict_path , new AnimDataTypes.DataCurve
                        {
                            path = path_name,
                            pathRaw = bind.path,
                            dataType = AnimDataTypes.DataType.LocalRotation,
                            propertyName = "m_LocalRotation",
                            count = curve.length,
                            curves = new List<AnimationCurve>(4) { null, null, null, null }
                        });
                    }

                    if( data.ContainsKey( dict_path ) )
                    {
                        var item = data[ dict_path ];

                        var c = bind.propertyName[ bind.propertyName.Length - 1 ];

                        if (c == 'x') item.curves[0] = curve;
                        else if (c == 'y') item.curves[1] = curve;
                        else if (c == 'z') item.curves[2] = curve;
                        else if (c == 'w') item.curves[3] = curve;
                        else Debug.Log("Unknown last char : " + c );

                        data[ dict_path ] = item;
                    }
                }

                else if( bind.propertyName.IndexOf("m_LocalPosition.") == 0 )
                {
                    var dict_path = path_name + "_position";

                    if (!data.ContainsKey(dict_path) )
                    {
                        data.Add( dict_path , new AnimDataTypes.DataCurve
                        {
                            path = path_name,
                            pathRaw = bind.path,
                            dataType = AnimDataTypes.DataType.LocalPosition,
                            propertyName = "m_LocalPosition",
                            count = curve.length,
                            curves = new List<AnimationCurve>(3) { null, null, null }
                        });
                    }

                    if (data.ContainsKey(dict_path))
                    {
                        var item = data[ dict_path ];

                        var c = bind.propertyName[bind.propertyName.Length - 1];

                        if (c == 'x') item.curves[0] = curve;
                        else if (c == 'y') item.curves[1] = curve;
                        else if (c == 'z') item.curves[2] = curve;
                        else Debug.Log("Unknown last char : " + c);

                        data[ dict_path ] = item;
                    }
                }
                else
                {
                    if (!data.ContainsKey(path_name))
                    {
                        data.Add(path_name, new AnimDataTypes.DataCurve
                        {
                            path = path_name,
                            pathRaw = bind.path,
                            dataType = AnimDataTypes.DataType.Other,
                            propertyName = bind.propertyName,
                            count = curve.length,
                            curves = new List<AnimationCurve>()
                        });
                    }

                    if (data.ContainsKey(path_name))
                    {
                        var item = data[path_name];
                        
                        item.curves.Add( curve );

                        data[path_name] = item;
                    }
                }
            }

        }

        #endregion

        data_keys = data.Keys.ToArray();

        #region GUI Preview
        {
            scroll = GUILayout.BeginScrollView( scroll );
            {
                if (previewData == PreviewMode.Data )
                {
                    foreach( var item in data )
                    {
                        var value = item.Value;
                            
                        GUILayout.Label( value.propertyName + ": " + value.count + "\tPath: " + value.path );
                    }
                }
                else if( previewData == PreviewMode.Raw )
                {
                    for (var i = 0; i < curve_bindings.Length; ++i)
                    {
                        var bind = curve_bindings[i];
                        
                        // var curve = AnimationUtility.GetEditorCurve(clip, curve_bindings[i]);

                        GUILayout.Label( bind.path + ", " + bind.propertyName );
                    }
                }
                else if( previewData == PreviewMode.Keys )
                {
                    AnimDataTypes.DataCurve item;
                    
                    using( new GUILayout.HorizontalScope() )
                    {
                        preview_index_i = EditorGUILayout.IntSlider( preview_index_i , 0, data.Count - 1 );

                        item = data[data_keys[preview_index_i]];

                        preview_index_j = EditorGUILayout.IntSlider( preview_index_j, 0 , item.count - 1 );
                    }

                    var time = item.EvalTime( preview_index_j );

                    var values = item.EvalAt( preview_index_j );

                    GUILayout.Label( item.propertyName );
                    GUILayout.Label( item.path );
                    EditorGUILayout.TextField( "Time: ", time.ToString() );
                    
                    if( values != null )
                    {
                        for( var i = 0; i < values.Length; ++i )
                        {
                            EditorGUILayout.TextField( i.ToString() , values[ i ].ToString("N3") );
                        }

                        if( item.dataType == AnimDataTypes.DataType.LocalRotation )
                        {
                            var e = item.ToEulerAngles( item.ToRotation( values ) );

                            using ( new GUILayout.HorizontalScope() )
                            {
                                GUILayout.Label("Euler Angles");
                                GUILayout.TextField(e.x.ToString("N3"));
                                GUILayout.TextField(e.y.ToString("N3"));
                                GUILayout.TextField(e.z.ToString("N3"));
                            }
                        }
                    }
                }
                else if( previewData == PreviewMode.Curves )
                {
                    preview_index_i = EditorGUILayout.IntSlider(preview_index_i, 0, data.Count - 1);

                    AnimDataTypes.DataCurve item = data[data_keys[ preview_index_i ] ];

                    GUILayout.Label(item.propertyName);
                    
                    GUILayout.Label(item.path);

                    for ( var i = 0; i < item.curves.Count; ++i )
                    {
                        EditorGUILayout.CurveField( i.ToString(), item.curves[i] );
                    }

                }
            }
            GUILayout.EndScrollView();
        }

        #endregion


        #region Export Data

        if (clip != null && data.Count > 0)
        {
            if (GUILayout.Button("Export Data"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Exported"))
                {
                    AssetDatabase.CreateFolder("Assets", "Exported");
                }

                var export_path = "Assets/Exported/" + clip.name;

                if (AssetDatabase.LoadAssetAtPath<AnimDataCurve>(export_path + ".asset") != null)
                {
                    var t = System.DateTime.Now;
                    string date_time = (t.DayOfYear < 100 ? "0" : "") + t.DayOfYear.ToString();
                    date_time += t.TimeOfDay.ToString().Substring(1, 7).Replace(":", "");
                    export_path += date_time;
                }

                var so = ScriptableObject.CreateInstance<AnimDataCurve>();

                so.data = new List<AnimDataTypes.DataCurve>();

                var dict_keys = data.Keys.ToArray();

                for (var i = 0; i < data_keys.Length; ++i)
                {
                    so.data.Add(data[data_keys[i]]);
                }

                AssetDatabase.CreateAsset(so, export_path + ".asset");

                AssetDatabase.Refresh();
            }

        }

        #endregion

    }
}

#endif