using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace FuturePath.Data
{
    public static class DatabaseHelper
    {
        // The database is one file called futurepath.db, created next to the running program.
        private const string ConnectionString = "Data Source=futurepath.db";

        // Opens a connection (a "phone line") to the database file.
        private static SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Tell the database to enforce relationships between tables
            // (for example, you cannot save a programme that belongs to a faculty that does not exist).
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }
            return connection;
        }

        // Builds a command: the SQL text plus its values (@p0, @p1...).
        private static SqliteCommand BuildCommand(SqliteConnection connection, string sql, object[] values)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            for (int i = 0; i < values.Length; i++)
            {
                // A C# null must become DBNull so the database understands "empty".
                command.Parameters.AddWithValue("@p" + i, values[i] ?? DBNull.Value);
            }
            return command;
        }

        // Runs a command that CHANGES data (INSERT, UPDATE, DELETE, CREATE TABLE).
        // Returns how many rows were changed.
        public static int Execute(string sql, params object[] values)
        {
            using (var connection = OpenConnection())
            using (var command = BuildCommand(connection, sql, values))
            {
                return command.ExecuteNonQuery();
            }
        }

        // Runs an INSERT and returns the new row's ID number (e.g. the new LearnerID).
        public static int Insert(string sql, params object[] values)
        {
            using (var connection = OpenConnection())
            {
                using (var command = BuildCommand(connection, sql, values))
                {
                    command.ExecuteNonQuery();
                }
                // Ask the database: "what ID did you just give the new row?"
                using (var idCommand = connection.CreateCommand())
                {
                    idCommand.CommandText = "SELECT last_insert_rowid();";
                    return Convert.ToInt32(idCommand.ExecuteScalar());
                }
            }
        }

        // Runs a SELECT that returns ONE single value (e.g. "how many learners are there?").
        public static object Scalar(string sql, params object[] values)
        {
            using (var connection = OpenConnection())
            using (var command = BuildCommand(connection, sql, values))
            {
                return command.ExecuteScalar();
            }
        }

        // Runs a SELECT that returns a table of rows. The DataTable is like a small spreadsheet in memory.
        public static DataTable Query(string sql, params object[] values)
        {
            var table = new DataTable();
            using (var connection = OpenConnection())
            using (var command = BuildCommand(connection, sql, values))
            using (var reader = command.ExecuteReader())
            {
                // Step 1: create one column in the table for each column in the results.
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    if (table.Columns.Contains(name)) name = name + i; // avoid two columns with the same name
                    table.Columns.Add(name, typeof(object));
                }

                // Step 2: copy every row across, one cell at a time.
                while (reader.Read())
                {
                    DataRow row = table.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i); // empty cell stays empty
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }
    }
}