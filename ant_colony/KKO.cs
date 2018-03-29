using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace KarincaKoloni
{
    public partial class KKO : Form
    {
        public Bitmap canvas;
        private List<Karinca> karinca_liste;
        private List<Sehir> sehir_liste;
        private List<int> enIyýTur_liste;
        private List<int> bruteForce_liste;
        private List<int> bruteForceTemp_liste;
        private double enIyýTurUzunlugu = -1;
        private double bruteForceUzunlugu = -1;
        private double[,] mesafeler;
        private double[,] feromonlar;
        private int sayac = 0;
        private int sehir_sayi = 10;
        private int karinca_sayi = 30;
        private int feromonSabit = 100;
        private double ALPHA = 1.0;//weight of pheromone
        private double BETA = 1.0;//weight of distance
        private double RHO = .5;//decay rate
        private int iterasyon = 100;
        private int clickSayac = 0;
        private bool secilen_sehir = false;
        private Stopwatch bruteSure = new Stopwatch();
        private Stopwatch KKOSure = new Stopwatch();
        private Random rand = new Random();
        public DoubleBufferPanel DrawingPanel = new DoubleBufferPanel();

        public KKO()
        {
            InitializeComponent();
            DrawingPanel.Size = new System.Drawing.Size(400, 400);
            DrawingPanel.Location = new System.Drawing.Point(12, 12);
            DrawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawingPanel_Paint);
            DrawingPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DrawingPanel_MouseClick);
            DrawingPanel.Parent = this;
            this.Controls.Add(DrawingPanel);
        }

        private void KKO_Load(object sender, EventArgs e)
        {
            canvas = new Bitmap(DrawingPanel.Width, DrawingPanel.Height,
               System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.Beige);
            sehir_sayi = int.Parse(number_Cities_Box.Text);
            karinca_sayi = int.Parse(number_Ants_Box.Text);
            sehir_liste = new List<Sehir>();
            karinca_liste = new List<Karinca>();
            enIyýTur_liste = new List<int>();
        }
        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(canvas, 0, 0, canvas.Width, canvas.Height);
        }
        private void Draw_button_Click(object sender, EventArgs e)
        {
            if (clickSayac == int.Parse(number_Cities_Box.Text) && !backgroundWorker1.IsBusy)
            {
                KKOSure.Start();
                secilen_sehir = false;
                Graphics g = Graphics.FromImage(canvas);
                KarincaYerlestir();

                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }

        }
        private void DrawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            Graphics g = Graphics.FromImage(canvas);
            sehir_sayi = int.Parse(number_Cities_Box.Text);
            if (secilen_sehir == true && clickSayac < sehir_sayi)
            {
                Sehir sehir = new Sehir(e.Location);
                sehir.Ciz(g);
                sehir_liste.Add(sehir);
                clickSayac++;
            }
            if (secilen_sehir == true && clickSayac == sehir_sayi)
            {
                KKOBaslat(g);
            }
            DrawingPanel.Invalidate();

        }
        private void define_cities_Button_Click(object sender, EventArgs e)
        {
            sehir_sayi = int.Parse(number_Cities_Box.Text);
            secilen_sehir = true;
        }
        private void KKOBaslat(Graphics g)
        {
            karinca_liste = new List<Karinca>();
            enIyýTur_liste = new List<int>();
            enIyýTurUzunlugu = -1;
            sehir_sayi = int.Parse(number_Cities_Box.Text);
            karinca_sayi = int.Parse(number_Ants_Box.Text);
            mesafeler = new double[sehir_sayi, sehir_sayi];
            feromonlar = new double[sehir_sayi, sehir_sayi];
            SehirYerlestir(g);
            KarincaYerlestir();
            FeromonYerlestir();
        }
        private void KarincaYerlestir()
        {
            int rand_city = 0;
            karinca_liste.Clear();
            karinca_sayi = int.Parse(number_Ants_Box.Text);
            for (int i = 0; i < karinca_sayi; i++)
            {
                rand_city = rand.Next(0, sehir_sayi);
                karinca_liste.Add(new Karinca(rand_city, sehir_sayi));//random start location
                karinca_liste[i].turListe[0] = karinca_liste[i].getOncekiKonum();//set tour's first position as current location
                karinca_liste[i].tabuListe[karinca_liste[i].getOncekiKonum()] = 1;//set to 1 to designate that we went to this city
                karinca_liste[i].tur_sayi = 1;
            }
        }
        private void SehirYerlestir(Graphics g)
        {
        
            mesafeler = new double[sehir_sayi, sehir_sayi];
            

            //compute city distances
            //(n^2-n)/2 == number of connections btwn cities
            for (int i = 0; i < sehir_sayi; i++)
                for (int k = 0; k < sehir_sayi; k++)
                {
                    double x = Math.Pow((double)sehir_liste[i].KonumGetir().X -
                        (double)sehir_liste[k].KonumGetir().X, 2.0);
                    double y = Math.Pow((double)sehir_liste[i].KonumGetir().Y -
                        (double)sehir_liste[k].KonumGetir().Y, 2.0);
                    mesafeler[i, k] = Math.Sqrt(x + y);
                }

        }
        private void FeromonYerlestir()
        {
            for (int i = 0; i < sehir_sayi; i++)
            {
                for (int j = 0; j < sehir_sayi; j++)
                {//initialize all pheromone btwn cities as a small constant
                    feromonlar[i, j] = 1.0 / (double)sehir_sayi;
                    feromonlar[j, i] = 1.0 / (double)sehir_sayi;
                }
            }
        }



        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Hesapla();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.Beige);
            Best_Tour_Box.Text = Math.Round(enIyýTurUzunlugu, 2).ToString();

            for (int i = 0; i < sehir_sayi; i++)
            {
                sehir_liste[i].Ciz(g);
            }
            //draw solutions
            for (int i = 0; i < enIyýTur_liste.Count; i++)
            {
                g.DrawLine(new Pen(Color.Black, 5), sehir_liste[enIyýTur_liste[i]].KonumGetir(),
                    sehir_liste[enIyýTur_liste[(i + 1) % sehir_sayi]].KonumGetir());//loop back to last point

                if (i == enIyýTur_liste.Count - 1)
                {
                    enIyýTurUzunlugu = -1;
                    enIyýTur_liste.Clear();
                }
            }
            DrawingPanel.Invalidate();
            KKOSure.Stop();
            totalTimeACO.Text = KKOSure.Elapsed.TotalSeconds.ToString();
            KKOSure.Reset();
        }

        private void Hesapla()
        {
            ALPHA = double.Parse(alpha_Box.Text);
            BETA = double.Parse(beta_Box.Text);
            RHO = double.Parse(rho_Box.Text);
            iterasyon = int.Parse(iterationBox.Text);
            for (int i = 0; i < iterasyon; i++)
            {
                for (int j = 0; j < sehir_sayi; j++)//move ants till they reach the end
                    if (KKODurdur()) //moves ants 1 step & check if all ants finished moving?
                    {
                        FeromonBuharlastýr();
                        FeromonGuncelle();
                        EnIyýTur();//go through every ant and check if it has optimal solution
                        KarincaYerlestir(); // reset ant position and tour
                    }
            }
        }

        private void SonrakiSehireGit(Karinca oncekiKarinca)
        {
            double toplam_olas = 0;//denominator in probability function
            double hareket_olas = 0;//numerator in probability function
            int oncekiSehir = oncekiKarinca.getOncekiKonum();
            for (int i = 0; i < sehir_sayi; i++)//loop through all cities
            {
                if (oncekiKarinca.tabuListe[i] == 0)
                {
                    toplam_olas += Math.Pow(feromonlar[oncekiSehir, i], ALPHA) *Math.Pow(1.0 / mesafeler[oncekiSehir, i], BETA);
                }
            }

            int hedefSehir = 0;
            double randHareket = 0;
            int count = 0;
            //loops until ant chooses a city
            while (count < 400)//400 is the threshold for loops
            {
                if (oncekiKarinca.tabuListe[hedefSehir] == 0)//ant hasnt been to  this city
                {//calculate probability of movement
                    hareket_olas = (Math.Pow(feromonlar[oncekiSehir, hedefSehir], ALPHA) *Math.Pow(1.0 / mesafeler[oncekiSehir, hedefSehir], BETA)) / toplam_olas;
                    randHareket = rand.NextDouble();
                    if (randHareket < hareket_olas) break;//break loop if ant moves to city
                }
                hedefSehir++;
                if (hedefSehir >= sehir_sayi) hedefSehir = 0;//reset city count
                count++;
            }
            //update next location and tour itinerary
            oncekiKarinca.setSonrakiKonum(hedefSehir);//going to that city
            oncekiKarinca.tabuListe[hedefSehir] = 1;//moved to that city
            oncekiKarinca.turListe[oncekiKarinca.tur_sayi] = hedefSehir;
            oncekiKarinca.tur_sayi++;
            //add to current distance
            oncekiKarinca.ToplamMesafeGuncelle(mesafeler[oncekiKarinca.getOncekiKonum(), hedefSehir]);

            //if the ant reached the end, add up the distance for return path
            if (oncekiKarinca.tur_sayi == sehir_sayi)
            {
                oncekiKarinca.ToplamMesafeGuncelle(
                    mesafeler[oncekiKarinca.turListe[sehir_sayi - 1], oncekiKarinca.turListe[0]]);
            }

            oncekiKarinca.setOncekiKonum(hedefSehir);//update current city to next city
        }


        private bool KKODurdur()
        {
            int hareket = 0;
            for (int i = 0; i < karinca_sayi; i++)
            {
                if (karinca_liste[i].tur_sayi < sehir_sayi)
                {//ants are still in moving
                    SonrakiSehireGit(karinca_liste[i]);
                    hareket++;
                }
            }
            if (hareket == 0)
            {
                return true;//ants have finished moving
            }
            else return false;
        }

        public void FeromonBuharlastýr()
        {
            //handles both cases of [i,k] and [k,i] so dont need to code pheromones 2x
            for (int i = 0; i < sehir_sayi; i++)
                for (int j = 0; j < sehir_sayi; j++)
                {
                    feromonlar[i, j] *= (1.0 - RHO);
                    //pheromone levels should always be at base levels
                    if (feromonlar[i, j] < 1.0 / (double)sehir_sayi)
                    {
                        feromonlar[i, j] = 1.0 / (double)sehir_sayi;
                    }
                }
        }

        private void FeromonGuncelle()
        {
            for (int i = 0; i < karinca_sayi; i++)
            {
                for (int j = 0; j < sehir_sayi; j++)
                {
                    int f = karinca_liste[i].turListe[j];//starting point of edge
                    //if city+1=num_cities, then city is last city and then destination is the starting city
                    int t = karinca_liste[i].turListe[((j + 1) % sehir_sayi)];//endpoint of edge
                    feromonlar[f, t] += (double)feromonSabit / karinca_liste[i].MesafeGetir();
                    feromonlar[t, f] = feromonlar[f, t];

                }
            }
        }
    
        private void EnIyýTur()
        {
            double eniyiYerelTur = karinca_liste[0].MesafeGetir();
            int indexKaydet = 0;
            for (int i = 1; i < karinca_liste.Count; i++)//checks the best tour length among this iteration
            {
                if (karinca_liste[i].MesafeGetir() < eniyiYerelTur)
                {
                    eniyiYerelTur = karinca_liste[i].MesafeGetir();
                    indexKaydet = i;
                }
            }
            //compare best local length with global length and update accordingly
            if (eniyiYerelTur < enIyýTurUzunlugu || enIyýTurUzunlugu == -1)
            {
                enIyýTur_liste = karinca_liste[indexKaydet].turListe;
                enIyýTurUzunlugu = eniyiYerelTur;
            }
        }

        private void clear_button_Click(object sender, EventArgs e)
        {
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.Beige);
            Best_Tour_Box.Text = "0";
            totalTimeACO.Text = "0";
            totalTimeBrute.Text = "0";
            brute_length_label.Text = "0";
            karinca_liste.Clear();
            sehir_liste.Clear();
            clickSayac = 0;
            enIyýTurUzunlugu = -1;

            enIyýTur_liste.Clear();
            feromonlar = new double[sehir_sayi, sehir_sayi];
            mesafeler = new double[sehir_sayi, sehir_sayi];
            DrawingPanel.Invalidate();
        }

        private void permute(int toplam,List<int> v, int basla, int n)
        {
            int percentProgress = (int)(100.0 * ((float)(sayac+1) / (float)toplam));
            if( percentProgress %5 ==0)
                backgroundWorker2.ReportProgress(percentProgress);


            if (basla == n - 1)
            {
                double yerelUzunluk = 0;
                sayac++;
                for (int i = 0; i < v.Count; i++)
                {
                    yerelUzunluk += Math.Sqrt(Math.Pow(sehir_liste[v[i]].KonumGetir().X - sehir_liste[v[(i + 1) % sehir_sayi]].KonumGetir().X, 2.0) +
                        Math.Pow(sehir_liste[v[i]].KonumGetir().Y - sehir_liste[v[(i + 1) % sehir_sayi]].KonumGetir().Y, 2.0));
                }
                if (yerelUzunluk < bruteForceUzunlugu || bruteForceUzunlugu == -1)
                {
                    bruteForceUzunlugu = yerelUzunluk;
                    bruteForceTemp_liste.Clear();
                    for (int i = 0; i < v.Count; i++)
                    {
                        bruteForceTemp_liste.Add(v[i]);
                    }
                }
            }
            else
            {
                for (int i = basla; i < n; i++)
                {
                    int tmp = v[i];

                    v[i] = v[basla];
                    v[basla] = tmp;
                    permute(toplam,v, basla + 1, n);
                    v[basla] = v[i];
                    v[i] = tmp;
                }
            }
        }
        //recursion for n!
        public int fact(int n)
        {
            if (n == 0) return 1;
            else return n * fact(n - 1);
        }

        private void brute_force_button_Click(object sender, EventArgs e)
        {
            sayac = 0;
            int temp_city;
            string s = number_Cities_Box.Text;
            bool sonuc = int.TryParse(s, out temp_city);
            if (sonuc && sehir_liste.Count != 0 && !backgroundWorker2.IsBusy)
            {
                Graphics g = Graphics.FromImage(canvas);
                sehir_sayi = int.Parse(number_Cities_Box.Text);
                bruteForce_liste = new List<int>();
                bruteForceTemp_liste = new List<int>();
                for (int i = 0; i < sehir_sayi; i++)
                {
                    bruteForce_liste.Add(i);
                }
                backgroundWorker2.RunWorkerAsync();
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            bruteSure.Start();
            int toplam = fact(sehir_sayi);
            permute(toplam,bruteForce_liste, 0, sehir_sayi);
        }
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Graphics g = Graphics.FromImage(canvas);

            for (int i = 0; i < bruteForceTemp_liste.Count; i++)
            {
                g.DrawLine(new Pen(Color.Red), sehir_liste[bruteForceTemp_liste[i]].KonumGetir(),
                    sehir_liste[bruteForceTemp_liste[(i + 1) % sehir_sayi]].KonumGetir());
            }
            brute_length_label.Text = Math.Round(bruteForceUzunlugu, 2).ToString();
            bruteSure.Stop();
            totalTimeBrute.Text = bruteSure.Elapsed.TotalSeconds.ToString();
            bruteSure.Reset();
            bruteForceUzunlugu = -1;
            bruteForceTemp_liste.Clear();
            DrawingPanel.Invalidate();
        }

        private void alpha_Box_TextChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }


    public class DoubleBufferPanel : Panel
    {

        public DoubleBufferPanel()
        {
            // Set the value of the double-buffering style bits to true.
            // ControlStyles.UserPaint -- allows user to control painting w/o passing off the work to the operating system
            //ControlStyles.AllPaintingInWmPaint--optimize to reduce flicker but only use it if ControlStyles.UserPaint is true
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
             ControlStyles.AllPaintingInWmPaint, true);// | evaluates all conditions even if condition 1 is true
            this.UpdateStyles();//forces style to be applied
        }

    }
}