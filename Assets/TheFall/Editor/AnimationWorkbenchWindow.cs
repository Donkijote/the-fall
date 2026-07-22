using System;
using System.Collections.Generic;
using System.Linq;
using TheFall.Domain;
using TheFall.Presentation.Animation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFall.Editor
{
    /// <summary>
    /// Edit Mode authoring surface for presentation beats. It drives the same controller,
    /// transport, state reconstruction, and path evaluator used by runtime playback.
    /// </summary>
    public sealed class AnimationWorkbenchWindow : EditorWindow
    {
        private const string ScenePath = "Assets/TheFall/Presentation/Scenes/AnimationLab.unity";
        private const string PresetFolder = "Assets/TheFall/Content/Animation";
        private const float MinimumPreviewProgress = 0.0001f;

        private readonly List<AnimationSequenceConfiguration> _presets =
            new List<AnimationSequenceConfiguration>();
        private AnimationLabController _controller;
        private int _scenarioIndex;
        private Seat _seat = Seat.First;
        private AnimationPreviewProfile _profile = AnimationPreviewProfile.Desktop;
        private int _presetIndex;
        private int _selectedStepIndex;
        private double _lastEditorTime;

        [MenuItem("The Fall/Animation Laboratory/Open Workbench", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<AnimationWorkbenchWindow>();
            window.titleContent = new GUIContent("Animation Workbench");
            window.minSize = new Vector2(470f, 620f);
            window.Show();
            window.OpenSceneAndInitialize();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Animation Workbench");
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SceneView.duringSceneGui += OnSceneGui;
            Undo.undoRedoPerformed += OnUndoRedo;
            _lastEditorTime = EditorApplication.timeSinceStartup;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SceneView.duringSceneGui -= OnSceneGui;
            Undo.undoRedoPerformed -= OnUndoRedo;
            _controller?.ClearEditorPreview();
            _controller = null;
        }

        private void OnGUI()
        {
            DrawHeader();
            if (UnityEngine.Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "This window authors presentation beats in Edit Mode. Exit Play Mode to resume the isolated preview.",
                    MessageType.Info);
                return;
            }

            if (!IsPreviewReady())
            {
                EditorGUILayout.HelpBox(
                    "Open the project-owned AnimationLab scene to create and preview beats without entering Play Mode.",
                    MessageType.Info);
                if (GUILayout.Button("Open AnimationLab and Start Edit Mode Preview", GUILayout.Height(32f)))
                {
                    OpenSceneAndInitialize();
                }

                return;
            }

            DrawSourceSelectors();
            DrawTransport();
            DrawSelectedStep();
            DrawDiagnostics();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("EDIT MODE · ANIMATION WORKBENCH", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reopen Scene", EditorStyles.toolbarButton))
                {
                    OpenSceneAndInitialize();
                }
            }
        }

        private void DrawSourceSelectors()
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField("Isolated Animation", EditorStyles.boldLabel);
            var scenarioNames = _controller.AvailableScenarioNames.ToArray();
            var nextScenario = EditorGUILayout.Popup("Animation", _scenarioIndex, scenarioNames);
            var nextSeat = (Seat)EditorGUILayout.EnumPopup("Acting seat", _seat);
            var nextProfile = (AnimationPreviewProfile)EditorGUILayout.EnumPopup("Presentation profile", _profile);

            var presetNames = _presets.Select(preset =>
                $"{preset.PresetName} · v{preset.PresetVersion}").ToArray();
            var nextPreset = presetNames.Length == 0
                ? 0
                : EditorGUILayout.Popup("Presentation preset", _presetIndex, presetNames);

            if (nextScenario != _scenarioIndex)
            {
                _scenarioIndex = nextScenario;
                _controller.SetScenarioIndex(_scenarioIndex);
                SelectStep(0);
            }

            if (nextSeat != _seat)
            {
                _seat = nextSeat;
                _controller.SetActingSeat(_seat);
                SelectStep(Mathf.Min(_selectedStepIndex, _controller.AnimatableStepCount - 1));
            }

            if (nextProfile != _profile)
            {
                _profile = nextProfile;
                _controller.SetPreviewProfile(_profile);
                SelectStep(_selectedStepIndex);
            }

            if (presetNames.Length > 0 && nextPreset != _presetIndex)
            {
                _presetIndex = nextPreset;
                _controller.LoadEditorPreset(_presets[_presetIndex]);
                SelectStep(0);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Preset"))
                {
                    _controller.SaveWorkingPreset();
                }

                if (GUILayout.Button("Save Preset As…"))
                {
                    SavePresetAs();
                }

                if (GUILayout.Button("Reload Asset"))
                {
                    _controller.LoadEditorPreset(_presets[_presetIndex]);
                    SelectStep(_selectedStepIndex);
                }

                if (GUILayout.Button("Frame Preview"))
                {
                    FramePreview();
                }
            }
        }

        private void DrawTransport()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Edit Mode Transport", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("▶ Animation"))
                {
                    _controller.RestartSequence();
                }

                if (GUILayout.Button("Pause"))
                {
                    _controller.Pause();
                }

                if (GUILayout.Button("Reset"))
                {
                    _controller.ResetToStart();
                }
            }

            var normalized = EditorGUILayout.Slider(
                "Animation scrub",
                _controller.NormalizedPosition,
                0f,
                1f);
            if (!Mathf.Approximately(normalized, _controller.NormalizedPosition))
            {
                _controller.Pause();
                _controller.SeekNormalized(normalized);
                _selectedStepIndex = Mathf.Clamp(
                    _controller.CurrentStepIndex,
                    0,
                    Mathf.Max(0, _controller.AnimatableStepCount - 1));
            }

            EditorGUILayout.LabelField(
                "Time",
                $"{_controller.ElapsedSeconds:F3}s / {_controller.DurationSeconds:F3}s");
        }

        private void DrawSelectedStep()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            var steps = _controller.Sequence.Steps;
            if (_controller.AnimatableStepCount == 0)
            {
                return;
            }

            _selectedStepIndex = Mathf.Clamp(
                _selectedStepIndex,
                0,
                _controller.AnimatableStepCount - 1);
            var selectedStep = steps[_selectedStepIndex];
            EditorGUILayout.LabelField("Beat", selectedStep.Kind.ToString());
            EditorGUILayout.LabelField(
                "Resolved event",
                selectedStep.SourceEvent?.Kind.ToString() ?? "none");
            var beat = _controller.WorkingConfiguration.GetBeat(selectedStep.Kind);
            if (beat == null)
            {
                EditorGUILayout.HelpBox("The selected step has no enabled preset beat.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField($"Wireframe · {selectedStep.Kind}", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Green and blue endpoints come from authoritative state. Move the yellow Scene-view handle to author the presentation trajectory between them.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            var duration = EditorGUILayout.Slider("Duration (seconds)", beat.DurationSeconds, 0f, 3f);
            var delay = EditorGUILayout.Slider("Delay (seconds)", beat.DelaySeconds, 0f, 2f);
            var easing = (AnimationBeatEasing)EditorGUILayout.EnumPopup("Easing", beat.Easing);
            var trajectory = EditorGUILayout.Vector3Field("Trajectory offset (m)", beat.TrajectoryOffset);
            var emphasis = EditorGUILayout.Slider("Emphasis", beat.Emphasis, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_controller.WorkingConfiguration, "Tune presentation beat");
                beat.SetTiming(duration, delay);
                beat.SetVisuals(easing, trajectory, emphasis);
                EditorUtility.SetDirty(_controller.WorkingConfiguration);
                _controller.RefreshEditorPreview();
                SelectStep(_selectedStepIndex, Mathf.Max(MinimumPreviewProgress, _controller.ActiveStepProgress));
            }

            var beatProgress = EditorGUILayout.Slider(
                "Selected beat scrub",
                _controller.CurrentStepIndex == _selectedStepIndex
                    ? _controller.ActiveStepProgress
                    : 0f,
                0f,
                1f);
            if (_controller.CurrentStepIndex != _selectedStepIndex ||
                !Mathf.Approximately(beatProgress, _controller.ActiveStepProgress))
            {
                SelectStep(_selectedStepIndex, Mathf.Max(MinimumPreviewProgress, beatProgress));
            }
        }

        private void DrawDiagnostics()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Diagnosis", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Active beat", _controller.ActiveStep?.Kind.ToString() ?? "none");
            EditorGUILayout.LabelField(
                "Rendered ↔ authoritative",
                _controller.IsRenderedStateSynchronized ? "AGREE" : "IN FLIGHT");
            EditorGUILayout.LabelField(
                "Source event",
                _controller.ActiveStep?.SourceEvent?.Kind.ToString() ?? "none");
        }

        private void OnEditorUpdate()
        {
            var now = EditorApplication.timeSinceStartup;
            var delta = Mathf.Clamp((float)(now - _lastEditorTime), 0f, 0.1f);
            _lastEditorTime = now;
            if (!IsPreviewReady() || !_controller.TickEditorPreview(delta))
            {
                return;
            }

            Repaint();
            SceneView.RepaintAll();
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (!IsPreviewReady() ||
                _selectedStepIndex < 0 ||
                _selectedStepIndex >= _controller.AnimatableStepCount ||
                _controller.CurrentStepIndex != _selectedStepIndex ||
                !_controller.TryGetPrimaryMotion(out var motion))
            {
                return;
            }

            var step = _controller.Sequence.Steps[_selectedStepIndex];
            var beat = _controller.WorkingConfiguration.GetBeat(step.Kind);
            if (beat == null || motion.PresentationRoot == null)
            {
                return;
            }

            var trajectoryWorld = motion.PresentationRoot.TransformVector(beat.TrajectoryOffset);
            var midpoint = Vector3.Lerp(motion.StartWorld, motion.TargetWorld, 0.5f);
            var control = midpoint + trajectoryWorld;

            Handles.color = new Color(0.95f, 0.75f, 0.25f, 1f);
            var points = new Vector3[25];
            for (var index = 0; index < points.Length; index++)
            {
                var progress = index / (float)(points.Length - 1);
                points[index] = AnimationBeatEvaluator.EvaluatePosition(
                    motion.StartWorld,
                    motion.TargetWorld,
                    progress,
                    beat.Easing,
                    trajectoryWorld);
            }

            Handles.DrawAAPolyLine(4f, points);
            Handles.color = new Color(0.3f, 0.9f, 0.45f, 1f);
            Handles.SphereHandleCap(
                0,
                motion.StartWorld,
                Quaternion.identity,
                HandleUtility.GetHandleSize(motion.StartWorld) * 0.08f,
                EventType.Repaint);
            Handles.Label(motion.StartWorld, "Authoritative start");
            Handles.color = new Color(0.35f, 0.65f, 1f, 1f);
            Handles.SphereHandleCap(
                0,
                motion.TargetWorld,
                Quaternion.identity,
                HandleUtility.GetHandleSize(motion.TargetWorld) * 0.08f,
                EventType.Repaint);
            Handles.Label(motion.TargetWorld, "Authoritative end");
            Handles.color = new Color(1f, 0.75f, 0.2f, 1f);
            Handles.DrawDottedLine(midpoint, control, 4f);

            EditorGUI.BeginChangeCheck();
            var nextControl = Handles.PositionHandle(control, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_controller.WorkingConfiguration, "Edit animation trajectory");
                var nextOffsetWorld = nextControl - midpoint;
                var nextOffsetLocal = motion.PresentationRoot.InverseTransformVector(nextOffsetWorld);
                beat.SetVisuals(beat.Easing, nextOffsetLocal, beat.Emphasis);
                EditorUtility.SetDirty(_controller.WorkingConfiguration);
                _controller.RefreshEditorPreview();
                SelectStep(_selectedStepIndex, Mathf.Max(MinimumPreviewProgress, _controller.ActiveStepProgress));
                Repaint();
            }
        }

        private void OpenSceneAndInitialize()
        {
            if (UnityEngine.Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Edit Mode required",
                    "Exit Play Mode before opening the Animation Workbench.",
                    "OK");
                return;
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            _controller = FindAnyObjectByType<AnimationLabController>();
            if (_controller == null)
            {
                EditorUtility.DisplayDialog(
                    "AnimationLab is incomplete",
                    "Run The Fall > Animation Laboratory > Generate, then reopen the workbench.",
                    "OK");
                return;
            }

            RefreshPresetList();
            var preset = _presets.Count > 0 ? _presets[_presetIndex] : _controller.Configuration;
            _controller.BeginEditorWorkbenchPreview(_scenarioIndex, _seat, _profile, preset);
            SelectStep(0);
            _lastEditorTime = EditorApplication.timeSinceStartup;
            FramePreview();
            Repaint();
        }

        private void RefreshPresetList()
        {
            _presets.Clear();
            foreach (var guid in AssetDatabase.FindAssets(
                "t:AnimationSequenceConfiguration",
                new[] { PresetFolder }))
            {
                var preset = AssetDatabase.LoadAssetAtPath<AnimationSequenceConfiguration>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (preset != null)
                {
                    _presets.Add(preset);
                }
            }

            _presets.Sort((left, right) =>
                string.Compare(left.PresetName, right.PresetName, StringComparison.Ordinal));
            if (_controller != null && _controller.WorkingConfiguration == null)
            {
                _presetIndex = Mathf.Max(0, _presets.IndexOf(_controller.Configuration));
            }
            else
            {
                _presetIndex = Mathf.Clamp(_presetIndex, 0, Mathf.Max(0, _presets.Count - 1));
            }
        }

        private void SelectStep(int stepIndex, float progress = 0.5f)
        {
            if (!IsPreviewReady() || _controller.AnimatableStepCount == 0)
            {
                return;
            }

            _selectedStepIndex = Mathf.Clamp(stepIndex, 0, _controller.AnimatableStepCount - 1);
            _controller.SeekToStep(
                _selectedStepIndex,
                Mathf.Clamp(progress, MinimumPreviewProgress, 1f));
            SceneView.RepaintAll();
            Repaint();
        }

        private void SavePresetAs()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Animation Preset",
                "AnimationSequencePreset",
                "asset",
                "Choose a project-owned location for the version-controlled presentation preset.",
                PresetFolder);
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var preset = Instantiate(_controller.WorkingConfiguration);
            preset.hideFlags = HideFlags.None;
            preset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            preset.SetPresetIdentity(ObjectNames.NicifyVariableName(preset.name));
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            RefreshPresetList();
            _presetIndex = _presets.IndexOf(preset);
            _controller.LoadEditorPreset(preset);
            SelectStep(_selectedStepIndex);
        }

        private void FramePreview()
        {
            if (_controller?.PreviewRoot == null || SceneView.lastActiveSceneView == null)
            {
                return;
            }

            Selection.activeTransform = _controller.PreviewRoot;
            SceneView.lastActiveSceneView.FrameSelected();
        }

        private void OnUndoRedo()
        {
            if (!IsPreviewReady())
            {
                return;
            }

            _controller.RefreshEditorPreview();
            SelectStep(_selectedStepIndex);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                _controller?.ClearEditorPreview();
            }
            else if (state == PlayModeStateChange.EnteredEditMode &&
                SceneManager.GetActiveScene().path == ScenePath)
            {
                OpenSceneAndInitialize();
            }
        }

        private bool IsPreviewReady()
        {
            return !UnityEngine.Application.isPlaying &&
                _controller != null &&
                _controller.WorkingConfiguration != null &&
                _controller.Sequence != null;
        }
    }
}
