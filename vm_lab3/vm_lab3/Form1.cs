using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;

namespace vm_lab3
{   
    public partial class Form1 : Form
    {
        double L, t, h, tau, b0, b1, fi1, fi2, b2;
        double[,] resultA;
        double[,] resultB;
        double[] result_test_A;
        int CountOfStepsX, CountOfStepsT;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            button1.Enabled = true;
            button1.Text = "Рассчитать часть B";
            label10.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "7";
            textBox2.Text = "1";
            textBox3.Text = "0.2";
            textBox4.Text = "0.01";
            textBox5.Text = "0";
            textBox6.Text = "0";
            textBox7.Text = "-7";
            textBox8.Text = "0";
            textBox9.Text = "0.2";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            RefreshAllVaruables();             
            if (ReadParams() == 1)
            {
                MessageBox.Show("Введены некорректные параметры!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (tau / Math.Pow(h, 2) >= 0.25)
            {
                MessageBox.Show("Параметры не удовлетворяют условию устойчивости. t/h^2 < 1/4", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }          
           
            CalculateResult();       
            ChartGraph();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[2].Enabled = checkBox1.Checked;
        }
        private int ReadParams()
        {
            try
            {
                L = double.Parse(textBox1.Text, CultureInfo.InvariantCulture.NumberFormat);
                t = double.Parse(textBox2.Text, CultureInfo.InvariantCulture.NumberFormat);
                h = double.Parse(textBox3.Text, CultureInfo.InvariantCulture.NumberFormat);
                tau = double.Parse(textBox4.Text, CultureInfo.InvariantCulture.NumberFormat);
                b0 = double.Parse(textBox6.Text, CultureInfo.InvariantCulture.NumberFormat);
                b1 = double.Parse(textBox7.Text, CultureInfo.InvariantCulture.NumberFormat);
                b2 = double.Parse(textBox5.Text, CultureInfo.InvariantCulture.NumberFormat);
                fi1= double.Parse(textBox8.Text, CultureInfo.InvariantCulture.NumberFormat);
                fi2 = double.Parse(textBox9.Text, CultureInfo.InvariantCulture.NumberFormat);     
            }
            catch
            {
                return 1;
            }
            return 0;        
        }

        
        private double fi(double x)
        {
            return (1.0 / L) + fi1 * Math.Cos((Math.PI * x) / L) + fi2 * Math.Cos((2 * Math.PI * x) / L);
        }

        private double bi(double x)
        {
            return b0 + b1 * Math.Cos((Math.PI * x) / L) + b2 * Math.Cos((2 * Math.PI * x) / L);
        }
        private void ChartGraph()
        {
            dataGridView1.RowCount = CountOfStepsX;
            for (int i = 0; i < CountOfStepsX; ++i)
            {
                chart1.Series[0].Points.AddXY(i * h, resultB[0, i]);
                chart1.Series[1].Points.AddXY(i * h, resultB[CountOfStepsT - 1, i]);
                chart1.Series[2].Points.AddXY(i * h, result_test_A[i]);
                dataGridView1.Rows[i].Cells[0].Value = resultB[0, i].ToString();
                dataGridView1.Rows[i].Cells[1].Value = bi(h * i).ToString();
            }
        }
        private void RefreshAllVaruables()
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            resultA = null;
            resultB = null;
            progressBar1.Value = 0;
            L = t = h = tau = b0 = b1 = fi1 = fi2 = b2 = CountOfStepsX = CountOfStepsT = 0;
        }
     
        private void CalculateResult()
        {
            CountOfStepsX = Convert.ToInt32(L / h);
            CountOfStepsT = Convert.ToInt32(t / tau);
            resultB = new double[CountOfStepsT, CountOfStepsX];
            resultA = new double[CountOfStepsT, CountOfStepsX];
            double[] beta = new double[CountOfStepsX];
            double[] alpha = new double[CountOfStepsX];
            double[] b_local_array = new double[CountOfStepsX];
            double[] x = new double[CountOfStepsX];
            progressBar1.Maximum = CountOfStepsT;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < CountOfStepsX; ++i)
            {
                resultB[0,i] = fi(i * h);
                resultA[0,i] = fi(i * h);
                b_local_array[i] = bi(i * h);
            }

            double[] a = new double[CountOfStepsX];
            double[] b = new double[CountOfStepsX];
            double[] c = new double[CountOfStepsX];

            double coef = 1;
            a[0] = 0.0; 
            b[0] = 1.0; 
            c[0] = -1.0;

            a[1] = tau * coef * coef / Math.Pow(h, 2);
            b[1] = -2.0 * tau * coef * coef / Math.Pow(h,2) - 1.0;
            c[1] = tau * coef * coef / Math.Pow(h,2);

            a[2] = -1.0; b[2] = 1.0; c[2] = 0.0;
           
            for (int j = 1; j < CountOfStepsT; ++j)
            {
                double integral = IntegrateB(b_local_array, j - 1);
                for (int i = 1; i < CountOfStepsX - 1; i++)
                {
                    SolveAlphaBeta(ref alpha, ref beta, j, i, b_local_array, integral);
                }
                beta[0] = beta[CountOfStepsX - 1] = 0;
                alpha[0] = alpha[CountOfStepsX - 1] = 0;          
                RunMatrixAlg(a, b, c, beta, ref x);
                for (int i = 0; i < CountOfStepsX; i++)
                    resultB[j,i] = x[i];
                RunMatrixAlg(a, b, c, alpha, ref x);
                for (int i = 0; i < CountOfStepsX; i++)
                    resultA[j,i] = x[i];
                progressBar1.Value++;
            }

            result_test_A = new double[CountOfStepsX];
            double Aintegral = IntegrateA( CountOfStepsX - 1);
            for (int i = 0; i < CountOfStepsX; i++)
                result_test_A[i] = resultA[CountOfStepsT - 1,i] / Aintegral;
            progressBar1.Value = CountOfStepsT;
            timer.Stop();
            double time = (timer.Elapsed).TotalMilliseconds;
            label5.Text = "Затрачено время: " + Convert.ToString(time) + " мс.";
        }
        private double IntegrateB(double[] b_local_array, int j)
        {
            double integral = resultB[j,0] * b_local_array[0];

            for (int i = 1; i < CountOfStepsX - 1; i += 2)
            {
                integral += 4.0 * resultB[j,i] * b_local_array[i];
                integral += 2.0 * resultB[j,i + 1] * b_local_array[i + 1];
            }

            integral += resultB[j,CountOfStepsX - 1] * b_local_array[CountOfStepsX - 1];
            integral = integral * h / 3.0;
            return integral;
        }

