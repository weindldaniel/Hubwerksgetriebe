using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL;

namespace Hubwerksgetriebe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region PRIVATE VARIABLES
        private float _rotation = 0;
        // Rollenparameter
        private double _r2 = 1;
        private double _r32 = 3;
        private double _r34 = 1.5;
        private double _r43 = 2.5;
        private double _r45 = 1.5;
        private double _thickness = 1;
        private int _segments = 60;
        // X & Z Positionen
        private double _x2 = 0;
        private double _x3 = 4;
        private double _x4 = 8;
        private double _z3 = 1;
        private double _z45 = 2;
        // Winkel
        private double _phi2 = 0.0; // [rad]

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        public enum DiskMarkerColor
        {
            Red,
            Green,
            Blue
        }

        public enum DiskMarkerDirection
        {
            PositiveX,
            NegativeX
        }
        
        private void OpenGLControl_OpenGLInitialized(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.ClearColor(1, 1, 1, 1);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_NORMALIZE);

            float[] ambient = { 0.2f, 0.2f, 0.2f, 1 };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, ambient);

            float[] diffuse = { 1, 1, 1, 1 };
            float[] specular = { 1, 1, 1, 1 };
            float[] position = { 3, 4, 6, 1 };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, specular);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, position);
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            double phi3 = _phi2 * (_r2 / _r32);
            double phi4 = phi3 * (_r32 / _r43);
            
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            
            // Kamera ausrichtung
            gl.Translate(0, 0, -25);
            gl.Rotate(20, 1, 0, 0);
            gl.Rotate(-40, 0, 1, 0);
            DrawFadenkreuz(gl);
            //gl.Rotate(_rotation, 0, 1, 0);
            gl.Enable(OpenGL.GL_LIGHTING);
            
            // ---------------- Rolle 1 ----------------
            SetMaterial(gl, 0.6f, 0.6f, 0.6f);
            gl.PushMatrix();
            gl.Translate(_x2, 0, 0);
            gl.Rotate(_phi2 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy_V2(gl,_r2,_thickness,_segments,_x2,0,0,DiskMarkerColor.Red,DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            // ---------------- Rolle 2 ----------------
            SetMaterial(gl, 0.4f, 0.6f, 0.9f);
            
            gl.PushMatrix();
            gl.Translate(0, 0, 0);
            gl.Rotate(phi3 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy_V2(gl,_r32,_thickness,_segments,_x3,0,0,DiskMarkerColor.Red,DiskMarkerDirection.NegativeX);
            DrawDiskXy_V2(gl,_r34,_thickness,_segments,_x3,0,_z3,DiskMarkerColor.Blue,DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            
            // ---------------- Rolle 3 ----------------
            SetMaterial(gl, 0.7f, 0.4f, 0.4f);
            gl.PushMatrix();
            gl.Translate(0, 0, 0);
            gl.Rotate(phi4 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy_V2(gl,_r43,_thickness,_segments,_x4,0,_z3,DiskMarkerColor.Blue,DiskMarkerDirection.NegativeX);
            DrawDiskXy_V2(gl,_r45,_thickness,_segments,_x4,0,_z45,DiskMarkerColor.Green,DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            // ---------------- Seil ----------------
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Color(0, 0, 0);
            gl.LineWidth(3);

            double ropeTopY = 0;
            double ropeBottomY = -4.5;
            
            double ropeZ = 2; // Verschiebung in Z-Richtung

            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(9.5, ropeTopY, ropeZ);
            gl.Vertex(9.5, ropeBottomY, ropeZ);
            gl.End();
            
            gl.Enable(OpenGL.GL_LIGHTING);

            // ---------------- Punktmasse ----------------
            SetMaterial(gl, 0, 0, 0);
            DrawSphere(gl, 0.5, 30, 30, 9.5, ropeBottomY - 0.4, 2);

            _rotation += 0.5f;
            _phi2 += 0.01; // rad pro Frame
        }

        // ================= Hilfsmethoden =================
        private static void DrawDiskXy(OpenGL gl, double radius, double thickness, int segments, double cx, double cy, double cz)
        {
            double zFront = cz + thickness / 2.0;
            double zBack = cz - thickness / 2.0;

            // ---------- Vorderfläche (+Z) ----------
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, 1);
            gl.Vertex(cx, cy, zFront);

            for (int i = 0; i <= segments; i++)
            {
                double t = i * 2.0 * Math.PI / segments;
                double x = radius * Math.Cos(t);
                double y = radius * Math.Sin(t);

                gl.Vertex(cx + x, cy + y, zFront);
            }

            gl.End();

            // ---------- Rückfläche (-Z) ----------
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, -1);
            gl.Vertex(cx, cy, zBack);

            for (int i = 0; i <= segments; i++)
            {
                double t = -i * 2.0 * Math.PI / segments;
                double x = radius * Math.Cos(t);
                double y = radius * Math.Sin(t);

                gl.Vertex(cx + x, cy + y, zBack);
            }

            gl.End();

            // ---------- Mantel ----------
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= segments; i++)
            {
                double t = i * 2.0 * Math.PI / segments;
                double nx = Math.Cos(t);
                double ny = Math.Sin(t);

                gl.Normal(nx, ny, 0);
                gl.Vertex(cx + radius * nx, cy + radius * ny, zFront);
                gl.Vertex(cx + radius * nx, cy + radius * ny, zBack);
            }
            gl.End();
        }
        private void DrawDiskXy_V2(OpenGL gl, double radius, double thickness, int segments, double cx, double cy, double cz, DiskMarkerColor markerColor, DiskMarkerDirection markerDirection)
        {
            double zFront = cz + thickness / 2.0;
            double zBack  = cz - thickness / 2.0;

            // ================= Vorderfläche (+Z) =================
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, 1);
            gl.Vertex(cx, cy, zFront);

            for (int i = 0; i <= segments; i++)
            {
                double t = i * 2.0 * Math.PI / segments;
                gl.Vertex(
                    cx + radius * Math.Cos(t),
                    cy + radius * Math.Sin(t),
                    zFront
                );
            }
            gl.End();

            // ================= Rückfläche (-Z) =================
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, -1);
            gl.Vertex(cx, cy, zBack);

            for (int i = 0; i <= segments; i++)
            {
                double t = -i * 2.0 * Math.PI / segments;
                gl.Vertex(
                    cx + radius * Math.Cos(t),
                    cy + radius * Math.Sin(t),
                    zBack
                );
            }
            gl.End();

            // ================= Mantel =================
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= segments; i++)
            {
                double t = i * 2.0 * Math.PI / segments;
                double nx = Math.Cos(t);
                double ny = Math.Sin(t);

                gl.Normal(nx, ny, 0);
                gl.Vertex(cx + radius * nx, cy + radius * ny, zFront);
                gl.Vertex(cx + radius * nx, cy + radius * ny, zBack);
            }
            gl.End();

            // ================= Marker (vordere Stirnfläche) =================
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.LineWidth(3.0f);
            SetMarkerColor(gl, markerColor);

            double dir = (markerDirection == DiskMarkerDirection.PositiveX) ? 1.0 : -1.0;
            double zMarker = zFront + 0.001; // gegen Z-Fighting

            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(cx, cy, zMarker);
            gl.Vertex(cx + dir * radius, cy, zMarker);
            gl.End();

            gl.Enable(OpenGL.GL_LIGHTING);
        }
        private void SetMarkerColor(OpenGL gl, DiskMarkerColor color)
        {
            switch (color)
            {
                case DiskMarkerColor.Red:
                    gl.Color(1.0f, 0.0f, 0.0f);
                    break;

                case DiskMarkerColor.Green:
                    gl.Color(0.0f, 1.0f, 0.0f);
                    break;

                case DiskMarkerColor.Blue:
                    gl.Color(0.0f, 0.0f, 1.0f);
                    break;
            }
        }
        private static void DrawSphere(OpenGL gl, double radius, int slices, int stacks, double cx, double cy, double cz)
        {
            for (int j = 0; j < stacks; j++)
            {
                double p1 = j * Math.PI / stacks;
                double p2 = (j + 1) * Math.PI / stacks;

                gl.Begin(OpenGL.GL_QUAD_STRIP);
                for (int i = 0; i <= slices; i++)
                {
                    double t = i * 2 * Math.PI / slices;

                    double x1 = Math.Sin(p1) * Math.Cos(t);
                    double y1 = Math.Cos(p1);
                    double z1 = Math.Sin(p1) * Math.Sin(t);

                    double x2 = Math.Sin(p2) * Math.Cos(t);
                    double y2 = Math.Cos(p2);
                    double z2 = Math.Sin(p2) * Math.Sin(t);

                    gl.Normal(x1, y1, z1);
                    gl.Vertex(cx + radius * x1, cy + radius * y1, cz + radius * z1);

                    gl.Normal(x2, y2, z2);
                    gl.Vertex(cx + radius * x2, cy + radius * y2, cz + radius * z2);
                }

                gl.End();
            }
        }
        private static void SetMaterial(OpenGL gl, float r, float g, float b)
        {
            float[] amb = { 0.2f, 0.2f, 0.2f, 1 };
            float[] diff = { r, g, b, 1 };
            float[] spec = { 1, 1, 1, 1 };
            float[] shin = { 50 };

            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, amb);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, diff);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, spec);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shin);
        }
        private static void DrawFadenkreuz(OpenGL gl)
        {
            // ---------- Fadenkreuz ----------
            gl.Disable(OpenGL.GL_LIGHTING);

            gl.Color(0.0f, 0.0f, 0.0f);
            gl.LineWidth(2.0f);

            gl.Begin(OpenGL.GL_LINES);

            // X-Achse
            gl.Vertex(-10.0f, 0.0f, 0.0f);
            gl.Vertex(10.0f, 0.0f, 0.0f);

            // Y-Achse
            gl.Vertex(0.0f, -10.0f, 0.0f);
            gl.Vertex(0.0f, 10.0f, 0.0f);

            // Z-Achse (in die Tiefe)
            gl.Vertex(0.0f, 0.0f, -10.0f);
            gl.Vertex(0.0f, 0.0f, 10.0f);
            gl.End();

            // ---------- Achsenbeschriftung ----------
            gl.DrawText(
                10, 10, // Screen-Koordinaten (px)
                0.0f, 0.0f, 0.0f, // Farbe
                "Arial",
                12,
                "X →"
            );
            gl.DrawText(
                10, 30,
                0.0f, 0.0f, 0.0f,
                "Arial",
                12,
                "Y ↑"
            );
            gl.DrawText(
                10, 50,
                0.0f, 0.0f, 0.0f,
                "Arial",
                12,
                "Z ⊙"
            );
        }
    }
}    