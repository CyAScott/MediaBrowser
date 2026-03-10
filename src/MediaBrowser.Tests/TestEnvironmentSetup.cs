namespace MediaBrowser;

[SetUpFixture]
public class TestEnvironmentSetup
{
    [OneTimeSetUp]
    public void GlobalSetup() =>
        /* Load environment variables from appsettings.Test.json file in the test project directory.
         * This allows us to set configuration settings for tests.
         * The appsettings.Test.json file is ignored by git,
         * so it can be used to set local configuration settings without affecting other developers.
         */
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
}