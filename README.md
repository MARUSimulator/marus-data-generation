To install **MARUS2 Data Generation** and its required core dependencies, add them directly to your Unity project's **`Packages/manifest.json`** file under the `"dependencies"` block:

```json
{
  "dependencies": {
    "com.marus2.proto": "https://github.com/MARUSimulator/marus2-proto.git#csharp",
    "com.marus2.core": "https://github.com/MARUSimulator/marus2-core.git",
    "com.marus2.sensors": "https://github.com/MARUSimulator/marus-sensors.git",
    "com.marus2.data-generation": "https://github.com/MARUSimulator/marus-data-generation.git"
  }
}
```

# Data generation usage

This package contains tools for synthetic sensor data collection, automatic labeling, and dataset generation in Unity. It is designed to create structured datasets tailored for training computer vision, acoustic perception, and 3D point cloud object detection models.

Datasets are saved to designated output folders with synchronized raw sensor files (images, point clouds) and corresponding ground truth annotation files (such as YOLO bounding boxes or per-point labels).

## Camera Object Detection Saver

Captures RGB images from one or more Unity cameras and automatically computes 2D bounding boxes in YOLO format for tracked GameObjects. Features include occlusion testing, minimum pixel visibility thresholds, configurable save rates, and automatic dataset folder organization.

## Sonar Object Detection Saver

Attaches to a `Sonar3D` sensor to record simulated 2D polar and Cartesian acoustic sonar images while generating bounding box annotations around detected underwater objects. It applies acoustic intensity thresholding to identify target signatures in the sonar imagery.

## Point Cloud Segmentation Saver

Attaches to a `RaycastLidar` sensor to capture 3D point clouds in `.pcd` format along with per-point semantic segmentation labels for specified object classes. Useful for training 3D point cloud semantic segmentation and object detection models.

## Object Bounding Box Visualizer

Visualizes 3D oriented bounding boxes and 2D projected screen-space bounding boxes directly in the Scene and Game view with optional bounding box noise simulation. Serves as an interactive debugging tool to verify annotations in real time.