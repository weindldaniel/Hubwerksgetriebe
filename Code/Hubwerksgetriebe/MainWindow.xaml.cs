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
        private float _rotation = 0;

        public MainWindow()
        {
            InitializeComponent();
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

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(0, 0, -25);
            //gl.Rotate(_rotation, 0, 1, 0);
            gl.Rotate(-30,0,1,0);
            gl.Rotate(-10,0,0,1);
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
                10, 10,              // Screen-Koordinaten (px)
                0.0f, 0.0f, 0.0f,    // Farbe
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

            gl.Enable(OpenGL.GL_LIGHTING);
            // ---------------- Rollenparameter ----------------
            double r2 = 1;
            double r32 = 3;
            double r34 = 1.5;
            double r43 = 2.5;
            double r45 = 1.5;
            
            double thickness = 1;
            int segments = 60;
            double x2 = 0;
            double x3 = 4; 
            double x4 = 8;

            double z3 = 1;
            double z45 = 2;
            // ---------------- Rolle 1 ----------------
            SetMaterial(gl, 0.6f, 0.6f, 0.6f);
            DrawDiskXY(gl, r2, thickness, segments, x2, 0, 0);
        
            // ---------------- Rolle 2 ----------------
            SetMaterial(gl, 0.4f, 0.6f, 0.9f);
            DrawDiskXY(gl, r32, thickness, segments, x3, 0, 0);
            DrawDiskXY(gl, r34, thickness, segments, x3, 0, z3);
            
            // ---------------- Rolle 3 ----------------
            SetMaterial(gl, 0.7f, 0.4f, 0.4f);
            DrawDiskXY(gl, r43, thickness, segments, x4, 0, z3);
            DrawDiskXY(gl, r45, thickness, segments, x4, 0, z45);

            // ---------------- Seil ----------------
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Color(0, 0, 0);
            gl.LineWidth(3);

            double ropeTopY = 0;
            double ropeBottomY = -4.5;

            // gl.Begin(OpenGL.GL_LINES);
            // gl.Vertex(9.5, ropeTopY, 0);
            // gl.Vertex(9.5, ropeBottomY, 0);
            // gl.End();

            double ropeZ = 2;   // Verschiebung in Z-Richtung

            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(9.5, ropeTopY, ropeZ);
            gl.Vertex(9.5, ropeBottomY, ropeZ);
            gl.End();
            
            
            gl.Enable(OpenGL.GL_LIGHTING);

            // ---------------- Punktmasse ----------------
            SetMaterial(gl, 0, 0, 0);
            DrawSphere(gl, 0.5, 30, 30, 9.5, ropeBottomY - 0.4, 2);

            _rotation += 0.5f;
        }

        // ================= Hilfsmethoden =================

        private void DrawDiskXY(OpenGL gl, double radius, double thickness, int segments,
            double cx, double cy, double cz)
        {
            double zFront = cz + thickness / 2.0;
            double zBack  = cz - thickness / 2.0;

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
        
        private void DrawSphere(OpenGL gl, double radius, int slices, int stacks, double cx, double cy, double cz)
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

        private void SetMaterial(OpenGL gl, float r, float g, float b)
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
    }
}    