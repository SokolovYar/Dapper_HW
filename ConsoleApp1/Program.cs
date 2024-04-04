using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Linq;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;


TeamRepository db = new TeamRepository(@" Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = Football; Integrated Security = SSPI;");

//Поиск команды по названию
Console.WriteLine("Seach team by name");
Team ? myTeam = db.GetTeamByName("Черноморец");
myTeam?.PrintTeam();

//Поиск команды по городу
Console.WriteLine("Seach team by city");
myTeam = db.GetTeamByCity("Париж");
myTeam?.PrintTeam();

//Отображение команды с наибольшим числом забитых голов
Console.WriteLine("Seach team with max GoalsScored");
myTeam = db.GetMaxScored();
myTeam?.PrintTeam();

//Отображение команды с наибольшим числом пропущенных голов
Console.WriteLine("Seach team with max GoalsConceded");
myTeam = db.GetMaxConceded();
myTeam?.PrintTeam();

//Добавление новой команды. Если такая команда уже существует, то добавление не происходит.
//проверка существования происходит по городу И имени команды
myTeam = new Team( "TestName", "TestCity", 0, 0);
db.AddTeam(myTeam);

//изменение команды
Console.WriteLine("Edit team finded by name");
myTeam = new Team("TestName2", "TestCity2", 0, 0);
db.EditTeam("TestName", myTeam);

//удаление команды (по названию и городу)
Console.WriteLine("\n\nDeleting the team");
db.DeleteTeam("TestName2", "TestCity2");



// Модель команды
public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }

    public Team()
    {
        Id = GoalsConceded = GoalsConceded = 0;
        Name = City = string.Empty;
    }

    public Team ( string name, string city, int goalsScored, int goalsConceded)
    {
        Name = name;
        City = city;
        GoalsScored = goalsScored;
        GoalsConceded = goalsConceded;
    }

    public void PrintTeam()
    {
        Console.WriteLine($"Id:             {Id}\n" +
                          $"Name:           {Name}\n" +
                          $"City:           {City}\n" +
                          $"GoalsScored:    {GoalsScored}\n" +
                          $"GoalsConceded:  {GoalsConceded}\n");
    }
}

// Класс для работы с базой данных
public class TeamRepository
{
    private readonly string _connectionString;

    public TeamRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Метод для поиска команды по названию
    public Team GetTeamByName(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Teams WHERE Name = @Name";
            return connection.QueryFirstOrDefault<Team>(query, new { Name = name });
        }
    }

    public Team GetTeamByCity(string city)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Teams WHERE City = @City";
            return connection.QueryFirstOrDefault<Team>(query, new { City = city });
        }
    }

    public Team GetTeamByName_City(string name, string city)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM Teams\r\nWHERE (Teams.Name = @Name AND Teams.City = @City)";
            return connection.QueryFirstOrDefault<Team>(query, new { Name = name, City = city });
        }
    }

    public Team GetMaxScored()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT TOP 1 *  FROM Teams\r\nORDER BY GoalsScored DESC";
            return connection.QueryFirstOrDefault<Team>(query);
        }
    }

    public Team GetMaxConceded()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT TOP 1 *  FROM Teams\r\nORDER BY GoalsConceded DESC";
            return connection.QueryFirstOrDefault<Team>(query);
        }
    }

    public void AddTeam(Team team)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            if (GetTeamByName_City(team.Name, team.City) != null)
            {
                Console.WriteLine("This team is already exist! Add not completed!");
                return;
            }
            string query = "INSERT INTO Teams(Teams.Name, Teams.City, GoalsScored, GoalsConceded)  VALUES (@Name, @City, @GoalsScored, @GoalsConceded);";
            connection.Query(query, new {Name = team.Name, City = team.City, GoalsScored = team.GoalsScored, GoalsConceded = team.GoalsConceded});
        }
    }

    public void EditTeam(string NameToChange, Team team)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "UPDATE Teams SET Name = @Name, City = @City WHERE Name = @OldName";
            connection.Query(query, new { Name = team.Name, City = team.City, OldName = NameToChange });
        }
    }


    public void DeleteTeam(string NameToDelete, string CityToDelete)
    {
        //если нет записи, то выход из метода
        if (GetTeamByName_City(NameToDelete, CityToDelete) == null) return;


            //если пользователь не подтвердил операцию, то выход из метода
            Console.WriteLine($"Team '{NameToDelete}' from {CityToDelete} has finded. Are you sure to delete it?");
            Console.WriteLine("To delete enter 1, and any key to rollback the deleting");
            Int32.TryParse(Console.ReadLine(), out int temp);
            if (temp != 1) return;

                 //сама процедура удаления
                 using (var connection = new SqlConnection(_connectionString))
                 {
                     connection.Open();
                     string query = "DELETE FROM Teams WHERE (Name = @Name AND City = @City);";
                     connection.Query(query, new { Name = NameToDelete, City = CityToDelete });
                 }
    }

}