using System.Linq;
using Framework.Environment.Configuration;
using Framework.Tests.Stub;
using Moq;
using Xunit;

namespace Framework.Tests.Environment.Configuration
{
    public class DefaultTenantManagerTests {
        private StubAppDataFolder _appDataFolder;

        
        public void Init() {
            var clock = new StubClock();
            _appDataFolder = new StubAppDataFolder(clock);
        }

        public DefaultTenantManagerTests()
        {
            Init();
        }
        [Fact]
        public void SingleSettingsFileShouldComeBackAsExpected() {

            _appDataFolder.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SqlCe\r\nDataConnectionString: something else");

            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var settings = loader.LoadSettings().Single();
            Assert.NotNull(settings);
            Assert.Equal(settings.Name,  ShellSettings.DefaultName);
            Assert.Equal(settings.DataProvider, "SqlCe");
            Assert.Equal(settings.DataConnectionString, "something else");
        }


        [Fact]
        public void MultipleFilesCanBeDetected() {

            _appDataFolder.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SqlCe\r\nDataConnectionString: something else");
            _appDataFolder.CreateFile("Sites\\Another\\Settings.txt", "Name: Another\r\nDataProvider: SqlCe2\r\nDataConnectionString: something else2");
            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var settings = loader.LoadSettings();
            Assert.Equal(settings.Count(), 2);

            var def = settings.Single(x => x.Name == ShellSettings.DefaultName);
            Assert.Equal(def.Name, ShellSettings.DefaultName);
            Assert.Equal(def.DataProvider, "SqlCe");
            Assert.Equal(def.DataConnectionString, "something else");

            var alt = settings.Single(x => x.Name == "Another");
            Assert.Equal(alt.Name, "Another");
            Assert.Equal(alt.DataProvider, "SqlCe2");
            Assert.Equal(alt.DataConnectionString, "something else2");
        }

        [Fact]
        public void NewSettingsCanBeStored() {
            _appDataFolder.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nDataProvider: SqlCe\r\nDataConnectionString: something else");


            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var foo = new ShellSettings {Name = "Foo", DataProvider = "Bar", DataConnectionString = "Quux"};

            Assert.Equal(loader.LoadSettings().Count(), 1);
            loader.SaveSettings(foo);
            Assert.Equal(loader.LoadSettings().Count(), 2);

            var text = _appDataFolder.ReadFile("Sites\\Foo\\Settings.txt");
            Assert.Contains("Foo", text);
            Assert.Contains("Bar", text);
            Assert.Contains("Quux", text);
        }

        [Fact]
        public void CustomSettingsCanBeRetrieved() {
            _appDataFolder.CreateFile("Sites\\Default\\Settings.txt", "Name: Default\r\nProperty1: Foo\r\nProperty2: Bar");

            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            Assert.Equal(loader.LoadSettings().Count(), 1);

            var settings = loader.LoadSettings().First();

            Assert.Equal(settings.Name, "Default");
            Assert.Equal(settings["Property1"], "Foo");
            Assert.Equal(settings["Property2"], "Bar");
        }

        [Fact]
        public void CustomSettingsCanBeStoredAndRetrieved() {
            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var foo = new ShellSettings { Name = "Default" };
            foo["Property1"] = "Foo";
            foo["Property2"] = "Bar";

            loader.SaveSettings(foo);
            Assert.Equal(loader.LoadSettings().Count(), 1);
            var settings = loader.LoadSettings().First();

            Assert.Equal(settings.Name, "Default");
            Assert.Equal(settings["Property1"], "Foo");
            Assert.Equal(settings["Property2"], "Bar");
        }

        [Fact]
        public void EncryptionSettingsAreStoredAndReadable() {
            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var foo = new ShellSettings { Name = "Foo", DataProvider = "Bar", DataConnectionString = "Quux", EncryptionAlgorithm = "AES", EncryptionKey = "ABCDEFG", HashAlgorithm = "HMACSHA256", HashKey = "HIJKLMN" };
            loader.SaveSettings(foo);
            Assert.Equal(loader.LoadSettings().Count(), 1);

            var settings = loader.LoadSettings().First();

            Assert.Equal(settings.EncryptionAlgorithm, "AES");
            Assert.Equal(settings.EncryptionKey, "ABCDEFG");
            Assert.Equal(settings.HashAlgorithm, "HMACSHA256");
            Assert.Equal(settings.HashKey, "HIJKLMN");
        }


        [Fact]
        public void SettingsDontLoseTenantState() {
            IShellSettingsManager loader = new ShellSettingsManager(_appDataFolder, new Mock<IShellSettingsManagerEventHandler>().Object);
            var foo = new ShellSettings { Name = "Default" };
            foo.State = TenantState.Disabled;

            loader.SaveSettings(foo);
            var settings = loader.LoadSettings().First();

            Assert.Equal(settings.Name,  "Default");
            Assert.Equal(settings.State, TenantState.Disabled);
        }
    }
}