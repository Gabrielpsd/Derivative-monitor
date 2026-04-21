using Microsoft.Data.Sqlite;

public static class DataBaseManager
{

    public static void Initialize(string source)
    {

        using var connection = new SqliteConnection($"Data Source={source}");
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Options (
            Ticket TEXT NOT NULL,
            Codigo TEXT NOT NULL,
            ISIN TEXT,
            Especificacao TEXT,
            Vencimento TEXT,
            PrecoExercicio REAL,
            Referencia TEXT,
            Protegida INTEGER,
            Estilo TEXT,
            PRIMARY KEY (Ticket, Codigo)
        );

        CREATE INDEX IF NOT EXISTS idx_codigo ON Options(Codigo);
        ";

        command.ExecuteNonQuery();
    }

    public static void SaveOptions(string source,string ticket, Options options)
    {

        using var connection = new SqliteConnection($"Data Source={source}");
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var allOptions = new List<OptionData>();

        if (options.CallOptions != null)
            allOptions.AddRange(options.CallOptions);

        if (options.PutOptions != null)
            allOptions.AddRange(options.PutOptions);

        // UPSERT
        var cmd = connection.CreateCommand();

        cmd.Transaction = transaction;

        cmd.CommandText = @"
            INSERT INTO Options 
            (Ticket, Codigo, ISIN, Especificacao, Vencimento, PrecoExercicio, Referencia, Protegida, Estilo)
            VALUES 
            ($ticket, $codigo, $isin, $esp, $venc, $preco, $ref, $prot, $estilo)
            ON CONFLICT(Ticket, Codigo) DO UPDATE SET
                ISIN = excluded.ISIN,
                Especificacao = excluded.Especificacao,
                Vencimento = excluded.Vencimento,
                PrecoExercicio = excluded.PrecoExercicio,
                Referencia = excluded.Referencia,
                Protegida = excluded.Protegida,
                Estilo = excluded.Estilo;
            ";

        foreach (var opt in allOptions)
        {
            cmd.Parameters.Clear();

            cmd.Parameters.AddWithValue("$ticket", ticket);
            cmd.Parameters.AddWithValue("$codigo", opt.Codigo);
            cmd.Parameters.AddWithValue("$isin", opt.ISIN);
            cmd.Parameters.AddWithValue("$esp", opt.Especificacao);
            cmd.Parameters.AddWithValue("$venc", opt.Vencimento.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$preco", opt.PrecoExercicio);
            cmd.Parameters.AddWithValue("$ref", opt.Referencia);
            cmd.Parameters.AddWithValue("$prot", opt.Protegida ? 1 : 0);
            cmd.Parameters.AddWithValue("$estilo", opt.Estilo);

            cmd.ExecuteNonQuery();
        }

        // DELETE missing
        if (allOptions.Count > 0)
        {
            var deleteCmd = connection.CreateCommand();
            deleteCmd.Transaction = transaction;

            var parameters = allOptions.Select((o, i) => $"$c{i}").ToList();

            deleteCmd.CommandText = $@"
            DELETE FROM Options
            WHERE Ticket = $ticket
            AND Codigo NOT IN ({string.Join(",", parameters)})
        ";

            deleteCmd.Parameters.AddWithValue("$ticket", ticket);

            for (int i = 0; i < allOptions.Count; i++)
            {
                deleteCmd.Parameters.AddWithValue($"$c{i}", allOptions[i].Codigo);
            }

            deleteCmd.ExecuteNonQuery();
        }
        else
        {

            // If no data returned → remove everything for that ticket
            var deleteAllCmd = connection.CreateCommand();
            deleteAllCmd.Transaction = transaction;

            deleteAllCmd.CommandText = "DELETE FROM Options WHERE Ticket = $ticket";
            deleteAllCmd.Parameters.AddWithValue("$ticket", ticket);

            deleteAllCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}