using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using Unity.EditorCoroutines.Editor;

namespace BakeATA {

    public class BakeATA : EditorWindow {
        public Transform BakeTarget;
        public Transform AlembicTarget;
        public float StartTime = 0.0f;
        public float EndTime = 10.0f;
        public float SampleRate = 24.0f;

        SerializedProperty timeProp = null;
        SerializedProperty startTimeProp = null;
        SerializedProperty endTimeProp = null;
        SerializedObject alembicObject = null;
        EditorCoroutine currentBaking = null;

        List<Transform> animatedTransform = new List<Transform>();

        public string ExportPath = "Assets/Export/";

        bool bakingInProgress = false;

        [MenuItem("Window/BakeATA")]
        public static void ShowWindow() {
            EditorWindow.GetWindow(typeof(BakeATA));
        }

        void OnGUI() {
            GUILayout.Space(2);
            GUILayout.Label("Bake Target",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            BakeTarget = EditorGUILayout.ObjectField("GameObject to bake",BakeTarget,typeof(Transform),true) as Transform;
            EditorGUI.EndChangeCheck();
            GUILayout.Space(2);
            GUILayout.Label("Alembic Target",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            AlembicTarget = EditorGUILayout.ObjectField("Alembic to bake",AlembicTarget,typeof(Transform),true) as Transform;
            GUILayout.Space(2);
            GUILayout.Label("Export",EditorStyles.boldLabel);
            ExportPath = EditorGUILayout.TextField("Export path",ExportPath);
            GUILayout.Space(10);
            GUILayout.Label("Animation info",EditorStyles.boldLabel);
            StartTime = EditorGUILayout.FloatField("Start time",StartTime);
            EndTime = EditorGUILayout.FloatField("End time",EndTime);
            SampleRate = EditorGUILayout.FloatField("Sample rate",SampleRate);

            if(bakingInProgress) {
                if(GUILayout.Button("Cancel bake")) {
                    CancelBake();
                }
            } else {
                if(GUILayout.Button("Bake Animate")) {
                    BakeMesh();
                }
            }
        }

        private void BakeMesh() {
            Debug.Log("Start baking mesh!");
            currentBaking = EditorCoroutineUtility.StartCoroutine(ExportFrames(),this);
        }
        private void CancelBake() {
            Debug.Log("Cancel current baking!");
            EditorCoroutineUtility.StopCoroutine(currentBaking);
        }

        SerializedObject InitAlembic() {
            if(BakeTarget == null) {
                Debug.LogError("No target to bake!");
                return null;
            }

            if(AlembicTarget == null) {
                Debug.LogError("No Alembic player");
                return null;
            }

            var alembicPlayer = AlembicTarget.GetComponent("AlembicStreamPlayer");
            if(alembicPlayer == null) {
                Debug.LogError("Alembic player!");
                return null;
            }
            alembicObject = new SerializedObject(alembicPlayer);

            timeProp = alembicObject.FindProperty("currentTime");
            startTimeProp = alembicObject.FindProperty("startTime");
            endTimeProp = alembicObject.FindProperty("endTime");

            return alembicObject;
        }

        IEnumerator ExportFrames() {
            animatedTransform.Clear();
            SerializedObject alembic = InitAlembic();
            int framesCount = Mathf.RoundToInt((EndTime - StartTime) * SampleRate + 0.5f);

            AnimationClip clip = new AnimationClip();
            string filePath = ExportPath + BakeTarget.name + "ATA.anim";
            clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(filePath,typeof(AnimationClip));
            if(clip == null) {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip,filePath);
            }

            Keyframe[] ksPosx = new Keyframe[framesCount];
            Keyframe[] ksPosy = new Keyframe[framesCount];
            Keyframe[] ksPosz = new Keyframe[framesCount];
            Keyframe[] ksRotw = new Keyframe[framesCount];
            Keyframe[] ksRotx = new Keyframe[framesCount];
            Keyframe[] ksRoty = new Keyframe[framesCount];
            Keyframe[] ksRotz = new Keyframe[framesCount];
            Keyframe[] ksScax = new Keyframe[framesCount];
            Keyframe[] ksScay = new Keyframe[framesCount];
            Keyframe[] ksScaz = new Keyframe[framesCount];
            AnimationCurve animCurvePosx = new AnimationCurve();
            AnimationCurve animCurvePosy = new AnimationCurve();
            AnimationCurve animCurvePosz = new AnimationCurve();
            AnimationCurve animCurveRotw = new AnimationCurve();
            AnimationCurve animCurveRotx = new AnimationCurve();
            AnimationCurve animCurveRoty = new AnimationCurve();
            AnimationCurve animCurveRotz = new AnimationCurve();
            AnimationCurve animCurveScax = new AnimationCurve();
            AnimationCurve animCurveScay = new AnimationCurve();
            AnimationCurve animCurveScaz = new AnimationCurve();

            timeProp.floatValue = StartTime;
            alembicObject.ApplyModifiedProperties();
            yield return null;

            for(int frame = 0; frame < framesCount; frame++) {
                float timing = StartTime + ((float)frame) / SampleRate;
                Debug.Log("Encoding frame " + frame + " / " + framesCount + " (" + timing + ")");
                timeProp.floatValue = timing;
                alembicObject.ApplyModifiedProperties();
                animatedTransform.Add(BakeTarget.transform);
                ksPosx[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.position.x);
                ksPosy[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.position.y);
                ksPosz[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.position.z);
                ksRotw[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localRotation.w);
                ksRotx[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localRotation.x);
                ksRoty[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localRotation.y);
                ksRotz[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localRotation.z);
                ksScax[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localScale.x);
                ksScay[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localScale.y);
                ksScaz[frame] = new Keyframe(frame / SampleRate,BakeTarget.transform.localScale.z);
                yield return null;
            }
            animCurvePosx = new AnimationCurve(ksPosx);
            animCurvePosy = new AnimationCurve(ksPosy);
            animCurvePosz = new AnimationCurve(ksPosz);
            animCurveRotw = new AnimationCurve(ksRotw);
            animCurveRotx = new AnimationCurve(ksRotx);
            animCurveRoty = new AnimationCurve(ksRoty);
            animCurveRotz = new AnimationCurve(ksRotz);
            animCurveScax = new AnimationCurve(ksScax);
            animCurveScay = new AnimationCurve(ksScay);
            animCurveScaz = new AnimationCurve(ksScaz);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localPosition.x",animCurvePosx);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localPosition.y",animCurvePosy);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localPosition.z",animCurvePosz);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localRotation.x",animCurveRotx);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localRotation.y",animCurveRoty);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localRotation.z",animCurveRotz);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localRotation.w",animCurveRotw);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localScale.x",animCurveScax);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localScale.y",animCurveScay);
            clip.SetCurve(BakeTarget.name,typeof(Transform),"localScale.z",animCurveScaz);
            Debug.Log("Finished Baking");
        }
    }
}