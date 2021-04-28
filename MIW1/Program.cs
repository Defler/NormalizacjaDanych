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
        public char separator { get; set; }
    }

    class Program
    {
        public static void UstawienieDataSetu(DataTable dane, string[] rodzajKolumny, int iloscKolumn, int iloscWierszy, bool czySuroweDane)
        {
            DataColumn column;
            DataRow row;

            for (int i = 0; i < iloscKolumn; i++)
            {
                column = new DataColumn();
                if (czySuroweDane)
                {
                    if (rodzajKolumny[i] == "liczba")
                        column.DataType = System.Type.GetType("System.Double");
                    else
                        column.DataType = System.Type.GetType("System.String");
                }
                else
                {
                    column.DataType = System.Type.GetType("System.Double");
                }

                column.ColumnName = $"kol{i}";
                dane.Columns.Add(column);
            }

            for (int i = 0; i < iloscWierszy; i++)
            {
                row = dane.NewRow();
                dane.Rows.Add(row);
            }
        }

        static void Main(string[] args)
        {
            JsonSerializer serializer = new JsonSerializer();

            //wczytanie configa z zamiana znakow na liczby
            StreamReader fileZnaki = File.OpenText("conZnakiNaLiczby.json");
            Dictionary<string, double> tabZnakiNaLiczby = (Dictionary<string, double>)serializer.Deserialize(fileZnaki, typeof(Dictionary<string,double>));

            //wczytanie configa z danymi o secie
            StreamReader fileDane = File.OpenText("conAustralian.json");
            AustralianConfig config = (AustralianConfig)serializer.Deserialize(fileDane, typeof(AustralianConfig));

            //tworzenie 3 dataTabli: jeden z surowymi danymi (jak w secie), drugi z zamienionymi znakami na liczby, trzeci ze znormalizowanymi danymi
            System.Data.DataTable dane = new DataTable("TablicaDanych");
            System.Data.DataTable daneLiczbowe = new DataTable("TablicaDanychLiczbowych");
            System.Data.DataTable daneZnormalizowane = new DataTable("TablicaDanychZnormalizowanych");
            

            UstawienieDataSetu(dane, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy, true);

            //zapisywanie danych do dataTable
            string[] load = System.IO.File.ReadAllLines("australian.dat");
            int nrWiersza = 0;
            string[] wiersz;

            foreach (string line in load)
            {
                wiersz = line.Split(config.separator);
                for (int i = 0; i < wiersz.Length; i++)
                {
                    wiersz[i] = wiersz[i].Replace('.', ',');
                    dane.Rows[nrWiersza][i] = Convert.ToDouble(wiersz[i]);
                }
                nrWiersza++;
            }

            UstawienieDataSetu(daneLiczbowe, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy, false);
            for (int i=0; i<config.iloscKolumn; i++)
            {
                for(int j=0; j<config.iloscWierszy; j++)
                {
                    if (config.rodzajKolumny[i] == "znak")
                        daneLiczbowe.Rows[j][i] = tabZnakiNaLiczby[dane.Rows[j].Field<string>(j)];
                    else
                        daneLiczbowe.Rows[j][i] = dane.Rows[j][i];
                }
            }

            //tworzenie tablic przetrzymujacych minimalne i maksymalne wartosci w kolumnach
            double[] tabMin = new double[config.iloscKolumn];
            double[] tabMax = new double[config.iloscKolumn];

            //ustawianie startowych wartosci dla tablic
            for(int i=0; i<config.iloscKolumn; i++)
            {
                tabMin[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
                tabMax[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
            }

            //wyszukiwanie wartosci min/max dla tablic
            for(int i=0; i<config.iloscKolumn; i++)
            {
                for(int j=1; j<config.iloscWierszy; j++)
                {
                    if (daneLiczbowe.Rows[j].Field<double>(i) < tabMin[i])
                        tabMin[i] = daneLiczbowe.Rows[j].Field<double>(i);
                    if (daneLiczbowe.Rows[j].Field<double>(i) > tabMax[i])
                        tabMax[i] = daneLiczbowe.Rows[j].Field<double>(i);
                }
            }


            UstawienieDataSetu(daneZnormalizowane, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy, false);

            //normalizacja danych
            for(int i=0; i<config.iloscWierszy; i++)
            {
                for(int j=0; j<config.iloscKolumn; j++)
                {
                    daneZnormalizowane.Rows[i][j] = (daneLiczbowe.Rows[i].Field<double>(j) - tabMin[j]) / (tabMax[j] - tabMin[j]);
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
            //Console.WriteLine($"q: {tabZnakiNaLiczby["q"]}");
            System.Console.ReadKey();
        }
    }
}
