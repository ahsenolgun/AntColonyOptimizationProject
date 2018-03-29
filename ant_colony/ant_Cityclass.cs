
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace KarincaKoloni
{
    public class Karinca
    {//ant class only contains the index to the city--use city class to get the 2D location
        private int oncekiKonum;
        private int sonrakiKonum;
        public int tur_sayi;
        public List<int> tabuListe;//where ant has been (Tabu)
        public List<int> turListe;//total itinerary
        public double gidilenMesafe;
        

        public Karinca()
        {
            oncekiKonum = 0;
            sonrakiKonum = 0;
            tur_sayi = 0;
            tabuListe = new List<int>();
            turListe = new List<int>();
            gidilenMesafe = 0;
        }
        public Karinca(int baslangýcpoz, int sehirNo)
        {
            oncekiKonum = baslangýcpoz;
            sonrakiKonum = 0;
            tur_sayi = 0;
            tabuListe = new List<int>();
            turListe = new List<int>();
            gidilenMesafe = 0;
            for (int k = 0; k < sehirNo; k++)
            {
                tabuListe.Add(0);
                turListe.Add(0);
            }
        }
        public void ToplamMesafeGuncelle(double mesafe)
        {
            gidilenMesafe += mesafe;
        }
        
        public float MutlakMesafe(PointF a, PointF b)
        {
            return (float)Math.Pow((Math.Pow(a.X - b.X, 2F) + Math.Pow(a.Y - b.Y, 2F)), .5F);
        }
      
        public void setOncekiKonum(int konum)
        {
            oncekiKonum = konum;
        }
        //set ants next location
        public void setSonrakiKonum(int sonraki)
        {
            sonrakiKonum = sonraki;
        }
        //get ants current location
        public int getOncekiKonum()
        {
            return oncekiKonum;
        }
        public double MesafeGetir()
        {
            return gidilenMesafe;
        }

        public void MesafeSýfýrla()
        {
            gidilenMesafe = 0;
        }

    }
    //city class
    public class Sehir
    {
        Point konum; //the city's location in x,y coordinate

        public Sehir()
        {
            konum = new Point(0, 0);
        }

        public Sehir(Point p)
        {
            konum = p;
        }
        public void setKonum(int x, int y)
        {
            konum.X = x;
            konum.Y = y;
        }

        public void Ciz(Graphics g) //draw circle representing city location
        {
            g.FillEllipse(new SolidBrush(Color.Purple), konum.X - 20 / 2, konum.Y - 20 / 2, 20, 20);
        }

        public Point KonumGetir()
        {
            return konum;
        }
    }

}