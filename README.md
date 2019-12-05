# CustomizedTrailRenderer
Unity's default trail renderer doesn't produce smooth trails when the object is moving in high speed. This customized utilizes catmule-spline algorithm and produce better looking trails when the gameobject is attached with it.

[Catmull-Rom Spline algorithm wiki](https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline "Catmull-Rom Spline")

It needs exactly 4 control points to work. For every new control point, we interpolate and add 3 display points in between control points.
The life time of each display point as well as width and minimum distance in between control points are adjustable through inspector panel.

Below is the video for you to preview how it is like in motion
[![Trail Renderer in motion](https://img.youtube.com/vi/-f0fGU6tJG8/0.jpg)](https://www.youtube.com/watch?v=-f0fGU6tJG8)

# Version of Unity 

Unity version was 2019.2.14f1 when last pushed


