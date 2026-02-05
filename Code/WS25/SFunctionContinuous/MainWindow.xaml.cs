using Microsoft.Msagl.Drawing;
using ScottPlot.Plottables;
using SFunctionContinuous.Framework;
using SFunctionContinuous.Framework.Blocks;
using SFunctionContinuous.Framework.Examples;
using SFunctionContinuous.Framework.Solvers;
using System.Windows;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

namespace SFunctionContinuous
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Modell erstellen und lösen

            //Example example = new PosServo(); 
            Example example = new Hubwerk();

            try
            {
                Solver solution = new EulerExplicitSolver(example.Model);
                solution.Solve(example.TimeStepMax, example.TimeMax);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            // ================================
            // CSV-EXPORT VON PHI (HIER!)
            // ================================

            RecordBlock phiBlock = example.Model.Blocks
                .OfType<RecordBlock>()
                .First(b => b.Name == "Phi");

            string filePath = @"Z:\_source\Repositorys\SSI_Hubwerksgetriebe\phi2.csv";

            CultureInfo culture = CultureInfo.InvariantCulture;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("phi");

            foreach ((_, double phi) in phiBlock.Data)
            {
                sb.AppendLine(phi.ToString(culture));
            }

            File.WriteAllText(filePath, sb.ToString());
            
            // Graph-Visualisierung erstellen

            Graph graph = new Graph();

            foreach (Block f in example.Model.Blocks)
            {
                graph.AddNode($"{f.GetHashCode()}").LabelText = f.ToString();
            }
            foreach (Connection c in example.Model.Connections)
            {
                graph.AddEdge($"{c.Source.GetHashCode()}", c.ToString(), $"{c.Target.GetHashCode()}");
            }

            Graph.Graph = graph;

            // Chart-Visualisierung erstellen

            foreach (Block f in example.Model.Blocks)
            {
                if (f is RecordBlock)
                {
                    RecordBlock r = (RecordBlock)f;

                    double[] t = new double[r.Data.Count];
                    double[] u = new double[r.Data.Count];

                    for (int i = 0; i < r.Data.Count; i++)
                    {
                        (double ti, double tu) = r.Data[i];

                        t[i] = ti;
                        u[i] = tu;
                    }

                    Scatter scatter = Chart.Plot.Add.Scatter(t, u);

                    scatter.LegendText = f.Name;
                }
            }
        }
    }
}