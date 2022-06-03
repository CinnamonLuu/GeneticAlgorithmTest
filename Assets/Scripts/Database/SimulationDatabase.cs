using Mono.Data.Sqlite;
using UnityEngine;

public class SimulationDatabase : MonoBehaviour
{
    private static string dbName = "URI=file:Simulation.db";

    private void Start()
    {
        CreateDB();
    }

    private void CreateDB()
    {
        using (SqliteConnection connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys=on;";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS iterations (iterationID INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "distanceType VARCHAR(255) CHECK(distanceType = 'Euclidean' OR distanceType = 'Manhattan' OR distanceType = 'Chebyshev')," +
                    "currentIteration INTEGER," +
                    "successRatio INTEGER," +
                    "numSuccessfulAgents INTEGER," +
                    "numCrashedAgents INTEGER" +
                    ");";

                command.ExecuteNonQuery();

                command.CommandText = "CREATE TABLE IF NOT EXISTS simulations (simulationID INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "distanceType VARCHAR(255) CHECK(distanceType = 'Euclidean' OR distanceType = 'Manhattan' OR distanceType = 'Chebyshev')," +
                    "startingNumAgents INTEGER," +
                    "elitism INTEGER," +
                    "cutoff REAL," +
                    "mutationChance REAL," +
                    "parentMutationWeight REAL CHECK(parentMutationWeight >= 0 OR parentMutationWeight <= 1)," +
                    "usesPoisson BOOLEAN NOT NULL CHECK (usesPoisson IN (0, 1))," +
                    "firstSuccessfulIteration INTEGER," +
                    "FOREIGN KEY (firstSuccessfulIteration) REFERENCES iterations(iterationID)" +
                    ");";
                command.ExecuteNonQuery();
            }
        }
    }

    public static void AddSimulation(TypeOfDistance distanceType, int startingNumAgents, int elitism, float cutoff, float mutationChance, float parentMutationWeight, bool usesPoisson, int iterationID)
    {
        using (SqliteConnection connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                string typeName = "";
                switch (distanceType)
                {
                    case TypeOfDistance.Manhattan:
                        typeName = "Manhattan";
                        break;

                    case TypeOfDistance.Euclidean:
                        typeName = "Euclidean";
                        break;

                    case TypeOfDistance.Chebyshev:
                        typeName = "Chebyshev";
                        break;
                }

                command.CommandText = "INSERT INTO simulations (distanceType, startingNumAgents, elitism, cutoff, mutationChance, parentMutationWeight, usesPoisson, firstSuccessfulIteration)" +
                    "VALUES ('" + typeName + "', '" + startingNumAgents + "', '" + elitism + "', '" + cutoff + "', '" + mutationChance + "', '" + parentMutationWeight + "', '" + (usesPoisson ? 1 : 0) + "', '" + iterationID + "');";

                command.ExecuteNonQuery();
            }
        }
    }

    public static void AddIteration(TypeOfDistance distanceType, int currentIteration, int successRatio, int numSuccessfulAgents, int numCrashedAgents)
    {
        using (SqliteConnection connection = new SqliteConnection(dbName))
        {
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                string typeName = "";
                switch (distanceType)
                {
                    case TypeOfDistance.Manhattan:
                        typeName = "Manhattan";
                        break;

                    case TypeOfDistance.Euclidean:
                        typeName = "Euclidean";
                        break;

                    case TypeOfDistance.Chebyshev:
                        typeName = "Chebyshev";
                        break;
                }
                command.CommandText = "INSERT INTO iterations (distanceType, currentIteration, successRatio, numSuccessfulAgents, numCrashedAgents)" +
                    "VALUES ('" + typeName + "', '" + currentIteration + "', '" + successRatio + "', '" + numSuccessfulAgents + "', '" + numCrashedAgents + "');";

                command.ExecuteNonQuery();
            }
        }
    }
}