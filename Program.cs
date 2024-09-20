using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
public class TransactionHistory
{
    public int Id { get; set; }
    public string Date { get; set; }
    public string ContractName { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}
class Program
{
    static async Task Main(string[] args)
    {
        //Enter your username and password here.
        //If you don't have an account please sign up from this link https://kayit.epias.com.tr/epias-transparency-platform-registration-form

        string username = "username";
        string password = "password";

        string tgtUrl = "https://giris.epias.com.tr/cas/v1/tickets";

        string tgt = await GetTgt(username, password, tgtUrl);

        if (!string.IsNullOrEmpty(tgt))
        {
           
            string apiUrl = "https://seffaflik.epias.com.tr/electricity-service/v1/markets/idm/data/transaction-history";

            // Define date range for API request
            string startDate = "2021-01-01T00:00:00+03:00";
            string endDate = "2021-01-02T00:00:00+03:00";

            await AccessApiWithTgt(apiUrl, tgt, startDate, endDate);
        }
        else
        {
            Console.WriteLine("TGT alınamadı.");
        }
        Console.ReadLine(); 
    }

    static async Task<string> GetTgt(string username, string password, string tgtUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Prepare request body with username and password
            var bodyData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            try
            {
                // Send POST request to get TGT
                HttpResponseMessage response = await client.PostAsync(tgtUrl, bodyData);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseData);
                    string tgtValue = jsonDoc.RootElement.GetProperty("tgt").GetString();
                    Console.WriteLine("TGT alındı: " + tgtValue); 
                    return tgtValue; 
                }
                else
                {
                    Console.WriteLine($"TGT alma isteği başarısız oldu. Hata kodu: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"TGT alma sırasında hata: {e.Message}");
            }
        }
        return null;
    }

    // Accesses the API using the TGT to fetch transaction history for the given date range.
    static async Task AccessApiWithTgt(string apiUrl, string tgt, string startDate, string endDate)
    {
        var transactions = await FetchTransactionHistory(apiUrl, tgt, startDate, endDate);

        if (transactions != null && transactions.Any())
        {
            var groupedTransactions = transactions.GroupBy(t => t.ContractName);

            Console.WriteLine("ContractName | Tarih             | Toplam İşlem Miktarı | Toplam İşlem Tutarı | Ağırlıklı Ortalama Fiyat");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            foreach (var group in groupedTransactions)
            {
                string contractName = group.Key;
                decimal toplamIslemMiktari = group.Sum(x => x.Quantity / 10);
                decimal toplamIslemTutari = group.Sum(x => (x.Price * x.Quantity) / 10);
                decimal agirlikliOrtalamaFiyat = toplamIslemTutari / toplamIslemMiktari;

              
                DateTime tarih = ParseContractNameToDate(contractName);

        
                Console.WriteLine($"{contractName}  | {tarih.ToString("dd/MM/yyyy HH:mm")} | {toplamIslemMiktari,18} | {toplamIslemTutari,18} | {agirlikliOrtalamaFiyat,24}");
            }
        }
        else
        {
            Console.WriteLine("API'den veri alınamadı veya veri bulunamadı.");
        }
    }


    // Fetches transaction history from the API using the provided TGT and date range.
    static async Task<List<TransactionHistory>> FetchTransactionHistory(string apiUrl, string tgt, string startDate, string endDate)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("TGT", tgt);

            // Prepare JSON request body with date range
            var bodyData = new StringContent(
                $"{{\"startDate\":\"{startDate}\",\"endDate\":\"{endDate}\"}}",
                Encoding.UTF8,
                "application/json");

            try
            {
                // Send POST request to fetch transaction history
                HttpResponseMessage response = await client.PostAsync(apiUrl, bodyData);
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    
                    var jsonDoc = JsonDocument.Parse(responseData);
                    var transactionHistoryArray = jsonDoc.RootElement.GetProperty("items");

                    var transactions = new List<TransactionHistory>();

                    // Deserialize each transaction into the TransactionHistory class
                    foreach (var element in transactionHistoryArray.EnumerateArray())
                    {
                        transactions.Add(new TransactionHistory
                        {
                            Id = element.GetProperty("id").GetInt32(),
                            Date = element.GetProperty("date").GetString(),
                            ContractName = element.GetProperty("contractName").GetString(),
                            Price = element.GetProperty("price").GetDecimal(),
                            Quantity = element.GetProperty("quantity").GetDecimal()
                        });
                    }

                    return transactions;
                }
                else
                {
                    Console.WriteLine("API isteği başarısız oldu: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("API isteği sırasında bir hata oluştu: " + ex.Message);
            }
        }

        return null;
    }
    // Parses the contract name to extract the date and time.
    static DateTime ParseContractNameToDate(string contractName)
    {
        // Extract date components from contract name
        string year = "20" + contractName.Substring(2, 2);
        string month = contractName.Substring(4, 2);
        string day = contractName.Substring(6, 2);
        string hour = contractName.Substring(8, 2);

        return new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), 0, 0);
    }
}