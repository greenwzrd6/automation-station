namespace AutomationStation.Integration.Tests.Fixtures
{

    /// <summary>
    /// Class <c>DatabaseCollection</c> shows xUnit which of the tests that should use the <c>DatabaseFixture</c>
    /// </summary>
    [CollectionDefinition(Name)]
    public sealed class DatabaseCollection
        : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Integration database";
    }
}
