using Dapper;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using IntegrationTests.TestInfrastructure;
using Reqnroll;
using System.Text;

namespace IntegrationTests.Steps
{
    [Binding]
    internal class UserManagementPostgreSqlSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public UserManagementPostgreSqlSteps(ScenarioContext scenarioContext)
        {
                _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public static async Task BeforeTestRun()
        {
            await IntegrationTestContext.BuildPostgreSqlContainerAsync();

            await IntegrationTestContext.DbConnection.ExecuteAsync(@"
               CREATE TABLE IF NOT EXISTS users  (
                id SERIAL PRIMARY KEY,
                name VARCHAR(50) NOT NULL,
                email VARCHAR(50) NOT NULL UNIQUE
                )");
        }

        [Given("I have a clean PostgreSql database")]
        public async Task GivenIHaveACleanPostgreSqlDatabase()
        {
            await IntegrationTestContext.DbConnection.ExecuteAsync("TRUNCATE TABLE users RESTART IDENTITY");
        }

        [When(@"I create a user with name ""(.*)"" and email ""(.*)""")]
        public async Task WhenICreateAUserWithNameAndEmail(string name, string email)
        {
            var insertedRow = await IntegrationTestContext.DbConnection.ExecuteAsync(
                "INSERT INTO users (name,email) VALUES (@name,@email)",
                new { Name = name, Email = email });

            _scenarioContext.Set(insertedRow, "InsertedRows");
            _scenarioContext.Set(new User { Name = name, Email = email}, "CreatedUser");
        }

        [Then(@"The user should exist in the database")]
        public async Task ThenTheUserShouldExistInTheDatabase()
        {
            var createdUser = _scenarioContext.Get<User>("CreatedUser");
            var count = await IntegrationTestContext.DbConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM users WHERE email = @Email", 
                new { createdUser.Email});

            Assert.That(count, Is.EqualTo(1));
        }

        [Then(@"The user should have the correct details")]
        public async Task ThenTheUserShouldHaveTheCorrectDetails()
        {
            var createdUser = _scenarioContext.Get<User>("CreatedUser");
            var user = await IntegrationTestContext.DbConnection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM users WHERE email = @Email",
                new { createdUser.Email});

            Assert.IsNotNull(user,$"User with email {createdUser.Email} not found");
            Assert.That(user.Name, Is.EqualTo(createdUser.Name));
            Assert.That(user.Email, Is.EqualTo(createdUser.Email));
        }


        [AfterScenario]
        public async Task CleanupDatabase()
        {
            await IntegrationTestContext.DisposePostgreSqlResourcesAsync();
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
