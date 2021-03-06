#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace ParadoxNotion.Design
{

    partial class EditorUtils
    {

        public struct ReorderableListOptions
        {
            public delegate GenericMenu GetItemMenuDelegate(int i);
            public bool allowRemove;
            public bool allowAdd;
            public GetItemMenuDelegate CustomItemMenu;
        }

        public delegate void ReorderableListCallback(int index, bool isReordering);
        private static readonly Dictionary<IList, int> pickedListIndex = new Dictionary<IList, int>();

        /// A simple reorderable list. Pass the list and a function to call for GUI. The callback comes with the current iterated element index in the list
        public static IList ReorderableList(IList list, ReorderableListOptions options, ReorderableListCallback GUICallback, UnityObject unityObject = null) {
            return Internal_ReorderableList(list, options, GUICallback, unityObject);
        }

        /// A simple reorderable list. Pass the list and a function to call for GUI. The callback comes with the current iterated element index in the list
        public static IList ReorderableList(IList list, ReorderableListCallback GUICallback, UnityObject unityObject = null) {
            return Internal_ReorderableList(list, default(ReorderableListOptions), GUICallback, unityObject);
        }

        /// A simple reorderable list. Pass the list and a function to call for GUI. The callback comes with the current iterated element index in the list
        static IList Internal_ReorderableList(IList list, ReorderableListOptions options, ReorderableListCallback GUICallback, UnityObject unityObject) {

            if ( list == null ) {
                return null;
            }

            var listType = list.GetType();
            var argType = listType.GetEnumerableElementType();
            if ( argType == null ) {
                return list;
            }

            if ( options.allowAdd ) {

                var dropRefs = DragAndDrop.objectReferences;

                //Drag And Drop.
                if ( dropRefs.Length > 0 ) {
                    if ( dropRefs.Any(r => argType.IsAssignableFrom(r.GetType()) || ( r.GetType() == typeof(GameObject) && typeof(Component).IsAssignableFrom(argType) )) ) {
                        var dropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                        dropRect.xMin += 5;
                        dropRect.xMax -= 5;
                        GUI.Box(dropRect, "", Styles.roundedBox);
                        GUI.Box(dropRect, "Drop Here to Enlist", Styles.centerLabel);
                        if ( dropRect.Contains(Event.current.mousePosition) ) {
                            if ( Event.current.type == EventType.DragUpdated ) {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                Event.current.Use();
                            }
                            if ( Event.current.type == EventType.DragPerform ) {
                                for ( var i = 0; i < dropRefs.Length; i++ ) {
                                    var dropRef = dropRefs[i];
                                    var dropRefType = dropRef.GetType();
                                    if ( argType.IsAssignableFrom(dropRefType) ) {
                                        if ( unityObject != null ) { Undo.RecordObject(unityObject, "Drag Add Item"); }
                                        list.Add(dropRef);
                                        continue;
                                    }
                                    if ( dropRefType == typeof(GameObject) && typeof(Component).IsAssignableFrom(argType) ) {
                                        var componentToAdd = ( dropRef as GameObject ).GetComponent(argType);
                                        if ( componentToAdd != null ) {
                                            if ( unityObject != null ) { Undo.RecordObject(unityObject, "Drag Add Item"); }
                                            list.Add(componentToAdd);
                                        }
                                        continue;
                                    }
                                }
                                Event.current.Use();
                            }
                        }
                    }
                }

                //Add new default element
                if ( dropRefs.Length == 0 ) {
                    if ( GUILayout.Button("Add Element") ) {
                        if ( unityObject != null ) { Undo.RecordObject(unityObject, "Add Item"); }
                        var o = argType.IsValueType ? argType.CreateObjectUninitialized() : null;
                        if ( listType.IsArray ) {
                            list = ReflectionTools.Resize((System.Array)list, list.Count + 1);
                        } else {
                            list.Add(o);
                        }
                        GUI.changed = true;
                        if ( unityObject != null ) { EditorUtility.SetDirty(unityObject); }
                        registeredEditorFoldouts[list] = true;
                        return list;
                    }
                }

            }

            if ( list.Count == 0 ) {
                return list;
            }

            if ( !pickedListIndex.ContainsKey(list) ) {
                pickedListIndex[list] = -1;
            }

            var e = Event.current;
            var pickedIndex = pickedListIndex[list];
            var handleStyle = new GUIStyle("label");
            handleStyle.alignment = TextAnchor.MiddleCenter;

            for ( var i = 0; i < list.Count; i++ ) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.BeginVertical();
                GUICallback(i, pickedIndex == i);
                GUILayout.EndVertical();
                var lastRect = GUILayoutUtility.GetLastRect();
                var pickRect = Rect.MinMaxRect(lastRect.xMin - 16, lastRect.yMin, lastRect.xMin, lastRect.yMax);
                GUI.color = new Color(1, 1, 1, 0.5f);
                GUI.Label(pickRect, "☰", handleStyle);
                GUI.color = Color.white;
                if ( options.CustomItemMenu != null ) {
                    GUILayout.Space(18);
                    var buttonRect = Rect.MinMaxRect(lastRect.xMax, lastRect.yMin, lastRect.xMax + 22, lastRect.yMax + 1);
                    EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
                    GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.grey;
                    if ( GUI.Button(buttonRect, Icons.gearPopupIcon, Styles.centerLabel) ) {
                        if ( unityObject != null ) { Undo.RecordObject(unityObject, "Menu Item"); }
                        options.CustomItemMenu(i).ShowAsContext();
                        GUI.changed = true;
                        if ( unityObject != null ) { EditorUtility.SetDirty(unityObject); }
                    }
                    GUI.color = Color.white;
                }
                if ( options.allowRemove ) {
                    GUILayout.Space(18);
                    var buttonRect = Rect.MinMaxRect(lastRect.xMax, lastRect.yMin, lastRect.xMax + 16, lastRect.yMax);
                    if ( GUI.Button(buttonRect, "X") ) {
                        if ( unityObject != null ) { Undo.RecordObject(unityObject, "Remove Item"); }
                        if ( listType.IsArray ) {
                            list = ReflectionTools.Resize((System.Array)list, list.Count - 1);
                            registeredEditorFoldouts[list] = true;
                        } else {
                            list.RemoveAt(i);
                        }
                        GUI.changed = true;
                        if ( unityObject != null ) { EditorUtility.SetDirty(unityObject); }
                    }
                }
                GUILayout.EndHorizontal();

                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                EditorGUIUtility.AddCursorRect(pickRect, MouseCursor.MoveArrow);
                var boundRect = GUILayoutUtility.GetLastRect();

                if ( pickRect.Contains(e.mousePosition) && e.type == EventType.MouseDown ) {
                    pickedListIndex[list] = i;
                }

                if ( pickedIndex == i ) {
                    GUI.Box(boundRect, string.Empty);
                }

                if ( pickedIndex != -1 && pickedIndex != i && boundRect.Contains(e.mousePosition) ) {

                    var markRect = new Rect(boundRect.x, boundRect.y - 2, boundRect.width, 2);
                    if ( pickedIndex < i ) {
                        markRect.y = boundRect.yMax - 2;
                    }

                    GUI.DrawTexture(markRect, Texture2D.whiteTexture);
                    if ( e.type == EventType.MouseUp ) {
                        if ( unityObject != null ) { Undo.RecordObject(unityObject, "Reorder Item"); }
                        var pickObj = list[pickedIndex];
                        list.RemoveAt(pickedIndex);
                        list.Insert(i, pickObj);
                        GUI.changed = true;
                        if ( unityObject != null ) { EditorUtility.SetDirty(unityObject); }
                    }
                }
            }

            if ( e.rawType == EventType.MouseUp ) {
                pickedListIndex[list] = -1;
            }

            return list;
        }
    }
}

#endif