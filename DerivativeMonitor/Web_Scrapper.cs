using HtmlAgilityPack;
using System.Globalization;
using System.Net.Http;

public static class Web_Scrapper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<string> GetData(string URlRequest)
    {

        try
        {
            Logger.Log("Sending HTTP GET request to URL: " + URlRequest);
            HttpResponseMessage response = await _httpClient.GetAsync(URlRequest);
            response.EnsureSuccessStatusCode();
            Logger.Log("Received successful response from URL: " + URlRequest);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data for ticker: {ex.Message}");
            return "";
        }
    }

    public static string ExtractTags(string HTML, string tag)
    {
        int start = HTML.IndexOf($"<{tag}>");
        int end = HTML.IndexOf($"</{tag}>");

        if (start < 0 || end < 0)
        {
            return string.Empty;
        }

        start += start + $"{tag}".Length; // jumping to the end of the opening tag
        return HTML.Substring(start, end - start);
    }
    public static List<List<string>> ExtractTableById(string html, string tableId)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        // Select table by ID
        var table = doc.DocumentNode.SelectSingleNode($"//table[@id='{tableId}']");

        if (table == null)
            return new List<List<string>>();

        var result = new List<List<string>>();

        // Get all rows
        var rows = table.SelectNodes(".//tr");

        if (rows == null)
            return result;

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("th|td");

            if (cells == null)
                continue;

            var rowData = new List<string>();

            foreach (var cell in cells)
            {
                rowData.Add(cell.InnerText.Trim());
            }

            result.Add(rowData);
        }

        return result;
    }

    public static List<OptionData> MapToOptions(List<List<string>> table)
    {
        var result = new List<OptionData>();

        foreach (var row in table.Skip(1)) // skip header
        {
            var option = new OptionData
            {
                ISIN = row[0],
                Especificacao = row[1],
                Codigo = row[2],
                Vencimento = DateTime.Parse(row[3]),
                PrecoExercicio = decimal.Parse(row[4], new CultureInfo("pt-BR")),
                Referencia = row[5],
                Protegida = row[6].Trim().ToUpper() == "SIM",
                Estilo = row[7]
            };

            result.Add(option);
        }

        return result;
    }

}
