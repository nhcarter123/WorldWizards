using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace WorldWizards.core.entity.gameObject
{
    [CustomEditor(typeof(WWSeeker))]
    public class EditorCurves : Editor
    {
        bool init = true;

        List<string> options3 = new List<string>();
        List<int> options4 = new List<int>();
        int selectedA = 0;
        int selectedB = 0;
        AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        override public void OnInspectorGUI()
        {

            var myScript = target as WWSeeker;

            myScript.team = EditorGUILayout.IntSlider("Team", myScript.team, 0, 5);
            myScript.max_health = EditorGUILayout.IntSlider("Max Health", myScript.max_health, 0, 100);
            myScript.health = EditorGUILayout.IntSlider("Health", myScript.health, 0, myScript.max_health);
            myScript.turnSpeed = EditorGUILayout.Slider("Turn Speed", myScript.turnSpeed, 1, 10);
            myScript.maxWalkSpeed = EditorGUILayout.Slider("Walk Speed", myScript.maxWalkSpeed, 1, 10);
            myScript.attackDistance = EditorGUILayout.Slider("Attack Range", myScript.attackDistance, 1, 10);
            myScript.aggroDistance = EditorGUILayout.Slider("Aggro Range", myScript.aggroDistance, 1, 100);
            myScript.deAggroDistance = EditorGUILayout.Slider("De-Aggro Range", myScript.deAggroDistance, 1, 100);

            if (init)
            {
                init = false;
                options3.AddRange(System.Enum.GetNames(typeof(contexts_enum)));
            }

            //Debug.Log(myScript.mylist[0].action);
            myScript.pairs.Clear();

            for (var i = 0; i < myScript.selectionsA.Count; i++)
            {
                if (i < myScript.selectionsB.Count && myScript.selectionsA[i] > 0 && myScript.selectionsB[i] > 0)
                {
                    if (i == myScript.pairs.Count)
                    {
                        int[] pair = { myScript.selectionsA[i], myScript.selectionsB[i] };
                        myScript.pairs.Add(pair);
                    }
                    else
                    {
                        var pair = myScript.pairs[i];
                        pair[0] = myScript.selectionsA[i];
                        pair[1] = myScript.selectionsB[i];
                        myScript.pairs[i] = pair;
                    }
                }
            }
            for (var i = 0; i < myScript.pairs.Count; i++)
            {
                Debug.Log("[" + myScript.pairs[i][0].ToString() + ", " + myScript.pairs[i][1].ToString() + "]");
            }

            EditorGUI.indentLevel++;
            ///

            for (var i = 0; i < myScript.selectionsA.Count; i++)
            {
                //horiz line
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                rect.height = 1;
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

                selectedA = myScript.selectionsA[i];
                if (selectedA > 0 || i == myScript.selectionsA.Count-1)
                {
                    if (i < myScript.selectionsB.Count)
                    {
                        selectedB = myScript.selectionsB[i];
                    }
                    else
                    {
                        myScript.selectionsB.Add(0);
                        selectedB = 0;
                    }

                    if (selectedB > 0)
                    {
                        if (i >= myScript.curves.Count)
                        {
                            AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
                            myScript.curves.Add(curve);
                            myScript.UpdateActiveActionsContexts();
                        }
                        var curveX = EditorGUILayout.CurveField(myScript.actions[selectedA].ToString() + " VS " + myScript.contexts[selectedB].ToString(), myScript.curves[i]);
                        if (GUILayout.Button("Delete")) {
                            myScript.selectionsA.RemoveAt(i);
                            myScript.selectionsB.RemoveAt(i);
                            i--;
                        }
                        if (i == myScript.selectionsA.Count - 1)
                        {
                            myScript.selectionsA.Add(0);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Action");
                        selectedA = EditorGUILayout.Popup(selectedA, myScript.actions);
                        myScript.selectionsA[i] = selectedA;

                        if (selectedA > 0)
                        {

                            //setup lists
                            var j = 0;
                            for (j = 0; j < myScript.contexts.Length; j++)
                            {
                                if (options3.Count > j)
                                {
                                    options3[j] = myScript.contexts[j];
                                } else
                                {
                                    options3.Add(myScript.contexts[j]);
                                }
                                if (options4.Count > j)
                                {
                                    options4[j] = j;
                                } else
                                {
                                    options4.Add(j);
                                }
                            }

                            //mark pairs
                            for (j = 0; j < myScript.pairs.Count; j++)
                            {
                                if (myScript.pairs[j][0] == selectedA)
                                {
                                    options3[myScript.pairs[j][1]] = "";
                                }
                            }

                            //remove pairs
                            j = 0;
                            while (j < options3.Count) {
                                if (options3[j] == "")
                                {
                                    options3.RemoveAt(j);
                                    options4.RemoveAt(j);
                                    j--;
                                }
                                j++;
                            }
                            EditorGUILayout.LabelField("Context");
                            selectedB = EditorGUILayout.Popup(selectedB, options3.ToArray());
                            selectedB = options4[selectedB];
                            myScript.selectionsB[i] = selectedB;
                        }
                    }
                } else
                {
                    myScript.selectionsA.RemoveAt(i);
                    myScript.selectionsB.RemoveAt(i);
                    i--;
                }
            }
            ///
            EditorGUI.indentLevel--;
        }
    }
}