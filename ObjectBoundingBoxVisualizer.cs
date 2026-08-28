// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Marus.ObjectAnnotation;
using Marus.NoiseDistributions;

// Wrap editor-only imports
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Marus.ObjectAnnotation // Changed from Marus.Visualization to match its new home
{
    [DefaultExecutionOrder(100)]
    public class ObjectBoundingBoxVisualizer : MonoBehaviour
    {
        public CameraObjectDetectionSaver Annotator;
        public NoiseParameters boundingBoxNoise;
        public int VertexStep = 20;

        private Dictionary<int, GameObject> canvasMap;
        private List<GameObject> boundingBoxList;

        private List<Camera> Cameras;
        private List<ObjectRecord> Objects;
        private List<(int, string)> _classes;

        void Setup()
        {
            if (Annotator is null) return;

            Cameras = Annotator.CameraViews;
            Objects = Annotator.ObjectsToTrack;
            _classes = Annotator._classList;

            boundingBoxList = new List<GameObject>();
            canvasMap = new Dictionary<int, GameObject>();

            foreach (var c in Cameras)
            {
                GameObject canvasGO = new GameObject();
                canvasGO.name = "VisualizationCanvas";
                canvasGO.AddComponent<Canvas>();

                Canvas myCanvas = canvasGO.GetComponent<Canvas>();
                myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                myCanvas.targetDisplay = c.targetDisplay;
                canvasGO.hideFlags = HideFlags.HideInHierarchy;
                canvasMap.Add(myCanvas.targetDisplay, canvasGO);
            }
        }

        void Start()
        {
            Setup();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (Annotator is null || boundingBoxList is null)
            {
                return;
            }

            if (Objects is null)
            {
                Setup();
            }

            foreach (GameObject go in boundingBoxList)
            {
                Destroy(go);
            }
            boundingBoxList.Clear();

            foreach (ObjectRecord go in Objects)
            {
                foreach (Camera c in Cameras)
                {
                    Rect boundingBox = new Rect();
                    try
                    {
                        boundingBox = CameraObjectDetectionSaver.GetBoundingBoxFromMesh(go.Object, c);
                    }
                    catch
                    {
                        continue;
                    }
                    VisualizeObjectBounds(go.Object, boundingBox, c, _classes[go.ClassIndex].Item2);
                }
            }
        }

        private void VisualizeObjectBounds(GameObject obj, Rect bounds, Camera CameraView, string className="")
        {
            if ((bounds.width * bounds.height) < 8000) return;

            var ld = new Vector3(bounds.center.x - bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y - bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var dd = new Vector3(bounds.center.x + bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y - bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var lg = new Vector3(bounds.center.x - bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y + bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);
            var dg = new Vector3(bounds.center.x + bounds.width/2f + Noise.Sample(boundingBoxNoise), bounds.center.y + bounds.height/2f + Noise.Sample(boundingBoxNoise), 0);

            Gizmos.color = Color.red;

            if (className == "")
            {
                className = obj.name;
            }

            var canvas = canvasMap[CameraView.targetDisplay].GetComponent<Canvas>();
            ScreenGizmos.DrawLine(canvas, CameraView, ld, dd);
            ScreenGizmos.DrawLine(canvas, CameraView, ld, lg);
            ScreenGizmos.DrawLine(canvas, CameraView, dd, dg);
            ScreenGizmos.DrawLine(canvas, CameraView, lg, dg);

// Safely wrap the Editor GUI drawing code
#if UNITY_EDITOR
            var pixelRatio = HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.right).x - HandleUtility.GUIPointToScreenPixelCoordinate(Vector2.zero).x;
            Handles.BeginGUI();
            Handles.color = Color.red;
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)15,
                normal = new GUIStyleState() { textColor = Color.white }
            };

            Vector2 size = style.CalcSize(new GUIContent(name));
            var pos = HandleUtility.WorldToGUIPoint(obj.transform.position) + new Vector2(100, -50);
            var scr = bounds.center;
            Vector2 convertedGUIPos = GUIUtility.ScreenToGUIPoint(scr);

            GUI.Label(new Rect(lg.x, Screen.height - lg.y - 50, bounds.width, 50), className, style);
            Handles.EndGUI();
#endif
        }
    }

    public static class ScreenGizmos
    {
        private const float offset = 0.001f;

        public static void DrawLine(
            Canvas canvas,
            Camera camera,
            Vector3 startPixelPos,
            Vector3 endPixelPos)
        {
            if (camera == null || canvas == null)
                return;

            Vector3 startWorld = PixelToCameraClipPlane(
                camera,
                canvas,
                startPixelPos);

            Vector3 endWorld = PixelToCameraClipPlane(
                camera,
                canvas,
                endPixelPos);

            Gizmos.DrawLine(startWorld, endWorld);
        }

        private static Vector3 PixelToCameraClipPlane(
            Camera camera,
            Canvas canvas,
            Vector3 screenPos)
        {
            screenPos *= canvas.scaleFactor;
            screenPos.z = camera.nearClipPlane + offset;
            return camera.ScreenToWorldPoint(screenPos);
        }
    }
}