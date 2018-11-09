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
        public string[] options3 = new string[] { "None", "Health", "Allies", "Enemies" };

        List<string> options4 = new List<string>();
        List<int> selectionsA = new List<int>() { 0 };
        List<int> selectionsB = new List<int>();
        List<AnimationCurve> curves = new List<AnimationCurve>();
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
                    if (i < selectionsB.Count)
                    {
                        selectedB = selectionsB[i];
                    }
                    else
                    {
                        selectionsB.Add(0);
                        selectedB = 0;
                    }

                    if (selectedB > 0)
                    {
                        if (i >= curves.Count)
                        {
                            AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
                            curves.Add(curve);
                        }
                        var curveX = EditorGUILayout.CurveField(options[selectedA].ToString() + " VS " + options2[selectedB].ToString(), curves[i]);
                        if (i == selectionsA.Count - 1)
                        {
                            selectionsA.Add(0);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Action");
                        selectedA = EditorGUILayout.Popup(selectedA, options);
                        selectionsA[i] = selectedA;

                        if (selectedA > 0)
                        {

                            options4.Clear();

                            for (var j = 0; j < options2.Length; j++)
                            {
                                options3[j] = options2[j];
                            }
                            //mark pairs
                            for (var j = 0; j < pairs.Count; j++)
                            {
                                if (pairs[j][0] == selectedA)
                                {
                                    options3[pairs[j][1]] = "";
                                }
                            }

                            for (var j = 0; j < options3.Length; j++)
                            {
                                if (options3[j] != "")
                                {
                                    options4.Add(options3[j]);
                                } else {
                                    options4.Add("");
                                }
                            }
                            EditorGUILayout.LabelField("Context");
                            selectedB = EditorGUILayout.Popup(selectedB, options4.ToArray());
                            selectionsB[i] = selectedB;
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