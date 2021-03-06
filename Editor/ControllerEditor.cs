﻿using System;
using System.Linq;
using Cubeage.Avatar.Editor.Util;
using UnityEditor;
using UnityEngine;

namespace Cubeage.Avatar.Editor
{
    [CustomEditor(typeof(AvatarController))]
    public class ControllerEditor : UnityEditor.Editor
    {
        private AvatarController _avatarController;

        private void OnEnable()
        {
            _avatarController = (AvatarController)target;
        }

        private bool DrawRemoveButton()
        {
            return GUILayout.Button("x", new GUIStyle(GUI.skin.label)
            {
                fixedWidth = EditorGUIUtility.singleLineHeight,
                hover = new GUIStyleState()
                {
                    textColor = Color.red
                }
            }) && ConfirmRemove();
        }

        private void RecordUndo(string name)
        {
            Undo.RecordObjects(_avatarController.Manager.Handlers.Where(x => x.IsValid()).Select(x => x.Transform).Cast<UnityEngine.Object>().Prepend(_avatarController).ToArray(), name);
        }

        private bool ConfirmRemove()
        {
            return Confirm("Are you sure want to remove?");
        }

        private void DrawHierarchy(TransformHandler handler = null)
        {
            var handlers = handler?.VirtualChildren ?? _avatarController.Manager.Handlers.Where(x => x.VirtualParent == null);
            if (handlers.Count() > 0)
            {
                using (Layout.Indent())
                {
                    foreach (var child in handlers)
                    {
                        using (Layout.Horizontal())
                        {
                            Layout.Foldout(child.IsExpanded)
                                .OnChanged(x =>
                                {
                                    child.IsExpanded = x;
                                });
                            Layout.ObjectLabel(child.Transform);
                            if (child.TryGetTargetTransform(out var target))
                            {
                                Layout.ObjectLabel(target);
                            }
                            else
                            {
                                using (Layout.Color(Color.red))
                                {
                                    Layout.ObjectLabel<GameObject>(null);
                                }
                            }
                        }

                        if (child.IsExpanded)
                        {
                            DrawHierarchy(child);
                            using (Layout.Horizontal())
                            {
                                Layout.Space(32);
                                Layout.Object<Transform>(null).OnChanged(x =>
                                {
                                    if (x != null)
                                    {
                                        try
                                        {
                                            RecordUndo("Add Virtual Child");
                                            child.AddVirtualChild(x);
                                        }
                                        catch (Exception e)
                                        {
                                            Alert(e.Message);
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            using (Layout.Horizontal())
            {
                Layout.Label("Enable");
                Layout.Toggle(_avatarController.IsEnabled)
                    .OnChanged(x =>
                    {
                        RecordUndo("Toggle Avatar Controller");
                        _avatarController.IsEnabled = x;
                    });
            }
            /*
            using (Layout.Horizontal())
            {
                Layout.Label("Target Avatar");
                Layout.Object(_avatarController.Manager.Root)
                    .OnChanged(x =>
                    {
                        _avatarController.Manager.Root = x;
                    });
            }
            */
            /*
            int pickerID = 455454425;
            if (GUILayout.Button("Select"))
            {
                EditorGUIUtility.ShowObjectPicker<Transform>(null, true, "_bc", pickerID);

            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == pickerID)
                {
                    var transform  = EditorGUIUtility.GetObjectPickerObject();
                    Debug.Log(transform);
                }
            }
            */
            using (Layout.Horizontal())
            using (Layout.Box())
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(_avatarController.Manager.IsExpanded).OnChanged(x => _avatarController.Manager.IsExpanded = x);
                    Layout.Label($"Bone Hierarchy: {_avatarController.Manager.Handlers.Count}");
                    Layout.FlexibleSpace();
                    if (Layout.Button("Auto"))
                    {
                        RecordUndo("Auto Set Hierarchy");
                        _avatarController.Manager.AutoSetParent();
                    }
                    if (Layout.Button("Reload"))
                    {
                        RecordUndo("Reload Hierarchy");
                        _avatarController.Manager.Reload();
                    }
                }

                if (_avatarController.Manager.IsExpanded)
                    DrawHierarchy();
            }

            using (Layout.Toolbar())
            {
                if (Layout.ToolbarButton("Add Controller"))
                {
                    RecordUndo("Add Controller");
                    _avatarController.AddController();
                }
                if (Layout.ToolbarButton("Set All To Default"))
                {
                    RecordUndo("Set All Controllers To Default");
                    _avatarController.SetToDefault();
                }
                Layout.FlexibleSpace();
                // if (Layout.ToolbarButton("測試"))
                // {
                    // foreach(var x  in controller.BoneControllers[0].Bones[0].Properties)
                    // {
                    //     Debug.Log(x.Key, x.Value);
                    // }
                    // Animator lAnimator = controller.gameObject.GetComponent<Animator>();
                    // Debug.Log(lAnimator);
                    // 
                    // Transform lBoneTransform = lAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    // 
                    // Debug.Log(lBoneTransform);
                // }
            }

            
            foreach (var controller in _avatarController.Controllers)
            {
                using (Layout.Horizontal())
                {
                    Layout.Foldout(controller.IsExpanded)
                          .OnChanged(x => {
                              RecordUndo("Expand Controller");
                              controller.IsExpanded = x;
                          });
                    Layout.Toggle(controller.IsEnabled)
                            .OnChanged(x =>
                            {
                                RecordUndo("Toggle Controller");
                                controller.IsEnabled = x;
                            });
                    Layout.Text(controller.Name)
                            .OnChanged(x =>
                            {
                                RecordUndo("Change Controller Name");
                                controller.Name = x;
                            });
                    Layout.Slider(controller.Value, 0, 100)
                        .OnChanged(x =>
                        {
                            RecordUndo("Slide Controller");
                            controller.Value = x;
                        });
                    /*
                    if (DrawRemoveButton())
                    {
                        AddUndo("Remove Bone");
                        controller.BoneControllers.Remove(currentController);
                        continue;
                    }
                    */
                }


                if (controller.IsExpanded)
                {
                    using (Layout.Indent())
                    using (Layout.Box())
                    {
                        using (Layout.Toolbar())
                        {
                            if (Layout.ToolbarButton("Reset"))
                            {
                                RecordUndo("Reset Controller");
                                controller.SetToDefault();
                            }
                            if (Layout.ToolbarButton("Set Default"))
                            {
                                RecordUndo("Set Default");
                                controller.SetDefault();
                            }
                            Layout.FlexibleSpace();

                            if (Layout.ToolbarButton("Remove") && ConfirmRemove())
                            {
                                RecordUndo("Remove Controller");
                                _avatarController.Remove(controller);
                                GUIUtility.ExitGUI();
                            }
                        }
                        Layout.EnumToolbar(controller.Mode).OnChanged(x =>
                        {
                            RecordUndo("Change Mode");
                            controller.Mode = x;
                        });


                        Layout.Label($"Bones ({controller.BoneControllers.Count})");
                        foreach (var bone in controller.BoneControllers)
                        {
                            using (Layout.Horizontal())
                            {
                                Layout.Foldout(bone.IsExpanded)
                                    .OnChanged(x =>
                                    {
                                        RecordUndo("Expand Transform Controller");
                                        bone.IsExpanded = x;
                                    });

                                Layout.Toggle(bone.IsEnabled)
                                        .OnChanged(x =>
                                        {
                                            RecordUndo("Toggle Transform Controller");
                                            bone.IsEnabled = x;
                                        });
                                Layout.Object(bone.Transform).OnChanged(x =>
                                {
                                    RecordUndo("Set Tarnsform Target");
                                    bone.Transform = x;
                                });
                                if (Layout.MiniButton("Remove") && ConfirmRemove())
                                {
                                    RecordUndo("Remove Tarnsform Controller");
                                    controller.Remove(bone);
                                    GUIUtility.ExitGUI();
                                }
                                /*
                                if (DrawRemoveButton())
                                {
                                    AddUndo("Remove Bone");
                                    currentController.Remove(bone);
                                    continue;
                                }
                                */
                            }

                            if (bone.IsExpanded)
                            {
                                using (Layout.Indent())
                                {
                                    using (Layout.Horizontal())
                                    {
                                        Layout.Label("Transform Children", GUILayout.MinWidth(50));
                                        Layout.FlexibleSpace();
                                        Layout.Toggle(bone.TransformChildren)
                                                .OnChanged(x =>
                                                {
                                                    RecordUndo("Toggle Transform Children");
                                                    bone.TransformChildren = x;
                                                });
                                    }

                                    foreach (var type in EnumHelper.GetValues<TransformType>())
                                    {
                                        using (Layout.Horizontal())
                                        {
                                            Layout.Label(type.ToString(), GUILayout.MinWidth(50));
                                            using (Layout.SetLabelWidth(10))
                                            {
                                                foreach (var direction in EnumHelper.GetValues<Dimension>())
                                                {
                                                    DrawTransformController(bone, new Property(type, direction), controller);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (Layout.Horizontal())
                        {
                            Layout.Space(32);
                            Layout.Object<Transform>(null).OnChanged(x =>
                            {
                                if (x != null)
                                {
                                    try
                                    {
                                        RecordUndo("Add Transform Controller");
                                        controller.Add(x);
                                    }
                                    catch (Exception e)
                                    {
                                        Alert(e.Message);
                                    }
                                }
                            });
                        }
                    }
                }
            }

        }


        private bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Yes", "No");
        }

        private void Alert(string message)
        {
            EditorUtility.DisplayDialog("Controller @ Cubeage", message, "Okay");
        }

        private void DrawTransformController(TransformController bone, Property property, Controller boneController)
        {
            var entry = bone.Properties[property];
            using (Layout.SetEnable(entry.IsEnabled || bone.IsAvailable(property)))
            {
                Layout.Toggle(entry.IsEnabled)
                        .OnChanged(x =>
                        {
                            RecordUndo("Toggle Property");
                            entry.IsEnabled = x;
                        });
            }

            using (Layout.SetEnable(boneController.Mode != Mode.View && entry.IsEnabled))
            {
                float? minValue = null;
                if (property.Type == TransformType.Scale)
                    minValue = 0.01f;
                var value = entry.Value;
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        value = entry.Min;
                        break;
                    case Mode.Max:
                        value = entry.Max;
                        break;
                }
                var floatField = Layout.Float(value, property.Dimension.ToString(), minValue, GUILayout.MinWidth(20));
                switch (boneController.Mode)
                {
                    case Mode.Min:
                        floatField.OnChanged(x =>
                            {
                                RecordUndo("Set Min");
                                entry.Min = x;
                            });
                        break;
                    case Mode.Max:
                        floatField.OnChanged(x =>
                            {
                                RecordUndo("Set Max");
                                entry.Max = x;
                            });
                        break;
                }
            }
        }


    }
}
