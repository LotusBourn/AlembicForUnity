using System;
using System.IO;
using UnityEngine;
using UnityEngine.Formats.Alembic.Exporter;
using UnityEngine.Formats.Alembic.Sdk;
using UnityEngine.Formats.Alembic.Util;

namespace UnityEditor.Formats.Alembic.Exporter
{

    [CustomEditor(typeof(AlembicExporter))]
    internal class AlembicExporterEditor : Editor
    {
        bool m_foldCaptureComponents;
        bool m_foldMeshComponents;

        public override void OnInspectorGUI()
        {
            var t = target as AlembicExporter;
            var settingsProp = serializedObject.FindProperty("m_recorder.m_settings");
            DrawRecorderSettings(serializedObject, settingsProp,ref m_foldCaptureComponents, ref m_foldMeshComponents);

            // capture control
            EditorGUILayout.LabelField("Capture Control", EditorStyles.boldLabel);
            if (t.recorder.recording)
            {
                if (GUILayout.Button("End Recording"))
                    t.EndRecording();
            }
            else
            {
                if (GUILayout.Button("Begin Recording"))
                    t.BeginRecording();

                if (GUILayout.Button("One Shot"))
                    t.OneShot();
            }
        }

        internal static void DrawRecorderSettings(SerializedObject so, SerializedProperty settingsProp, ref bool foldCaptureComponents, ref bool foldMeshComponents)
        {
            var outputPathProperty = settingsProp.FindPropertyRelative("outputPath");

            // output path
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Output Path", EditorStyles.boldLabel);
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                var s = EditorGUILayout.TextField(outputPathProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    outputPathProperty.stringValue = s;
                }
                

                if (GUILayout.Button("...", GUILayout.Width(24)))
                {
                    var dir = "";
                    var filename = "";
                    try
                    {
                        dir = Path.GetDirectoryName(outputPathProperty.stringValue);
                        filename = Path.GetFileName(outputPathProperty.stringValue);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var path = EditorUtility.SaveFilePanel("Output Path", dir, filename, "abc");
                    if (path.Length > 0)
                    {
                        outputPathProperty.stringValue = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(5);


            // alembic settings
            EditorGUILayout.LabelField("Alembic Settings", EditorStyles.boldLabel);
            {
                var confProperty = settingsProp.FindPropertyRelative("conf");
                EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("archiveType"));
                EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("xformType"));
                var timeSamplingType = confProperty.FindPropertyRelative("timeSamplingType");
                EditorGUILayout.PropertyField(timeSamplingType);
                if (timeSamplingType.intValue == (int)aeTimeSamplingType.Uniform)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("frameRate"));
                    EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("fixDeltaTime"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("swapHandedness"));
                EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("swapFaces"));
                EditorGUILayout.PropertyField(confProperty.FindPropertyRelative("scaleFactor"));
            }
            GUILayout.Space(5);


            // capture settings
            EditorGUILayout.LabelField("Capture Settings", EditorStyles.boldLabel);
            var scope = settingsProp.FindPropertyRelative("scope");
            EditorGUILayout.PropertyField(scope);
            if (scope.intValue == (int)ExportScope.TargetBranch)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                var targetBranch = so.FindProperty("m_recorder.m_settings.targetBranch");
                var branch = EditorGUILayout.ObjectField("Target", targetBranch.objectReferenceValue, typeof(GameObject), true) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    targetBranch.objectReferenceValue = branch;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("assumeNonSkinnedMeshesAreConstant"), new GUIContent("Static MeshRenderers"));
            GUILayout.Space(5);

            foldCaptureComponents = EditorGUILayout.Foldout(foldCaptureComponents, "Capture Components");
            if (foldCaptureComponents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("captureMeshRenderer"), new GUIContent("MeshRenderer"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("captureSkinnedMeshRenderer"), new GUIContent("SkinnedMeshRenderer"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("captureCamera"), new GUIContent("Camera"));
                EditorGUI.indentLevel--;
            }

            foldMeshComponents = EditorGUILayout.Foldout(foldMeshComponents, "Mesh Components");
            if (foldMeshComponents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("meshNormals"), new GUIContent("Normals"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("meshUV0"), new GUIContent("UV 1"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("meshUV1"), new GUIContent("UV 2"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("meshColors"), new GUIContent("Vertex Color"));
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("meshSubmeshes"), new GUIContent("Submeshes"));
                EditorGUI.indentLevel--;
            }
            {
                var mCaptureOnStart = so.FindProperty("m_captureOnStart");
                EditorGUILayout.PropertyField(mCaptureOnStart);
                if (mCaptureOnStart.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(so.FindProperty("m_ignoreFirstFrame"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(so.FindProperty("m_maxCaptureFrame"));
            }
            GUILayout.Space(5);

            // misc settings
            EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
            {
                EditorGUILayout.PropertyField(settingsProp.FindPropertyRelative("detailedLog"));
            }
            GUILayout.Space(10);
            so.ApplyModifiedProperties();
        }
    }
}
