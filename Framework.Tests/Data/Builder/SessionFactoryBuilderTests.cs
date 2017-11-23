using System;
using Framework.Environment.ShellBuilder.Models;
using Framework.Tests.Records;
using Xunit;

namespace Framework.Tests.Data.Builder
{
    public class SessionFactoryBuilderTests {
//        [Fact]
//        public void SqlCeSchemaShouldBeGeneratedAndUsable() {
//
//            var recordDescriptors = new[] {
//                new RecordBlueprint {TableName = "Hello", Type = typeof (FooRecord)}
//            };
//
//            ProviderUtilities.RunWithSqlCe(recordDescriptors,
//                sessionFactory => {
//                    var session = sessionFactory.OpenSession();
//                    var foo = new FooRecord { Name = "hi there" };
//                    session.Save(foo);
//                    session.Flush();
//                    session.Close();
//
//                    Assert.NotEqual(foo.Id, default(int));
//
//                });
//
//        }

        [Fact]
        public void SqlServerSchemaShouldBeGeneratedAndUsable() {
            var recordDescriptors = new[] {
                new RecordBlueprint {TableName = "Hello", Type = typeof (FooRecord)}
            };

            ProviderUtilities.RunWithSqlServer(recordDescriptors,
                sessionFactory => {
                    var session = sessionFactory.OpenSession();
                    var foo = new FooRecord { Name = "hi there", Timespan = DateTime.Now};
                    
                     session.Insert(foo);
                    //session.Flush();
                    session.Close();

                    Assert.NotEqual(foo.Id, default(int));

                });
        }
    }
}