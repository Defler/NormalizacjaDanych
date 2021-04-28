using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MIW1
{
    public class AustralianConfig
    {
        public string nazwa { get; set; }
        public int iloscKolumn { get; set; }
        public int iloscWierszy { get; set; }
        public string[] rodzajKolumny { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            StreamReader file = File.OpenText("conAustralian.json");
            JsonSerializer serializer = new JsonSerializer();
            AustralianConfig config = (AustralianConfig)serializer.Deserialize(file, typeof(AustralianConfig));


            System.Data.DataTable dane = new DataTable("TablicaDanych");

            DataColumn column;
            DataRow row;

            for(int i=0; i<config.iloscKolumn; i++)
            {
                column = new DataColumn();
                if (config.rodzajKolumny[i] == "liczba")
                    column.DataType = System.Type.GetType("System.Double");
                else
                    column.DataType = System.Type.GetType("System.String");
                column.ColumnName = $"kol{i}";
                dane.Columns.Add(column);
            }

            for(int i=0; i<config.iloscWierszy; i++)
            {
                row = dane.NewRow();
                dane.Rows.Add(row);
            }


            string[] load = System.IO.File.ReadAllLines("australian.dat");
            int nrWiersza = 0;
            string[] wiersz;

            foreach (string line in load)
            {
                wiersz = line.Split(' ');
                for (int i = 0; i < wiersz.Length; i++)
                {
                    wiersz[i] = wiersz[i].Replace('.', ',');
                    dane.Rows[nrWiersza][i] = Convert.ToDouble(wiersz[i]);
                }
                nrWiersza++;
            }

            double[] tabMin = new double[config.iloscKolumn];
            double[] tabMax = new double[config.iloscKolumn];

            for(int i=0; i<config.iloscKolumn; i++)
            {
                tabMin[i] = Convert.ToDouble(dane.Rows[0][i]);
                tabMax[i] = Convert.ToDouble(dane.Rows[0][i]);
            }

            for(int i=0; i<config.iloscKolumn; i++)
            {
                for(int j=1; j<config.iloscWierszy; j++)
                {
                    if (dane.Rows[j].Field<double>(i) < tabMin[i])
                        tabMin[i] = dane.Rows[j].Field<double>(i);
                    if (dane.Rows[j].Field<double>(i) > tabMax[i])
                        tabMax[i] = dane.Rows[j].Field<double>(i);
                }
            }

            System.Data.DataTable daneZnormalizowane = new DataTable("TablicaDanychZnormalizowanych");

            for (int i = 0; i < config.iloscKolumn; i++)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.Double");
                column.ColumnName = $"kol{i}";
                daneZnormalizowane.Columns.Add(column);
            }
            for (int j = 0; j < config.iloscWierszy; j++)
            {
                row = daneZnormalizowane.NewRow();
                daneZnormalizowane.Rows.Add(row);
            }

            for(int i=0; i<config.iloscWierszy; i++)
            {
                for(int j=0; j<config.iloscKolumn; j++)
                {
                    daneZnormalizowane.Rows[i][j] = (dane.Rows[i].Field<double>(j) - tabMin[j]) / (tabMax[j] - tabMin[j]);
                }
            }


            /*
            for (int i=0; i<tabMax.Length; i++)
            {
                Console.Write($"{tabMin[i]} ");
            }
            */


            
            for(int i=0; i<daneZnormalizowane.Rows.Count; i++)
            {
                for (int j = 0; j < daneZnormalizowane.Columns.Count; j++)
                    Console.Write($"{Math.Round(daneZnormalizowane.Rows[i].Field<double>(j),2)} ");
                Console.WriteLine();
            }
            

            Console.WriteLine($"Nazwa : {nrWiersza}");
            System.Console.ReadKey();
        }
    }
}
