using System.Globalization;

public static class FieldFormatter
{
    public static string Format(
        string? value,
        string? formatType)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        formatType ??= "number";

        switch (formatType.ToLower())
        {
            case "money_us":
                if (decimal.TryParse(value, out var money_us))
                {
                    return "US$ " + money_us.ToString(
                        "N3",
                        CultureInfo.InvariantCulture);
                }
                break;

            case "money_br":
                if (decimal.TryParse(value, out var money_br))
                {
                    return "R$ " + money_br.ToString(
                        "N3",
                          new CultureInfo("pt-BR"));
                }
                break;

            case "number":
                if (decimal.TryParse(value, out var number))
                {
                    return number.ToString(
                        "0.###",
                        new CultureInfo("pt-BR"));
                }
                break;

            case "percent":
                if (decimal.TryParse(value, out var percent))
                {
                    return percent.ToString(
                        "0.##%",
                        CultureInfo.InvariantCulture);
                }
                break;

            case "integer":
                if (int.TryParse(value, out var integer))
                {
                    return integer.ToString();
                }
                break;

            case "date":
                if (DateTime.TryParse(value, out var date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
                break;

            case "text":
            default:
                return value;

        }

        return value;
    }
}