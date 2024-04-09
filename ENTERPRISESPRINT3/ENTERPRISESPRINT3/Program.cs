//Install-Package CsvHelper no terminal
using CsvHelper;
//Install-Package HtmlAgilityPack no terminal
using HtmlAgilityPack;
using System;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
//Install-Package Newtonsoft.Json no terminal
using Newtonsoft.Json.Linq;



namespace SP3FIAP
{
    class Program
    {

        private const string NomeProduto = "//*[@id=\"wrapper\"]/main/div[1]/div/div[2]/div/div/h1";
        private const string PrecoProduto = "//*[@id=\"wrapper\"]/main/div[1]/div/div[2]/div/div/div[1]/div/div";
        private const string DescricaoProduto = "//*[@id=\"wrapper\"]/main/div[1]/div/div[2]/div/div/div[2]/p";

        static async Task Main(string[] args)
        {

            Console.WriteLine("Digite a URL do produto na Cienalab: ");
            string url = Console.ReadLine();

            if (!IsValidCienalabUrl(url))
            {
                Console.WriteLine("URL inválida. Verifique se você colou a URL correta da Cienalab.");
                return;
            }

            try
            {
                string[] productInfo = await ScrapeCienalabProduct(url);

                await WriteToCsv(productInfo, "cienalab_product.csv");

                Console.WriteLine("Dados do produto foram salvos em cienalab_product.csv");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao fazer a requisição HTTP: {ex.Message}");
            }
            catch (HtmlWebException ex)
            {
                Console.WriteLine($"Erro ao fazer a requisição HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }


        }

        static bool IsValidCienalabUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps) &&
                result.Host.ToLower().Contains("cienalab.com.br");
        }

        static async Task<string[]> ScrapeCienalabProduct(string url)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            string html = await client.GetStringAsync(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var text = document.DocumentNode.SelectNodes("//script");
            string filteredString = string.Empty;
            string jsonString = text[10].InnerText;
            // Encontra o índice da string "price"
            int index = jsonString.IndexOf("\"price\":219.0");

            // Verifica se a string "price" com o valor 219.0 foi encontrada
            if (index != -1)
                // Extrai a substring da string original a partir do índice encontrado
                 filteredString = jsonString.Substring(index);
            else
                Console.WriteLine($"Could not find {jsonString}");

            string JsonString = "{\"@context\":\"http://schema.org/\",\"@type\":\"Product\",\"url\":\"/produto/le-scimmie-heavy-tee-77\",\"name\":\"Le Scimmie Heavy Tee\",\"description\":\" Confeccionada em Suedine preto 100% algodão 220GSM (alta gramatura) na modelagem CIENA, nossa t-shirt conta com estampa frontal em silk. \\n. \\nConsidere um encolhimento natural de 5%, consultar tabela de medidas. \\n\",\"brand\":{\"@type\":\"Brand\",\"name\":\"CIENA\"},\"gtin0\":\"\",\"sku\":\"CNA8-1-1PT2P\",\"image\":\"https://cdn.vnda.com.br/cienalab/2023/12/10/15_03_20_144_15_12_4_423_prancheta201.jpg?v=1702231403\",\"offers\":[{\"@type\":\"Offer\",\"sku\":\"CNA8-1-1PT2P\",\"gtin\":\"\",\"itemCondition\":\"http://schema.org/NewCondition\",\"availability\":\"http://schema.org/InStock\",\"price\":219.0,\"priceCurrency\":\"BRL\"},{\"@type\":\"Offer\",\"sku\":\"CNA8-1-1PT2M\",\"gtin\":\"\",\"itemCondition\":\"http://schema.org/NewCondition\",\"availability\":\"http://schema.org/InStock\",\"price\":219.0,\"priceCurrency\":\"BRL\"},{\"@type\":\"Offer\",\"sku\":\"CNA8-1-1PT2G\",\"gtin\":\"\",\"itemCondition\":\"http://schema.org/NewCondition\",\"availability\":\"http://schema.org/InStock\",\"price\":219.0,\"priceCurrency\":\"BRL\"},{\"@type\":\"Offer\",\"sku\":\"CNA8-1-1PT2GG\",\"gtin\":\"\",\"itemCondition\":\"http://schema.org/NewCondition\",\"availability\":\"http://schema.org/InStock\",\"price\":219.0,\"priceCurrency\":\"BRL\"}]}";

            // Converte a string JSON em um objeto JObject
            JObject jsonObject = JObject.Parse(jsonString);

            // Obtém a matriz de ofertas do objeto
            JArray offers = (JArray)jsonObject["offers"];

            // Itera sobre cada oferta e extrai o preço
            foreach (JToken offer in offers)
            {
                double price = (double)offer["price"];
                Console.WriteLine("Price: " + price);
            }



            string productName = ObterTexto(document, NomeProduto);
            string productPriceRaw = ObterTexto(document, PrecoProduto);
            string productPrice = productPriceRaw.Replace("R$&nbsp;", "").Trim(); // Remover "R$&nbsp;" do preço
            string productDescription = ObterTexto(document, DescricaoProduto);

            return new string[] { productName, productPrice, productDescription };
        }

        static string ObterTexto(HtmlDocument document, string xPath)
        {
            var element = document.DocumentNode.SelectSingleNode(xPath);
            return element?.InnerText?.Trim() ?? "Não encontrado";
        }

        static async Task WriteToCsv(string[] data, string filePath)
        {
            await using var writer = new StreamWriter(filePath);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecord(new { Nome = data[0], Preço = data[1], Descrição = data[2] });
        }

    }
}
