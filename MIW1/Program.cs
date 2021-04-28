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


            char wczytanyZnak;
            string nazwaSetu;
            string nazwaConfigu;

            //menu
            Console.WriteLine("Witaj drogi uzytkowniku! Wybierz odpowiednia cyfre: ");
            Console.WriteLine("Australian - 1 / BCW - 2 / CRX - 3");
            wczytanyZnak = Console.ReadKey().KeyChar;

            if (wczytanyZnak == '1')
            {
                nazwaSetu = "australian.dat";
                nazwaConfigu = "conAustralian.json";
            }
            else if(wczytanyZnak == '2')
            {
                nazwaSetu = "breast-cancer-wisconsin.data";
                nazwaConfigu = "conBCW.json";
            }
            else if(wczytanyZnak == '3')
            {
                nazwaSetu = "crx.data";
                nazwaConfigu = "conCRX.json";
            }
            else
            {
                Console.WriteLine("Wybrano znak spoza zakresu!!!");
                Console.WriteLine("Ustawiono wartosc domyslna - Australian");
                nazwaSetu = "australian.dat";
                nazwaConfigu = "conAustralian.json";
            }
            Console.WriteLine();



            //wczytanie configa z danymi o secie
            StreamReader fileDane = File.OpenText(nazwaConfigu);
            AustralianConfig config = (AustralianConfig)serializer.Deserialize(fileDane, typeof(AustralianConfig));

            //tworzenie 3 dataTabli: jeden z surowymi danymi (jak w secie), drugi z zamienionymi znakami na liczby, trzeci ze znormalizowanymi danymi
            System.Data.DataTable dane = new DataTable("TablicaDanych");
            System.Data.DataTable daneLiczbowe = new DataTable("TablicaDanychLiczbowych");
            System.Data.DataTable daneZnormalizowane = new DataTable("TablicaDanychZnormalizowanych");

            List<int> indeksyBledow = new List<int>();
            int indeksBledu = 0;

            UstawienieDataSetu(dane, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy, true);

            //zapisywanie danych do dataTable
            string[] load = System.IO.File.ReadAllLines(nazwaSetu);
            int nrWiersza = 0;
            string[] wiersz;

            foreach (string line in load)
            {
                //jezeli jest '?' to linia jest pomijana a jej indeks trafia do listy z bledami
                if (line.Contains('?'))
                {
                    indeksyBledow.Add(indeksBledu);
                    indeksBledu++;
                    continue;
                }

                wiersz = line.Split(config.separator);
                for (int i = 0; i < wiersz.Length; i++)
                {
                    wiersz[i] = wiersz[i].Replace('.', ',');
                    if (config.rodzajKolumny[i] == "znak")
                        dane.Rows[nrWiersza][i] = wiersz[i];
                    else
                        dane.Rows[nrWiersza][i] = Convert.ToDouble(wiersz[i]);
                }
                nrWiersza++;
                indeksBledu++;
            }

            //ustawianie wartosci w danychLiczbowych - ewentualne zamienianie znakow na liczby
            UstawienieDataSetu(daneLiczbowe, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy-indeksyBledow.Count, false);
            for (int i=0; i<config.iloscKolumn; i++)
            {
                for(int j=0; j< config.iloscWierszy - indeksyBledow.Count; j++)
                {
                    if (config.rodzajKolumny[i] == "znak")
                        daneLiczbowe.Rows[j][i] = tabZnakiNaLiczby[dane.Rows[j].Field<string>(i)];
                    else
                        daneLiczbowe.Rows[j][i] = dane.Rows[j][i];
                }
            }

            Console.WriteLine("Czy znormalizowac dane? [y / n]");
            char czyNormalizacja = Console.ReadKey().KeyChar;

            if(czyNormalizacja == 'y')
            {
                //tworzenie tablic przetrzymujacych minimalne i maksymalne wartosci w kolumnach
                double[] tabMin = new double[config.iloscKolumn];
                double[] tabMax = new double[config.iloscKolumn];

                //ustawianie startowych wartosci dla tablic
                for (int i = 0; i < config.iloscKolumn; i++)
                {
                    tabMin[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
                    tabMax[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
                }

                //wyszukiwanie wartosci min/max dla tablic
                for (int i = 0; i < config.iloscKolumn; i++)
                {
                    for (int j = 1; j < config.iloscWierszy - indeksyBledow.Count; j++)
                    {
                        if (daneLiczbowe.Rows[j].Field<double>(i) < tabMin[i])
                            tabMin[i] = daneLiczbowe.Rows[j].Field<double>(i);
                        if (daneLiczbowe.Rows[j].Field<double>(i) > tabMax[i])
                            tabMax[i] = daneLiczbowe.Rows[j].Field<double>(i);
                    }
                }

                UstawienieDataSetu(daneZnormalizowane, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy - indeksyBledow.Count, false);

                //normalizacja danych
                for (int i = 0; i < config.iloscWierszy - indeksyBledow.Count; i++)
                {
                    for (int j = 0; j < config.iloscKolumn; j++)
                    {
                        daneZnormalizowane.Rows[i][j] = (daneLiczbowe.Rows[i].Field<double>(j) - tabMin[j]) / (tabMax[j] - tabMin[j]);
                    }
                }

                Console.WriteLine("\n\nDane znormalizowane: ");
                for (int i = 0; i < daneZnormalizowane.Rows.Count; i++)
                {
                    for (int j = 0; j < daneZnormalizowane.Columns.Count; j++)
                        Console.Write($"{Math.Round(daneZnormalizowane.Rows[i].Field<double>(j), 2)} ");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("\n\nDane liczbowe: \n");
                for(int i=0; i<config.iloscWierszy - indeksyBledow.Count; i++)
                {
                    for(int j=0; j<config.iloscKolumn; j++)
                    {
                        Console.Write($"{Math.Round(daneLiczbowe.Rows[i].Field<double>(j), 2)} ");
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"Ilosc wierszy : {nrWiersza}");
            //Console.WriteLine($"q: {tabZnakiNaLiczby["q"]}");
            Console.WriteLine("Brak wartosci w wierszach: ");
            foreach (int indeks in indeksyBledow)
                Console.Write($"{indeks} ");
            System.Console.ReadKey();
        }
    }
}
