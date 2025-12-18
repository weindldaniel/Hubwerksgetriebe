using SharpGL;
using SharpGL.WPF;
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


namespace Hubwerksgetriebe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ----------------- Animation -----------------
        private double phi2 = 0.0;

        // ----------------- Geometrie (Skaliert) -----------------
        double r2 = 0.6;   // Rad 2
        double r32 = 1.2;   // Rad 3
        double r43 = 1.0;   // Rad 4
        double r45 = 1.0;   // Seiltrommel
        double rred  = 0.6;   //reduziert

        public MainWindow()
        {
            InitializeComponent();
        }

        // =========================================================
        // INITIALISIERUNG
        // =========================================================
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.ClearColor(1, 1, 1, 1);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            float[] ambient = { 0.2f, 0.2f, 0.2f, 1 };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, ambient);

            float[] lightPos = { 6, 6, 6, 1 };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, lightPos);
        }

        // =========================================================
        // DRAW
        // =========================================================
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            // ---------------- Kamera (echtes 3D) ----------------
            gl.Translate(-2.5, 0, -14);
            gl.Rotate(20, 1, 0, 0);
            gl.Rotate(-15, 0, 1, 0);

            // ---------------- Kinematik ----------------
            double phi3 = -phi2 * r2 / r32;
            double phi4 = phi3 * r32 / r43;

            // ---------------- Radmittelpunkte ----------------
            double x2 = 0.0;
            double x3 = r2 + r32;
            double x4 = x3 + r32 + r43 - rred;

            // ---------------- Räder ----------------
            // ★ NEU: FARBE RAD 2 (ROT – ANTRIEB)
            SetMaterial(gl, 0.85f, 0.25f, 0.25f);
            DrawGear(gl, x2, 0, 0, r2, phi2);

            // ★ NEU: FARBE RAD 3 (BLAU – ZWISCHENRAD)
            SetMaterial(gl, 0.25f, 0.4f, 0.85f);
            DrawGear(gl, x3, 0, 0, r32, phi3);

            // ★ NEU: FARBE RAD 4 (GRÜN – ABTRIEB)
            SetMaterial(gl, 0.3f, 0.75f, 0.3f);

            DrawGear(gl, x4, 0, 0, r43, phi4);
            DrawGear(gl, x2, 0, 0, r2, phi2);
            DrawGear(gl, x3, 0, 0, r32, phi3);
            DrawGear(gl, x4, 0, 0, r43, phi4);

            // ---------------- Seil & Masse ----------------
            double ropeX = x4;
            double ropeY = -r43;
            double y5 = ropeY + phi4 * r45;

            // Seil
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Color(0, 0, 0);
            gl.LineWidth(3);

            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(ropeX, ropeY, 0);
            gl.Vertex(ropeX, y5, 0);
            gl.End();

            gl.Enable(OpenGL.GL_LIGHTING);

            // Masse (Kugel)

            // ★ NEU: neutrale Farbe für die Last
            SetMaterial(gl, 0.2f, 0.2f, 0.2f);

            gl.PushMatrix();
            gl.Translate(ropeX, y5 - 0.4, 0);
            DrawSphere(gl, 0.4, 40, 40);
            gl.PopMatrix();

            // ---------------- Animation ----------------
            phi2 += 0.02;
        }

        // =========================================================
        // GEAR (3D-SCHEIBE)
        // =========================================================
        private void SetMaterial(OpenGL gl, float r, float g, float b)
        {
            float[] ambient = { 0.2f * r, 0.2f * g, 0.2f * b, 1 };
            float[] diffuse = { r, g, b, 1 };
            float[] specular = { 1, 1, 1, 1 };
            float[] shin = { 50 };

            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, ambient);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, diffuse);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, specular);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shin);
        }

        private void DrawGear(OpenGL gl, double x, double y, double z, double r, double angle)
        {
            gl.PushMatrix();
            gl.Translate(x, y, z);
            gl.Rotate(angle * 180 / Math.PI, 0, 0, 1);

            int seg = 80;
            double thickness = 0.35;

            float[] matDiff = { 0.35f, 0.35f, 0.35f, 1 };
            float[] matSpec = { 1, 1, 1, 1 };
            float[] shin = { 50 };

            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, matDiff);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, matSpec);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shin);

            // Mantel
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= seg; i++)
            {
                double a = i * 2 * Math.PI / seg;
                double nx = Math.Cos(a);
                double ny = Math.Sin(a);

                gl.Normal(nx, ny, 0);
                gl.Vertex(r * nx, r * ny, +thickness);
                gl.Vertex(r * nx, r * ny, -thickness);
            }
            gl.End();

            // Vorderseite
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, 1);
            gl.Vertex(0, 0, thickness);
            for (int i = 0; i <= seg; i++)
            {
                double a = i * 2 * Math.PI / seg;
                gl.Vertex(r * Math.Cos(a), r * Math.Sin(a), thickness);
            }
            gl.End();

            // Rückseite
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 0, -1);
            gl.Vertex(0, 0, -thickness);
            for (int i = 0; i <= seg; i++)
            {
                double a = -i * 2 * Math.PI / seg;
                gl.Vertex(r * Math.Cos(a), r * Math.Sin(a), -thickness);
            }
            gl.End();

            gl.PopMatrix();
        }

        // =========================================================
        // KUGEL (MASSE)
        // =========================================================
        private void DrawSphere(OpenGL gl, double radius, int slices, int stacks)
        {
            float[] matDiff = { 0.2f, 0.2f, 0.2f, 1 };
            float[] matSpec = { 1, 1, 1, 1 };
            float[] shin = { 80 };

            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, matDiff);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, matSpec);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shin);

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
                    gl.Vertex(radius * x1, radius * y1, radius * z1);

                    gl.Normal(x2, y2, z2);
                    gl.Vertex(radius * x2, radius * y2, radius * z2);
                }
                gl.End();
            }
        }
    }

}
