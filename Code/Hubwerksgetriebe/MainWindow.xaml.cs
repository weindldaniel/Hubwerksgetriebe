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
using System.Windows.Threading;
using System.IO;
using System.Globalization;
using System.Windows.Threading;
using SharpGL;

namespace Hubwerksgetriebe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region PRIVATE MEMBERS
        private float _rotation = 0;
        // Rollenparameter
        private double _r2 = 2;
        private double _r32 = 5;
        private double _r34 = 2;
        private double _r43 = 4;
        private double _r45 = 2;
        private double _thickness2 = 2;
        private double _thickness31 = 1;
        private double _thickness32 = 2;
        private double _thickness41 = 3;
        private double _thickness42 = 4;
        private int _segments = 50;
        // X & Z Positionen
        private double _x2 = 0;
        private double _z3 = 1;
        private double _z45 = 2;
        // Winkel
        private double _phi2 = 0.0; // [rad]
        // Punktmaße & Seil
        private double _ropeBaseY = -10;   // Ausgangsposition der Masse
        // DOF-Zeitreihe
        private List<double> _phi2Sequence = new List<double>();
        private int _phi2Index = 0;
        private DispatcherTimer _phiTimer;
        #endregion
        
        public MainWindow()
        {
            InitializeComponent();

            // CSV laden
            LoadPhi2FromCsv(@"Z:\_source\Repositorys\SSI_Hubwerksgetriebe\phi2.csv");

            // Timer starten
            _phiTimer = new DispatcherTimer();
            _phiTimer.Interval = TimeSpan.FromSeconds(0.1);
            _phiTimer.Tick += PhiTimer_Tick;
            _phiTimer.Start();
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
            #region DECLARATIONS
            // X & Z Positionen
            double x3 = _r2 + _r32;
            double x4 = _r2 + _r32 + _r34 + _r43 ;
            // müssen noch berechnet werden für parametrischen aufbau
            double z3;
            double z45;
            // Winkel
            double phi3 = -_phi2 * (_r2 / _r32);
            double phi4 = _phi2 * ((_r2 * _r34) / (_r32 * _r43));
            // Rope 
            double ropeDisplacement = _r45 * phi4;   // s = r * φ
            double ropeY = _ropeBaseY + ropeDisplacement;
            double ropeZ = _z45 + _thickness42 / 2.0; // vordere Stirnfläch
                
            #endregion
            
            #region OpenGL  settings
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            
            // Kamera ausrichtung
            gl.Translate(-5, 5, -30);
            gl.Rotate(20, 1, 0, 0);
            // gl.Rotate(-40, 0, 1, 0);
            DrawFadenkreuz(gl);
            // gl.Rotate(_rotation, 0, 1, 0);
            gl.Enable(OpenGL.GL_LIGHTING);
            
            #endregion

            #region Drawings
            // ---------------- Rolle 2 ----------------
            SetMaterial(gl, 0f, 0f, 0.5f);
            gl.PushMatrix();
            gl.Translate(_x2, 0, 0);
            gl.Rotate(_phi2 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy(gl,_r2,_thickness2,_segments,0,0,0,DiskMarkerColor.Red,DiskMarkerDirection.PositiveX);
            //drawer.DrawDiskXy(gl, _r2, _thickness2, _segments, 0, 0, 0, DiskMarkerColor Red, DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            // ---------------- Rolle 3.1 ----------------
            SetMaterial(gl, 0f, 1f, 0f);
            gl.PushMatrix();
            gl.Translate(x3,0, 0);
            gl.Rotate(phi3 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy(gl,_r32,_thickness31,_segments,0,0,0,DiskMarkerColor.Red,DiskMarkerDirection.NegativeX);
            gl.PopMatrix();
            // ---------------- Rolle 3.2 ----------------
            gl.PushMatrix();
            gl.Translate(x3,0, _z3);
            gl.Rotate(phi3 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy(gl,_r34,_thickness32,_segments,0,0,_thickness31/2,DiskMarkerColor.Blue,DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            // ---------------- Rolle 4.1 ----------------
            SetMaterial(gl, 1f, 0f, 0f); 
            gl.PushMatrix();
            gl.Translate(x4, 0, _z3);
            gl.Rotate(phi4 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy(gl,_r43,_thickness41,_segments,0,0,_thickness32/2,DiskMarkerColor.Blue,DiskMarkerDirection.NegativeX);
            gl.PopMatrix();
            // ---------------- Rolle 4.2 ----------------
            gl.PushMatrix();
            gl.Translate(x4, 0, _z45);
            gl.Rotate(phi4 * 180.0 / Math.PI, 0, 0, 1);
            DrawDiskXy(gl,_r45,_thickness42,_segments,0,0,_thickness41/2,DiskMarkerColor.Green,DiskMarkerDirection.PositiveX);
            gl.PopMatrix();
            // ---------------- Seil ----------------
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Color(0, 0, 0);
            gl.LineWidth(3.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(x4+_r45, 0, ropeZ);   // Austritt an der Rolle
            gl.Vertex(x4+_r45, ropeY, ropeZ);       // bewegte Masse
            gl.End();
            gl.Enable(OpenGL.GL_LIGHTING);
            // ---------------- Punktmasse ----------------
            SetMaterial(gl, 0, 0, 0);
            DrawSphere(gl, 0.5, 30, 30, x4+_r45, ropeY - 0.4, ropeZ
            );
            #endregion
            
            _rotation -= -0.5f;
            // _phi2 += 0.05; // rad pro Frame
        }

        // ================= Hilfsmethoden =================
        // Visualisierung
        private void DrawDiskXy(OpenGL gl, double radius, double thickness, int segments, double cx, double cy, double cz, DiskMarkerColor markerColor, DiskMarkerDirection markerDirection)
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
        // CSV 
        private void LoadPhi2FromCsv(string filePath)
        {
            _phi2Sequence.Clear();
            _phi2Index = 0;

            foreach (string rawLine in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue;

                // BOM + Whitespaces entfernen
                string line = rawLine
                    .Trim()
                    .TrimStart('\uFEFF');

                // Kommentare / Header überspringen
                if (line.StartsWith("#"))
                    continue;

                if (line.ToLower().Contains("phi"))
                    continue;

                // Falls mehrere Spalten → erste nehmen
                string firstToken = line
                    .Split(';', ',', '\t')
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(firstToken))
                    continue;

                // Dezimalnormalisierung
                firstToken = firstToken.Replace(',', '.');

                if (double.TryParse(
                        firstToken,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                {
                    _phi2Sequence.Add(value);
                }
                else
                {
                    // 🔴 Debug-Hilfe (SEHR WICHTIG)
                    System.Diagnostics.Debug.WriteLine(
                        $"Parse fehlgeschlagen: '{firstToken}'");
                }
            }

            if (_phi2Sequence.Count == 0)
                throw new InvalidOperationException(
                    "CSV enthält keine gültigen phi2-Werte.");
        }
        // Time Heartbeat
        private void PhiTimer_Tick(object sender, EventArgs e)
        {
            if (_phi2Index >= _phi2Sequence.Count)
            {
                _phiTimer.Stop();   // Ende der Sequenz
                return;
            }

            _phi2 = _phi2Sequence[_phi2Index];
            _phi2Index++;
        }
    }
}    