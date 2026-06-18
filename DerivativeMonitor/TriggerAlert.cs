using System.Globalization;

public static class TriggerAlert
{
    public static bool ShouldTriggerAlert(
    string parameter,
    string oldValue,
    string newValue)
    {
        // invalid values
        if (!decimal.TryParse(
            oldValue,
            NumberStyles.Any,
            new CultureInfo("pt-BR"),
            out var oldDecimal))
        {
            return false;
        }

        if (!decimal.TryParse(
            newValue,
            NumberStyles.Any,
            new CultureInfo("pt-BR"),
            out var newDecimal))
        {
            return false;
        }

        // avoid division by zero
        if (oldDecimal == 0)
            return false;

        var variation =
            Math.Abs(
                ((newDecimal - oldDecimal)
                / oldDecimal) * 100);

        return variation >= decimal.Parse(parameter, NumberStyles.Any, new CultureInfo("pt-BR"));
    }
}
