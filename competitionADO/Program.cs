using Microsoft.Data.SqlClient;
using System.Data;

namespace competitionADO
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=compDb2;Trusted_Connection=True;Encrypt=False";



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Anslutning lyckades!");

                    DeleteCompetitions(connection);
                    AddSampleData(connection);
                    GetAllCompetitions(connection);
                    GetAllCompetitionsWithParticipants(connection);

                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Anslutningsfel: {ex.Message}");
                }
            }
        }

        private static void DeleteCompetitions(SqlConnection connection)
        {

            string deleteParticipantsQuery = "DELETE FROM Participant WHERE CompId IN (SELECT CompId FROM Competitions)";
            using (SqlCommand command = new SqlCommand(deleteParticipantsQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rows deleted in the Participant table.");
            }

            string selectDeleteQuery = "delete FROM Competitions";

            using (SqlCommand command = new SqlCommand(selectDeleteQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rows deleted in the competitions table.");

            }
        }

        private static void GetAllCompetitions(SqlConnection connection)
        {
            string selectCompetitionsQuery = "SELECT * FROM Competitions";


            using (SqlCommand command = new SqlCommand(selectCompetitionsQuery, connection))
            {

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    Console.WriteLine("CompId\tName");
                    Console.WriteLine("------------------");

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Name"]}");
                    }
                }
            }
        }

        private static void GetAllCompetitionsWithParticipants(SqlConnection connection)
        {
            string selectCompetitionsQuery = @"
        SELECT c.CompId, c.Name AS CompetitionName, p.Name AS ParticipantName
        FROM Competitions c
        LEFT JOIN Participant p ON c.CompId = p.CompId
        ORDER BY c.CompId, p.Name
    ";

            using (SqlCommand command = new SqlCommand(selectCompetitionsQuery, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Guid currentCompId = Guid.Empty;
                    bool firstRow = true;

                    while (reader.Read())
                    {
                        Guid compId = reader.GetGuid(reader.GetOrdinal("CompId"));
                        string competitionName = Convert.ToString(reader["CompetitionName"]);
                        string participantName = Convert.ToString(reader["ParticipantName"]);

                        if (compId != currentCompId)
                        {
                            // Skriv ut tävlingsnamnet när vi når en ny tävling
                            if (!firstRow)
                            {
                                Console.WriteLine(); // Tom rad mellan tävlingar
                            }
                            Console.WriteLine($"Tävling: {competitionName}");
                            Console.WriteLine("Deltagare:");

                            currentCompId = compId;
                            firstRow = false;
                        }

                        // Skriv ut deltagarnamn
                        Console.WriteLine($"- {participantName}");
                    }
                }
            }
        }

        private static void AddSampleData(SqlConnection connection)
        {
            string insertCompetitionsQuery = @"
                        INSERT INTO Competitions (Name) VALUES ('E-Sport');
                        INSERT INTO Competitions (Name) VALUES ('Simning');
                        INSERT INTO Competitions (Name) VALUES ('Fotboll');
                    ";

            using (SqlCommand command = new SqlCommand(insertCompetitionsQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rows inserted in the competitions table.");

            }

            Guid? eSportGuid = GetCompetitionGuid(connection, "E-Sport");
            Guid? simningGuid = GetCompetitionGuid(connection, "Simning");
            Guid? fotbollGuid = GetCompetitionGuid(connection, "Fotboll");



            string insertParticipantsQuery = @"
                        INSERT INTO Participant (Name, CompId) VALUES ('Saga', @eSportGuid);
                        INSERT INTO Participant (Name, CompId) VALUES ('Embla',@eSportGuid);
                        INSERT INTO Participant (Name, CompId) VALUES ('Gisle', @fotbollGuid);
                    ";

            using (SqlCommand command = new SqlCommand(insertParticipantsQuery, connection))
            {
                SqlParameter paramESportGuid = new SqlParameter("@ESportGuid", SqlDbType.UniqueIdentifier);
                paramESportGuid.Value = eSportGuid;
                command.Parameters.Add(paramESportGuid);

                SqlParameter paramFotbollGuid = new SqlParameter("@FotbollGuid", SqlDbType.UniqueIdentifier);
                paramFotbollGuid.Value = fotbollGuid;
                command.Parameters.Add(paramFotbollGuid);

                SqlParameter paramSimningGuid = new SqlParameter("@simningGuid", SqlDbType.UniqueIdentifier);
                paramSimningGuid.Value = fotbollGuid;
                command.Parameters.Add(paramSimningGuid);

                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rad(er) infogades i Participant-tabellen.");
            }

        }

        private static Guid? GetCompetitionGuid(SqlConnection connection, string competitionName)
        {
            string selectGuidQuery = "SELECT CompId FROM Competitions WHERE Name = @Name";
            Guid? compId = null;

            using (SqlCommand command = new SqlCommand(selectGuidQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", competitionName);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        compId = reader.GetGuid(reader.GetOrdinal("CompId"));
                    }
                }
            }

            return compId;
        }
    }
}
