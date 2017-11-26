using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autofac.Core;
using Framework.Data;
using Framework.Data.Repository;
using Framework.Tests.Records;
using Xunit;

namespace Framework.Tests.Data
{
      public class RepositoryTests : IDisposable {
        #region Setup/Teardown

        
        public void InitFixture() {
        }

        
        public void Init() {
            _databaseFilePath = Path.GetTempFileName();
            //_sessionFactory = DataUtility.CreateSessionFactory(_databaseFilePath, typeof(FooRecord));
          
            
            ProviderUtilities.RunWithSqlServer(null,
                sessionFactory => {
                    
                    _session = sessionFactory.OpenSession();
                    _fooRepos = new Repository<FooRecord>(new TestTransactionManager(_session));

                },true);
        }

       
        public void Term() {
            _session.Close();
        }

       
        public void TermFixture() {
            File.Delete(_databaseFilePath);
        }

        #endregion

          public RepositoryTests()
          {
              Init();
          }

          public void Dispose()
          {
              Term();
              TermFixture();
              ProviderUtilities.ReleaseWithSqlServer();
             
          }
        private IRepository<FooRecord> _fooRepos;
        private ISession _session;
        private string _databaseFilePath;
        private ISessionFactory _sessionFactory;

        private void CreateThreeFoos() {
            _fooRepos.Create(new FooRecord { Name = "one" });
            _fooRepos.Create(new FooRecord { Name = "two" });
            _fooRepos.Create(new FooRecord { Name = "three" });
        }

        [Fact]
        public void GetByIdShouldReturnNullIfValueNotFound() {
            CreateThreeFoos();
            var nofoo = _fooRepos.Get(6655321);
            Assert.Null(nofoo);
        }

//        [Fact]
//        public void GetCanSelectSingleBasedOnFields() {
//            CreateThreeFoos();
//
//            var two = _fooRepos.Get(f => f.Name == "two");
//            Assert.That(two.Name, Is.EqualTo("two"));
//        }
//
//        [Fact]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void GetThatReturnsTwoOrMoreShouldThrowException() {
//            CreateThreeFoos();
//            _fooRepos.Get(f => f.Name == "one" || f.Name == "three");
//        }

//        [Fact]
//        public void GetWithZeroMatchesShouldReturnNull() {
//            CreateThreeFoos();
//            var nofoo = _fooRepos.Get(f => f.Name == "four");
//            Assert.That(nofoo, Is.Null);
//        }

//        [Fact]
//        public void LinqCanBeUsedToSelectObjects() {
//            CreateThreeFoos();
//
//            var foos = from f in _fooRepos.Table
//                       where f.Name == "one" || f.Name == "two"
//                       select f;
//
//            Assert.That(foos.Count(), Is.EqualTo(2));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
//        }

//        [Fact]
//        public void LinqExtensionMethodsCanAlsoBeUsedToSelectObjects() {
//            CreateThreeFoos();
//
//            var foos = _fooRepos.Table
//                .Where(f => f.Name == "one" || f.Name == "two");
//
//            Assert.That(foos.Count(), Is.EqualTo(2));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("one"));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
//        }

//        [Fact]
//        public void OrderShouldControlResults() {
//            CreateThreeFoos();
//
//            var foos = _fooRepos.Fetch(
//                f => f.Name == "two" || f.Name == "three",
//                o => o.Asc(f => f.Name, f => f.Id));
//
//            Assert.That(foos.Count(), Is.EqualTo(2));
//            Assert.That(foos.First().Name, Is.EqualTo("three"));
//            Assert.That(foos.Skip(1).First().Name, Is.EqualTo("two"));
//        }

//        [Fact]
//        public void LinqOrderByCanBeUsedToControlResults() {
//            CreateThreeFoos();
//
//            IEnumerable<FooRecord> foos =
//                        from f in _fooRepos.Table
//                        where f.Name == "two" || f.Name == "three"
//                        orderby f.Name, f.Id ascending
//                        select f;
//
//            Assert.That(foos.Count(), Is.EqualTo(2));
//            Assert.That(foos.First().Name, Is.EqualTo("three"));
//            Assert.That(foos.Skip(1).First().Name, Is.EqualTo("two"));
//        }

//        [Fact]
//        public void RangeShouldSliceResults() {
//            for (var x = 0; x != 40; ++x) {
//                _fooRepos.Create(new FooRecord { Name = x.ToString().PadLeft(8, '0') });
//            }
//
//            var foos = _fooRepos.Fetch(
//                f => f.Name.StartsWith("000"),
//                o => o.Asc(f => f.Name),
//                10, 20);
//
//            Assert.That(foos.Count(), Is.EqualTo(20));
//            Assert.That(foos.First().Name, Is.EqualTo("00000010"));
//            Assert.That(foos.Last().Name, Is.EqualTo("00000029"));
//        }

        [Fact]
        public void RepositoryCanCreateFetchAndDelete() {
            var foo1 = new FooRecord { Name = "yadda" };
            _fooRepos.Create(foo1);

            var foo2 = _fooRepos.Get(foo1.Id);
            foo2.Name = "blah";            

            //TODO query with cache ??
            Assert.Same(foo1, foo2);

            _fooRepos.Delete(foo2);
        }

//        [Fact]
//        public void RepositoryFetchTakesCompoundLambdaPredicate() {
//            CreateThreeFoos();
//
//            var foos = _fooRepos.Fetch(f => f.Name == "three" || f.Name == "two");
//
//            Assert.That(foos.Count(), Is.EqualTo(2));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("two"));
//            Assert.That(foos, Has.Some.Property("Name").EqualTo("three"));
//        }
//
//        [Fact]
//        public void RepositoryFetchTakesSimpleLambdaPredicate() {
//            CreateThreeFoos();
//
//            var one = _fooRepos.Fetch(f => f.Name == "one").Single();
//            var two = _fooRepos.Fetch(f => f.Name == "two").Single();
//
//            Assert.That(one.Name, Is.EqualTo("one"));
//            Assert.That(two.Name, Is.EqualTo("two"));
//        }
//
//        [Fact]
//        public void StoringDateTimeDoesntRemovePrecision() {
//            _fooRepos.Create(new FooRecord { Name = "one", Timespan = DateTime.Parse("2001-01-01 16:28:21.489", CultureInfo.InvariantCulture) });
//
//            var one = _fooRepos.Fetch(f => f.Name == "one").Single();
//
//            Assert.That(one.Name, Is.EqualTo("one"));
//            Assert.That(one.Timespan.Value.Millisecond, Is.EqualTo(489));
//        }
//
//
//        [Fact]
//        public void RepositoryFetchTakesExistsPredicate() {
//            CreateThreeFoos();
//
//            var array = new[] { "one", "two" };
//
//            var result = _fooRepos.Fetch(f => array.Contains(f.Name)).ToList();
//
//            Assert.That(result.Count(), Is.EqualTo(2));
//        }

          
      }
}