using System.Security.Cryptography;
using System.Text;
using Autofac;
using Framework.Environment.Configuration;
using Framework.Security.Providers;
using Framework.Utility.Extensions;
using Xunit;

namespace Framework.Tests.Security
{
   public class DefaultEncryptionServiceTests {
        private IContainer _container;

        
        public void Init() {

            const string encryptionAlgorithm = "AES";
            const string hashAlgorithm = "HMACSHA256";

            var shellSettings = new ShellSettings {
                Name = "Foo",
                DataProvider = "Bar",
                DataConnectionString = "Quux",
                EncryptionAlgorithm = encryptionAlgorithm,
                EncryptionKey = SymmetricAlgorithm.Create(encryptionAlgorithm).Key.ToHexString(),
                HashAlgorithm = hashAlgorithm,
                HashKey = HMAC.Create(hashAlgorithm).Key.ToHexString()
            };

            var builder = new ContainerBuilder();
            builder.RegisterInstance(shellSettings);
            builder.RegisterType<DefaultEncryptionService>().As<IEncryptionService>();
            _container = builder.Build();
        }

       public DefaultEncryptionServiceTests()
       {
           Init();
       }
        [Fact]
        public void CanEncodeAndDecodeData() {
            var encryptionService = _container.Resolve<IEncryptionService>();

            var secretData = Encoding.Unicode.GetBytes("this is secret data");
            var encrypted = encryptionService.Encode(secretData);
            var decrypted = encryptionService.Decode(encrypted);

            Assert.NotEqual(encrypted,  decrypted);
            Assert.Equal(decrypted,  secretData);
        }

        [Fact]
        public void ShouldDetectTamperedData() {
            var encryptionService = _container.Resolve<IEncryptionService>();

            var secretData = Encoding.Unicode.GetBytes("this is secret data");
            var encrypted = encryptionService.Encode(secretData);

            try {
                // tamper the data
                encrypted[encrypted.Length - 1] ^= 66;
                var decrypted = encryptionService.Decode(encrypted);
            }
            catch {
                return;
            }
            Assert.True(false);
        }

        [Fact]
        public void SuccessiveEncodeCallsShouldNotReturnTheSameData() {
            var encryptionService = _container.Resolve<IEncryptionService>();

            var secretData = Encoding.Unicode.GetBytes("this is secret data");
            byte[] previousEncrypted = null;
            for (int i = 0; i < 10; i++) {
                var encrypted = encryptionService.Encode(secretData);
                var decrypted = encryptionService.Decode(encrypted);

                Assert.NotEqual(encrypted,  decrypted);
                Assert.Equal(decrypted,  secretData);

                if(previousEncrypted != null) {
                    Assert.NotEqual(encrypted,  previousEncrypted);
                }
                previousEncrypted = encrypted;
            }
        }
    }
}