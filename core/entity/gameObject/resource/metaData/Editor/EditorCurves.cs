using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace WorldWizards.core.entity.gameObject
{
    [CustomEditor(typeof(WWSeeker))]
    public class EditorCurves : Editor
    {
        public enum Action2 //actions
        {
            Attack,
            Flee,
        };

        List<int[]> pairs = new List<int[]>();

        public string[] options = new string[] { "None", "Attack", "Flee", "Regroup" };
        public string[] options2 = new string[] { "None", "Health", "Allies", "Enemies" };
        List<int> selectionsA = new List<int>() { 0 };
        List<int> selectionsB = new List<int>();
        int selectedA = 0;
        int selectedB = 0;
        AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        override public void OnInspectorGUI()
        {

            var myScript = target as WWSeeker;

            //using (new EditorGUI.DisabledScope(myScript.enemy))
            //{
            myScript.enemy = EditorGUILayout.Toggle("Enemy", myScript.enemy);
            myScript.turnSpeed = EditorGUILayout.FloatField("Turn Speed", myScript.turnSpeed);
            myScript.maxWalkSpeed = EditorGUILayout.FloatField("Walk Speed", myScript.maxWalkSpeed);
            //}

            if (myScript.enemy)
            {

            }

            //Debug.Log(myScript.mylist[0].action);

            for (var i = 0; i < selectionsA.Count; i++)
            {
                if (i < selectionsB.Count && selectionsA[i] > 0 && selectionsB[i] > 0)
                {
                    if (i == pairs.Count)
                    {
                        int[] pair = { selectionsA[i], selectionsB[i] };
                        pairs.Add(pair);
                    }
                    else
                    {
                        var pair = pairs[i];
                        pair[0] = selectionsA[i];
                        pair[1] = selectionsB[i];
                        pairs[i] = pair;
                    }
                }
            }
            for (var i = 0; i < pairs.Count; i++)
            {
                Debug.Log("[" + pairs[i][0].ToString() + ", " + pairs[i][1].ToString() + "]");
            }

            EditorGUI.indentLevel++;
            ///

            for (var i = 0; i < selectionsA.Count; i++)
            {
                //horiz line
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                rect.height = 1;
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

                selectedA = selectionsA[i];
                if (selectedA > 0 || i == selectionsA.Count-1)
                {
                    EditorGUILayout.LabelField("Action");
                    selectionsA[i] = EditorGUILayout.Popup(selectedA, options);
                    if (selectedA > 0)
                    {

                        if (i < selectionsB.Count)
                        {
                            selectedB = selectionsB[i];
                        }
                        else
                        {
                            selectionsB.Add(0);
                            selectedB = 0;
                        }
                        EditorGUILayout.LabelField("Context");
                        selectionsB[i] = EditorGUILayout.Popup(selectedB, options2);
                        
                        if (selectedB > 0)
                        {
                            var curveX = EditorGUILayout.CurveField(options[selectedA].ToString()+" VS "+options2[selectedB].ToString(), curve);
                            if (i == selectionsA.Count - 1)
                            {
                                selectionsA.Add(0);
                            }
                        }
                    }
                } else
                {
                    selectionsA.RemoveAt(i);
                    selectionsB.RemoveAt(i);
                    i--;
                }
            }
            ///
            EditorGUI.indentLevel--;

            //EditorGUILayout.LabelField("Curve Editor");
            //var serializedObject = new SerializedObject(myScript);
            //var property = serializedObject.FindProperty("mylist");

            //property.GetArrayElementAtIndex(0).FindPropertyRelative("action").;
            //property.GetArrayElementAtIndex(0).FindPropertyRelative("action").intValue = 1;

            //serializedObject.ApplyModifiedProperties();
            //property.GetArrayElementAtIndex(0).GetArrayElementAtIndex(0));
            //Debug.Log(property.GetArrayElementAtIndex(0).FindPropertyRelative("action").intValue);


            //serializedObject.Update();
            //EditorGUILayout.PropertyField(property, true);
            //serializedObject.ApplyModifiedProperties();
            //myScript.mylist = EditorGUILayout.Ar("Enemy", myScript.mylist);

            //myScript.mylist[0].action = (Curves.Action) Action2.Attack;
            //myScript.mylist = GUILayout.Toggle(myScript.mylist, "Flag");

            //if (myScript.flag)
            //    myScript.i = EditorGUILayout.IntSlider("I field:", myScript.i, 1, 100);

        }
    }
}