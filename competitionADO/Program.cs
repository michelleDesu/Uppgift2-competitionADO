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
                    Console.WriteLine("Connection succeded!\n");

                    DeleteCompetitions(connection);
                    AddSeedData(connection);
                    Console.WriteLine("\n\t* Alla tävlingar med dess deltagare:\n");
                    GetAllCompetitionsWithParticipants(connection);

                    String compName = "E-Sport";
                    Console.WriteLine("\n\t* Exempel på en sökning på ett existerande tävlingsid:\n");
                    getCompByName(connection, compName);

                    Console.WriteLine("\n\t* Exempel på en sökning på ett tävlingsid som inte existerar:\n");
                    getCompByName(connection, "Dans");

                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Connectionerror: {ex.Message}");
                }
            }
        }

        private static void getCompByName(SqlConnection connection, string compName)
        {
            Guid ? compGuid = GetCompetitionGuid(connection, compName);
            if (compGuid.HasValue)
            {
                GetCompetitionById(connection, compGuid.Value);
            }
            else
            {
                Console.WriteLine($"Tävlingen {compName} hittades inte.");
            }
        }

        private static void DeleteCompetitions(SqlConnection connection)
        {

            string deleteParticipantsQuery = "DELETE FROM Participant WHERE CompId IN (SELECT CompId FROM Competitions)";
            using (SqlCommand command = new SqlCommand(deleteParticipantsQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                //Console.WriteLine($"{rowsAffected} rows deleted in the Participant table.");
            }

            string selectDeleteQuery = "delete FROM Competitions";

            using (SqlCommand command = new SqlCommand(selectDeleteQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                //Console.WriteLine($"{rowsAffected} rows deleted in the competitions table.");

            }
        }

        private static void GetAllCompetitions(SqlConnection connection)
        {
            string selectCompetitionsQuery = "SELECT * FROM Competitions";


            using (SqlCommand command = new SqlCommand(selectCompetitionsQuery, connection))
            {

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Name"]}");
                    }
                }
            }
        }

        private static void GetAllCompetitionsWithParticipants(SqlConnection connection)
        {

           // Console.WriteLine("\n\tAlla tävlingar samt dess deltagare:\n");

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
                    bool hasParticipants = false;

                    while (reader.Read())
                    {
                        Guid compId = reader.GetGuid(reader.GetOrdinal("CompId"));
                        string competitionName = Convert.ToString(reader["CompetitionName"]);
                        string participantName = reader.IsDBNull(reader.GetOrdinal("ParticipantName")) ? "Inga deltagare" : Convert.ToString(reader["ParticipantName"]);

                        if (compId != currentCompId)
                        {
                            if (!firstRow)
                            {
                               
                                Console.WriteLine();
                            }
                            Console.WriteLine($"Tävling: {competitionName}");
                            Console.WriteLine("Deltagare:");

                            currentCompId = compId;
                            firstRow = false;
                            hasParticipants = false;
                        }

                        if (participantName != null)
                        {
                            Console.WriteLine($"- {participantName}");
                            hasParticipants = true;
                        }
                    }
                }
            }
        }

        private static void GetCompetitionById(SqlConnection connection, Guid competitionId)
        {
            string selectCompetitionQuery = @"
    SELECT c.CompId, c.Name AS CompetitionName, p.Name AS ParticipantName
    FROM Competitions c
    LEFT JOIN Participant p ON c.CompId = p.CompId
    WHERE c.CompId = @CompetitionId
    ORDER BY p.Name
";

            using (SqlCommand command = new SqlCommand(selectCompetitionQuery, connection))
            {
                command.Parameters.AddWithValue("@CompetitionId", competitionId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    bool firstRow = true;
                    bool hasParticipants = false;

                    while (reader.Read())
                    {
                        string competitionName = Convert.ToString(reader["CompetitionName"]);
                        string participantName = reader.IsDBNull(reader.GetOrdinal("ParticipantName")) ? "Inga deltagare" : Convert.ToString(reader["ParticipantName"]);

                        if (firstRow)
                        {
                            Console.WriteLine($"Tävling: {competitionName}");
                            Console.WriteLine("Deltagare:");
                            firstRow = false;
                        }

                        if (participantName != "Inga deltagare" || hasParticipants)
                        {
                            Console.WriteLine($"- {participantName}");
                            hasParticipants = true;
                        }
                    }

                }
            }
        }

        private static void AddSeedData(SqlConnection connection)
        {
            string insertCompetitionsQuery = @"
                        INSERT INTO Competitions (Name) VALUES ('E-Sport');
                        INSERT INTO Competitions (Name) VALUES ('Simning');
                        INSERT INTO Competitions (Name) VALUES ('Fotboll');
                    ";

            using (SqlCommand command = new SqlCommand(insertCompetitionsQuery, connection))
            {
                int rowsAffected = command.ExecuteNonQuery();
                //Console.WriteLine($"{rowsAffected} rows inserted in the competitions table.");

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
                //Console.WriteLine($"{rowsAffected} rad(er) infogades i Participant-tabellen.");
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
