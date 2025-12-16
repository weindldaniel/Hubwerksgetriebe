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

            // Hintergrundfarbe
            gl.ClearColor(1, 1, 1, 1);

            // Lichtsystem
            gl.Enable(OpenGL.GL_LIGHTING);

            // Umgebungslicht
            float[] ambient = { 0.2f, 0.2f, 0.2f, 1 };

            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, ambient);

            // Punktlicht
            gl.Enable(OpenGL.GL_LIGHT0);

            float[] position = { 0, 0, 0, 1 };
            float[] diffuse = { 1, 1, 1, 1 };
            float[] specular = { 1, 1, 1, 1 };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, position);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, specular);

            // Schattierungsmodell
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            // Tiefentest
            gl.Enable(OpenGL.GL_DEPTH_TEST);
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;

            #region DECLARATIONS & BEGINNING
            // Puffer löschen
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            // Kamera zurücksetzen
            gl.Translate(0.0f, 0.0f, -9.0f);

            // Szene leicht rotieren
            gl.Rotate(_rotation, 1.0f, 1.0f, 0.0f);

            // Fadenkreuz zeichnen
            gl.Color(0.0f, 0.0f, 0.0f);
            gl.LineWidth(2.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(-10.0f, 0.0f, 0.0f);
            gl.Vertex(10.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, -10.0f, 0.0f);
            gl.Vertex(0.0f, 10.0f, 0.0f);
            gl.End();

            // Materialeigenschaften für große Kugel
            float[] mat_ambient = { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] mat_diffuse = { 0.4f, 0.6f, 0.9f, 1.0f };
            float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] mat_shininess = { 50.0f };

            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, mat_ambient);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, mat_diffuse);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, mat_specular);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, mat_shininess);

            // Lichtquelle
            float[] light_position = { 2.0f, 3.0f, 4.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light_position);
            #endregion

            // Kugelparameter
            double radius = 1.5;
            int slices = 50;
            int stacks = 50;

            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_NORMALIZE);

            // ---------- Große Kugel ----------
            for (int j = 0; j < stacks; j++)
            {
                double phi1 = j * Math.PI / stacks;
                double phi2 = (j + 1) * Math.PI / stacks;

                gl.Begin(OpenGL.GL_QUAD_STRIP);
                for (int i = 0; i <= slices; i++)
                {
                    double theta = i * 2.0 * Math.PI / slices;

                    double x1 = Math.Sin(phi1) * Math.Cos(theta);
                    double y1 = Math.Cos(phi1);
                    double z1 = Math.Sin(phi1) * Math.Sin(theta);

                    double x2 = Math.Sin(phi2) * Math.Cos(theta);
                    double y2 = Math.Cos(phi2);
                    double z2 = Math.Sin(phi2) * Math.Sin(theta);

                    gl.Normal(x1, y1, z1);
                    gl.Vertex(radius * x1, radius * y1, radius * z1);

                    gl.Normal(x2, y2, z2);
                    gl.Vertex(radius * x2, radius * y2, radius * z2);
                }
                gl.End();
            }
            
            // ---------------- Scheibe ----------------
            double diskRadius = 3;
            double diskThickness = 0.15;
            double diskY = -0.8;
            double diskZ = radius + 0.2;
            int diskSegments = 60;

            // obere Fläche
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, 1, 0);
            gl.Vertex(0, diskY + diskThickness / 2, diskZ);
            for (int i = 0; i <= diskSegments; i++)
            {
                double t = i * 2 * Math.PI / diskSegments;
                gl.Vertex(
                    diskRadius * Math.Cos(t),
                    diskY + diskThickness / 2,
                    diskZ + diskRadius * Math.Sin(t)
                );
            }
            gl.End();

            // untere Fläche
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            gl.Normal(0, -1, 0);
            gl.Vertex(0, diskY - diskThickness / 2, diskZ);
            for (int i = 0; i <= diskSegments; i++)
            {
                double t = -i * 2 * Math.PI / diskSegments;
                gl.Vertex(
                    diskRadius * Math.Cos(t),
                    diskY - diskThickness / 2,
                    diskZ + diskRadius * Math.Sin(t)
                );
            }
            gl.End();

            // Mantel
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= diskSegments; i++)
            {
                double t = i * 2 * Math.PI / diskSegments;
                double nx = Math.Cos(t);
                double nz = Math.Sin(t);

                gl.Normal(nx, 0, nz);
                gl.Vertex(
                    diskRadius * nx,
                    diskY + diskThickness / 2,
                    diskZ + diskRadius * nz
                );
                gl.Vertex(
                    diskRadius * nx,
                    diskY - diskThickness / 2,
                    diskZ + diskRadius * nz
                );
            }

            gl.End();

            _rotation += 0.8f;
        }

        // Hilfsmethode für Kugel
        private void DrawSphere(OpenGL gl, double radius, int slices, int stacks, double cx, double cy, double cz)
        {
            for (int j = 0; j < stacks; j++)
            {
                double phi1 = j * Math.PI / stacks;
                double phi2 = (j + 1) * Math.PI / stacks;

                gl.Begin(OpenGL.GL_QUAD_STRIP);
                for (int i = 0; i <= slices; i++)
                {
                    double theta = i * 2.0 * Math.PI / slices;

                    double x1 = Math.Sin(phi1) * Math.Cos(theta);
                    double y1 = Math.Cos(phi1);
                    double z1 = Math.Sin(phi1) * Math.Sin(theta);

                    double x2 = Math.Sin(phi2) * Math.Cos(theta);
                    double y2 = Math.Cos(phi2);
                    double z2 = Math.Sin(phi2) * Math.Sin(theta);

                    gl.Normal(x1, y1, z1);
                    gl.Vertex(cx + radius * x1, cy + radius * y1, cz + radius * z1);

                    gl.Normal(x2, y2, z2);
                    gl.Vertex(cx + radius * x2, cy + radius * y2, cz + radius * z2);
                }
                gl.End();
            }
        }
    }

}
