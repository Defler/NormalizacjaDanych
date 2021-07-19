using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ClosedXML;
using ClosedXML.Excel;

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
        //bool czySurowe dane sluzy do okreslenia czy pracujemy na pierwszym datatable (tam gdzie potrzebne jest ustawienie odpowiedniego rodzaju kolum)
        public static void UstawienieDataSetu(DataTable dane, string[] rodzajKolumny, int iloscKolumn, int iloscWierszy, bool czySuroweDane)
        {
            DataColumn column;
            DataRow row;

            for (int i = 0; i < iloscKolumn - 1; i++)
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
            //ostatnia kolumna jest zawsze stringiem, bo to klasa dec
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = $"kol{iloscKolumn - 1}";
            dane.Columns.Add(column);

            for (int i = 0; i < iloscWierszy; i++)
            {
                row = dane.NewRow();
                dane.Rows.Add(row);
            }
        }

        public static void ZapisDoXXX(DataTable dane)
        {
            var workbook = new XLWorkbook();
            workbook.Worksheets.Add(dane, "zad1a");
            workbook.SaveAs("../../../dane.xlsx");
        }
        public static void ZapisDoJson(DataTable dane)
        {
            var json = JsonConvert.SerializeObject(dane);
            File.WriteAllText("../../../dane.json", json);
        }
        public static void ZapisDoTXXXT(DataTable dane)
        {
            using StreamWriter file = new StreamWriter("../../../dane.txt");
            string nowaLinia;
            for (int i = 0; i < dane.Rows.Count; i++)
            {
                nowaLinia = "";
                for (int j = 0; j < dane.Columns.Count; j++)
                {
                    nowaLinia += dane.Rows[i][j] + " ";
                }
                //nowaLinia += dane.Rows[i].Field<string>(dane.Columns.Count - 1);   
                file.WriteLine(nowaLinia);
            }
        }

        public static void MenuZapisywania(DataTable daneNormalne, DataTable daneZnormalizowane, bool czyPoNormalizacji)
        {
            Console.WriteLine("\nCzy chcesz zapisac dane? [y-tak]");
            if(Console.ReadKey().KeyChar == 'y')
            {
                if (czyPoNormalizacji)
                {
                    char wybor;
                    Console.WriteLine("\nCzy zapisac dane normalne czy znormalizowane? [1 - normalne / 2-znormalizowane / inny znak - anuluj zapis]");
                    wybor = Console.ReadKey().KeyChar;
                    if (wybor == '1')
                    {
                        Console.WriteLine("\nWybierz metode zapisu: [1 - xlsx / 2 - json / 3 - txt / inny znak - anuluj zapis]");
                        wybor = Console.ReadKey().KeyChar;
                        if (wybor == '1')
                            ZapisDoXXX(daneNormalne);
                        else if (wybor == '2')
                            ZapisDoJson(daneNormalne);
                        else if (wybor == '3')
                            ZapisDoTXXXT(daneNormalne);
                    }
                    else if (wybor == '2')
                    {
                        Console.WriteLine("\nWybierz metode zapisu: [1 - xlsx / 2 - json / 3 - txt / inny znak - anuluj zapis]");
                        wybor = Console.ReadKey().KeyChar;
                        if (wybor == '1')
                            ZapisDoXXX(daneZnormalizowane);
                        else if (wybor == '2')
                            ZapisDoJson(daneZnormalizowane);
                        else if (wybor == '3')
                            ZapisDoTXXXT(daneZnormalizowane);
                    }
                }
                else
                {
                    Console.WriteLine("\nWybierz metode zapisu: [1 - xlsx / 2 - json / 3 - txt / inny znak - anuluj zapis]");
                    char wybor = Console.ReadKey().KeyChar;
                    if (wybor == '1')
                        ZapisDoXXX(daneNormalne);
                    else if (wybor == '2')
                        ZapisDoJson(daneNormalne);
                    else if (wybor == '3')
                        ZapisDoTXXXT(daneNormalne);
                }
                

            }
            Console.WriteLine();
        }

        static int Main(string[] args)
        {
            JsonSerializer serializer = new JsonSerializer();

            //wczytanie configa z zamiana znakow na liczby
            StreamReader fileZnaki = File.OpenText("conZnakiNaLiczby.json");
            Dictionary<string, double> tabZnakiNaLiczby = (Dictionary<string, double>)serializer.Deserialize(fileZnaki, typeof(Dictionary<string,double>));


            //wyszukiwanie i wybieranie danych nowa metoda
            DirectoryInfo d = new DirectoryInfo("./");
            FileInfo[] tabDataSety = d.GetFiles("*.data"); //tablica wszystkich plikow .data w folderze


            char nrSetu;
            Console.WriteLine("Wybierz dane na ktorych chcesz pracowac: ");
            for (int i = 0; i < tabDataSety.Length; i++)
                Console.Write($"{tabDataSety[i].Name} [{i}]      ");
            Console.WriteLine();
            nrSetu = Console.ReadKey().KeyChar;
            string nazwaSetu = tabDataSety[Int32.Parse(nrSetu.ToString())].Name; //wyciaganie nazwy wybranego setu
            Console.WriteLine($"\n\nWybrano set: {nazwaSetu}");

            //wyszukiwanie configa zawierajacego nazwe data setu
            string nazwaConfigu="";
            FileInfo[] tabConfigi = d.GetFiles("con*.json");
            for (int i = 0; i < tabConfigi.Length; i++)
                if (tabConfigi[i].Name.Contains(nazwaSetu.Replace(".data", "")))
                    nazwaConfigu = tabConfigi[i].Name;
            if (nazwaConfigu == "")
            {
                Console.WriteLine("Nie istnieje config dla danego setu");
                return 0;
            }
                
            Console.WriteLine($"\nWczytano config: {nazwaConfigu}\n");

            /*
            char wczytanyZnak;
            string nazwaSetu;
            string nazwaConfigu;

            //menu
            Console.WriteLine("Witaj drogi uzytkowniku! Wybierz odpowiednia cyfre: ");
            Console.WriteLine("Australian - 1 / BCW - 2 / CRX - 3");
            wczytanyZnak = Console.ReadKey().KeyChar;

            if (wczytanyZnak == '1')
            {
                nazwaSetu = "australian.data";
                nazwaConfigu = "con_australian.json";
            }
            else if(wczytanyZnak == '2')
            {
                nazwaSetu = "bcw.data";
                nazwaConfigu = "con_bcw.json";
            }
            else if(wczytanyZnak == '3')
            {
                nazwaSetu = "crx.data";
                nazwaConfigu = "con_crx.json";
            }
            else
            {
                Console.WriteLine("Wybrano znak spoza zakresu!!!");
                Console.WriteLine("Ustawiono wartosc domyslna - Australian");
                nazwaSetu = "australian.data";
                nazwaConfigu = "conAustralian.json";
            }
            Console.WriteLine();
            */


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
            UstawienieDataSetu(daneLiczbowe, config.rodzajKolumny, config.iloscKolumn, config.iloscWierszy - indeksyBledow.Count, false);
            for (int i = 0; i < config.iloscKolumn; i++)
            {
                for (int j = 0; j < config.iloscWierszy - indeksyBledow.Count; j++)
                {
                    if (config.rodzajKolumny[i] == "znak")
                        if (i == config.iloscKolumn - 1)
                            daneLiczbowe.Rows[j][i] = dane.Rows[j][i];
                        else
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
                double[] tabMin = new double[config.iloscKolumn-1];
                double[] tabMax = new double[config.iloscKolumn-1];

                //ustawianie startowych wartosci dla tablic
                for (int i = 0; i < config.iloscKolumn-1; i++)
                {
                    tabMin[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
                    tabMax[i] = Convert.ToDouble(daneLiczbowe.Rows[0][i]);
                }

                //wyszukiwanie wartosci min/max dla tablic oprocz ostatniej kolumny
                for (int i = 0; i < config.iloscKolumn-1; i++)
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
                        if (j == config.iloscKolumn - 1)
                            daneZnormalizowane.Rows[i][j] = daneLiczbowe.Rows[i][j];
                        else
                            daneZnormalizowane.Rows[i][j] = (daneLiczbowe.Rows[i].Field<double>(j) - tabMin[j]) / (tabMax[j] - tabMin[j]);
                    }
                }

                Console.WriteLine("\n\nDane znormalizowane: ");
                for (int i = 0; i < daneZnormalizowane.Rows.Count; i++)
                {
                    for (int j = 0; j < daneZnormalizowane.Columns.Count; j++)
                    {
                        if (j == daneZnormalizowane.Columns.Count - 1)
                            Console.Write($"{daneZnormalizowane.Rows[i].Field<string>(j)} ");
                        else
                            Console.Write($"{Math.Round(daneZnormalizowane.Rows[i].Field<double>(j), 2)} ");
                    }   
                    Console.WriteLine();
                }
                MenuZapisywania(dane, daneZnormalizowane, true);
            }
            else
            {
                Console.WriteLine("\n\nDane wczytane: \n");
                for(int i=0; i<config.iloscWierszy - indeksyBledow.Count; i++)
                {
                    for(int j=0; j<config.iloscKolumn; j++)
                    {
                        Console.Write($"{dane.Rows[i][j]} ");
                    }
                    Console.WriteLine();
                }
                MenuZapisywania(dane, daneZnormalizowane, false);
            }

            Console.WriteLine();
            Console.WriteLine($"Ilosc wierszy : {nrWiersza}");
            //Console.WriteLine($"q: {tabZnakiNaLiczby["q"]}");
            Console.WriteLine("Brak wartosci w wierszach: ");
            foreach (int indeks in indeksyBledow)
                Console.Write($"{indeks} ");
            Console.WriteLine("\nKoniec przedstawienia!");
            return 0;
        }
    }
}