        double IntegrateA(int j)
        {
            double integral = resultA[j,0];

            for (int i = 1; i < CountOfStepsX - 1; i += 2)
            {
                integral += 4.0 * resultA[j,i];
                integral += 2.0 * resultA[j,i + 1];
            }

            integral += resultA[j,CountOfStepsX - 1];
            integral = integral * h / 3.0;
            return integral;
        }

        private void SolveAlphaBeta(ref double[] alpha, ref double [] beta, int j, int i, double[] b_local_array, double integral)
        {
            beta[i] = -resultB[j - 1, i] * (Math.Pow(tau, 2) * b_local_array[i] - integral * Math.Pow(tau, 2) + 1.0);
            alpha[i] = -resultA[j - 1, i] * (Math.Pow(tau, 2) * b_local_array[i] + 1.0);
        }

        private void RunMatrixAlg(double[] a, double[] b, double[] c, double[] f, ref double[] x)
        {
            int size = x.Length;
            double[] A = new double[size];
            double[] B = new double[size];
        
            A[0] = -c[0] / b[0];
            B[0] = f[0] / b[0];

            for (int i = 1; i < size; ++i)
            {
                if (i == size - 1)
                {
                    A[i] = -c[2] / (a[2] * A[i - 1] + b[2]);
                    B[i] = (f[i] - a[2] * B[i - 1]) / (a[2] * A[i - 1] + b[2]);
                    break;
                }
                A[i] = -c[1] / (a[1] * A[i - 1] + b[1]);
                B[i] = (f[i] - a[1] * B[i - 1]) / (a[1] * A[i - 1] + b[1]);
            }

            x[size - 1] = B[size - 1];

            for (int i = size - 2; i >= 0; i--)
                x[i] = A[i] * x[i + 1] + B[i];
        }

    }
}
