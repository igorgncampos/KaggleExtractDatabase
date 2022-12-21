
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using OpenQA.Selenium;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace KaggleExtractDatabase
{


  internal class Program
  {

    public static void Main(string[] args)
    {

      IWebDriver driver = new ChromeDriver();

      DeletarArquivoVelho();

      ////======================== funcao abrir navegador ==========
      driver.Navigate().GoToUrl("https://www.kaggle.com/");
      driver.Manage().Window.Maximize();

      ////========================funcao login pagina==========

      driver.Navigate().GoToUrl("https://www.kaggle.com/");

      Thread.Sleep(500);
      //*[@id="site-container"]/div/div[3]/div[2]/div[2]/div[1]/a/button/span
      Console.WriteLine("click sign in");
      try
      {
        driver.FindElement(By.XPath("//*[@id='site-container']/div/div[3]/div[2]/div[2]/div/a/button")).Click();

      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        //throw;
      }

      Console.WriteLine("click sign finalizado");


      Thread.Sleep(500);

      driver.FindElement(By.XPath("//div[@id='site-container']/div/div[3]/form/div[2]/div/div[2]/a/li/div")).Click();


      Thread.Sleep(500);

      driver.FindElement(By.Name("email")).Click();
      driver.FindElement(By.Name("email")).Clear();
      driver.FindElement(By.Name("email")).SendKeys("igorgncampos@gmail.com");
      driver.FindElement(By.Name("password")).Click();
      driver.FindElement(By.Name("password")).Clear();
      driver.FindElement(By.Name("password")).SendKeys("255255255");
      driver.FindElement(By.XPath("//div[@id='site-container']/div/div[3]/form/div[2]/div[3]/button/span")).Click();

      Thread.Sleep(500);


      ////========================funcao navegar pagina==========

      driver.Navigate().GoToUrl("https://www.kaggle.com/datasets/ahmettalhabektas/argentina-car-prices?resource=download");
      //////https://www.kaggle.com/datasets/swaptr/bitcoin-historical-data


      Thread.Sleep(500);

      driver.FindElement(By.XPath("//*[@id=\"site-content\"]/div/div/div[2]/div[1]/div/a/button/span")).Click();

      WaitDownload();

      driver.Dispose();

      ExtrairZip();


      string csvfilepath = @"C:\Users\igorc\Downloads\argentina_cars.csv";
      DataTable table = LerCsv(csvfilepath);
      InsertDataIntoSqlServerUsingSQLBulkCopy(table);


      Environment.Exit(0);

    }


    static void DeletarArquivoVelho()
    {
      string nomeArquivoZip = @"\archive.zip";
      string nomeArquivoCsv = @"\argentina_cars.csv";
      string nomePastaDownloads = (Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal)) + @"\downloads");

      if (File.Exists(nomePastaDownloads + nomeArquivoCsv))
      {
        File.Delete(nomePastaDownloads + nomeArquivoCsv);
      }
      else
      {
        Console.WriteLine("arquivo nao existe");
      }

      if (File.Exists(nomePastaDownloads + nomeArquivoZip))
      {
        File.Delete(nomePastaDownloads + nomeArquivoZip);
      }
      else
      {
        Console.WriteLine("arquivo nao existe");
      }
    }

    static void WaitDownload()
    {

      string nomeArquivoZip = @"\archive.zip";
      string nomePastaDownloads = (Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal)) + @"\downloads");
      int countloop = 0;

      while (File.Exists(nomePastaDownloads + nomeArquivoZip) == false)
      {
        Thread.Sleep(500);
        countloop++;
        if (File.Exists(nomePastaDownloads + nomeArquivoZip) == true)
        {
          break;
        }
        if (countloop >= 90)
        {
          throw new Exception("Erro ao abaixar arquivo");
        }
      }
    }

    public static void ExtrairZip()
    {
      string caminhoPastaZip = @"C:\Users\igorc\Downloads\archive.zip";
      string caminhoPastaExtraido = @"C:\Users\igorc\Downloads\";
      try
      {
        System.IO.Compression.ZipFile.ExtractToDirectory(caminhoPastaZip, caminhoPastaExtraido);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    public static DataTable LerCsv(string csvfilepath)
    {
      DataTable csvData = new DataTable();

      try
      {
        using (TextFieldParser csvReader = new TextFieldParser(csvfilepath))
        {
          csvReader.SetDelimiters(new string[] { "," });
          csvReader.HasFieldsEnclosedInQuotes = true;
          string[] colFields = csvReader.ReadFields();
          foreach (string column in colFields)
          {
            DataColumn dataColumn = new DataColumn(column);
            dataColumn.AllowDBNull = true;
            csvData.Columns.Add(dataColumn);
          }
          while (!csvReader.EndOfData)
          {
            string[] fieldData = csvReader.ReadFields();
            for (int i = 0; i < fieldData.Length; i++)
            {
              if (fieldData[i] == "")
              {
                fieldData[i] = null;
              }
            }
            csvData.Rows.Add(fieldData);
          }
          csvReader.Close();  
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        //throw;
      }
      return csvData;
    }

    static void InsertDataIntoSqlServerUsingSQLBulkCopy(DataTable csvFileData)
    {
      using (SqlConnection dbconnection = new SqlConnection("server = localhost; Database = Teste;Trusted_connection = True"))
      {
        dbconnection.Open();
        using(SqlBulkCopy s = new SqlBulkCopy(dbconnection))
        {
          s.DestinationTableName = "ArgentinaCars";
          foreach (var column in csvFileData.Columns)
          {
            s.ColumnMappings.Add(column.ToString(),column.ToString());
          }
          s.WriteToServer(csvFileData);
          s.Close();
        }
        dbconnection.Close();
        dbconnection.Dispose();
      }
    }

    //======================================================================================
  }
}




