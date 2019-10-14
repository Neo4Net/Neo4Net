/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.store
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Neo4Net.Test;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.getPropertyKeys;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class TestGraphProperties
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private TestGraphDatabaseFactory _factory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _factory = ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(Fs.get()));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void basicProperties()
		 public virtual void BasicProperties()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();
			  PropertyContainer graphProperties = Properties( db );
			  assertThat( graphProperties, inTx( db, not( hasProperty( "test" ) ) ) );

			  Transaction tx = Db.beginTx();
			  graphProperties.SetProperty( "test", "yo" );
			  assertEquals( "yo", graphProperties.GetProperty( "test" ) );
			  tx.Success();
			  tx.Close();
			  assertThat( graphProperties, inTx( db, hasProperty( "test" ).withValue( "yo" ) ) );
			  tx = Db.beginTx();
			  assertNull( graphProperties.RemoveProperty( "something non existent" ) );
			  assertEquals( "yo", graphProperties.RemoveProperty( "test" ) );
			  assertNull( graphProperties.GetProperty( "test", null ) );
			  graphProperties.SetProperty( "other", 10 );
			  assertEquals( 10, graphProperties.GetProperty( "other" ) );
			  graphProperties.SetProperty( "new", "third" );
			  tx.Success();
			  tx.Close();
			  assertThat( graphProperties, inTx( db, not( hasProperty( "test" ) ) ) );
			  assertThat( graphProperties, inTx( db, hasProperty( "other" ).withValue( 10 ) ) );
			  assertThat( getPropertyKeys( db, graphProperties ), containsOnly( "other", "new" ) );

			  tx = Db.beginTx();
			  graphProperties.SetProperty( "rollback", true );
			  assertEquals( true, graphProperties.GetProperty( "rollback" ) );
			  tx.Close();
			  assertThat( graphProperties, inTx( db, not( hasProperty( "rollback" ) ) ) );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNonExistentGraphPropertyWithDefaultValue()
		 public virtual void getNonExistentGraphPropertyWithDefaultValue()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();
			  PropertyContainer graphProperties = Properties( db );
			  Transaction tx = Db.beginTx();
			  assertEquals( "default", graphProperties.GetProperty( "test", "default" ) );
			  tx.Success();
			  tx.Close();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setManyGraphProperties()
		 public virtual void SetManyGraphProperties()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();

			  Transaction tx = Db.beginTx();
			  object[] values = new object[]{ 10, "A string value", new float[]{ 1234.567F, 7654.321F }, "A rather longer string which wouldn't fit inlined #!)(&¤" };
			  int count = 200;
			  for ( int i = 0; i < count; i++ )
			  {
					Properties( db ).setProperty( "key" + i, values[i % values.Length] );
			  }
			  tx.Success();
			  tx.Close();

			  for ( int i = 0; i < count; i++ )
			  {
					assertThat( Properties( db ), inTx( db, hasProperty( "key" + i ).withValue( values[i % values.Length] ) ) );
			  }
			  for ( int i = 0; i < count; i++ )
			  {
					assertThat( Properties( db ), inTx( db, hasProperty( "key" + i ).withValue( values[i % values.Length] ) ) );
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setBigArrayGraphProperty()
		 public virtual void SetBigArrayGraphProperty()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();
			  long[] array = new long[1000];
			  for ( int i = 0; i < 10; i++ )
			  {
					array[array.Length / 10 * i] = i;
			  }
			  string key = "big long array";
			  Transaction tx = Db.beginTx();
			  Properties( db ).setProperty( key, array );
			  assertThat( Properties( db ), hasProperty( key ).withValue( array ) );
			  tx.Success();
			  tx.Close();

			  assertThat( Properties( db ), inTx( db, hasProperty( key ).withValue( array ) ) );
			  assertThat( Properties( db ), inTx( db, hasProperty( key ).withValue( array ) ) );
			  Db.shutdown();
		 }

		 private static PropertyContainer Properties( GraphDatabaseAPI db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( EmbeddedProxySPI ) ).newGraphPropertiesProxy();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void firstRecordOtherThanZeroIfNotFirst()
		 public virtual void FirstRecordOtherThanZeroIfNotFirst()
		 {
			  File storeDir = TestDirectory.databaseDir();
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase( storeDir );
			  Transaction tx = Db.beginTx();
			  Node node = Db.createNode();
			  node.SetProperty( "name", "Yo" );
			  tx.Success();
			  tx.Close();
			  Db.shutdown();

			  db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase( storeDir );
			  tx = Db.beginTx();
			  Properties( db ).setProperty( "test", "something" );
			  tx.Success();
			  tx.Close();
			  Db.shutdown();

			  Config config = Config.defaults();
			  StoreFactory storeFactory = new StoreFactory( TestDirectory.databaseLayout(), config, new DefaultIdGeneratorFactory(Fs.get()), PageCacheRule.getPageCache(Fs.get()), Fs.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  NeoStores neoStores = storeFactory.OpenAllNeoStores();
			  long prop = neoStores.MetaDataStore.GraphNextProp;
			  assertTrue( prop != 0 );
			  neoStores.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void graphPropertiesAreLockedPerTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GraphPropertiesAreLockedPerTx()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();

			  Worker worker1 = new Worker( "W1", new State( db ) );
			  Worker worker2 = new Worker( "W2", new State( db ) );

			  PropertyContainer properties = properties( db );
			  worker1.BeginTx();
			  worker2.BeginTx();

			  string key1 = "name";
			  string value1 = "Value 1";
			  string key2 = "some other property";
			  string value2 = "Value 2";
			  string key3 = "say";
			  string value3 = "hello";
			  worker1.SetProperty( key1, value1 ).get();
			  assertThat( properties, inTx( db, not( hasProperty( key1 ) ) ) );
			  assertFalse( worker2.HasProperty( key1 ) );
			  Future<Void> blockedSetProperty = worker2.SetProperty( key2, value2 );
			  assertThat( properties, inTx( db, not( hasProperty( key1 ) ) ) );
			  assertThat( properties, inTx( db, not( hasProperty( key2 ) ) ) );
			  worker1.SetProperty( key3, value3 ).get();
			  assertFalse( blockedSetProperty.Done );
			  assertThat( properties, inTx( db, not( hasProperty( key1 ) ) ) );
			  assertThat( properties, inTx( db, not( hasProperty( key2 ) ) ) );
			  assertThat( properties, inTx( db, not( hasProperty( key3 ) ) ) );
			  worker1.CommitTx();
			  assertThat( properties, inTx( db, hasProperty( key1 ) ) );
			  assertThat( properties, inTx( db, not( hasProperty( key2 ) ) ) );
			  assertThat( properties, inTx( db, hasProperty( key3 ) ) );
			  blockedSetProperty.get();
			  assertTrue( blockedSetProperty.Done );
			  worker2.CommitTx();
			  assertThat( properties, inTx( db, hasProperty( key1 ).withValue( value1 ) ) );
			  assertThat( properties, inTx( db, hasProperty( key2 ).withValue( value2 ) ) );
			  assertThat( properties, inTx( db, hasProperty( key3 ).withValue( value3 ) ) );

			  worker1.Dispose();
			  worker2.Dispose();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoUncleanInARow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TwoUncleanInARow()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  using ( EphemeralFileSystemAbstraction snapshot = ProduceUncleanStore( Fs.get(), databaseDir ) )
			  {
					using ( EphemeralFileSystemAbstraction snapshot2 = ProduceUncleanStore( snapshot, databaseDir ) )
					{
						 GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setFileSystem(ProduceUncleanStore(snapshot2, databaseDir)).newImpermanentDatabase(databaseDir);
						 assertThat( Properties( db ), inTx( db, hasProperty( "prop" ).withValue( "Some value" ) ) );
						 Db.shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEquals()
		 public virtual void TestEquals()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();
			  PropertyContainer graphProperties = Properties( db );
			  using ( Transaction tx = Db.beginTx() )
			  {
					graphProperties.SetProperty( "test", "test" );
					tx.Success();
			  }

			  assertEquals( graphProperties, Properties( db ) );
			  Db.shutdown();
			  db = ( GraphDatabaseAPI ) _factory.newImpermanentDatabase();
			  assertNotEquals( graphProperties, Properties( db ) );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCreateLongGraphPropertyChainsAndReadTheCorrectNextPointerFromTheStore()
		 public virtual void ShouldBeAbleToCreateLongGraphPropertyChainsAndReadTheCorrectNextPointerFromTheStore()
		 {
			  GraphDatabaseService database = _factory.newImpermanentDatabase();

			  PropertyContainer graphProperties = Properties( ( GraphDatabaseAPI ) database );

			  using ( Transaction tx = database.BeginTx() )
			  {
					graphProperties.SetProperty( "a", new string[]{ "A", "B", "C", "D", "E" } );
					graphProperties.SetProperty( "b", true );
					graphProperties.SetProperty( "c", "C" );
					tx.Success();
			  }

			  using ( Transaction tx = database.BeginTx() )
			  {
					graphProperties.SetProperty( "d", new string[]{ "A", "F" } );
					graphProperties.SetProperty( "e", true );
					graphProperties.SetProperty( "f", "F" );
					tx.Success();
			  }

			  using ( Transaction tx = database.BeginTx() )
			  {
					graphProperties.SetProperty( "g", new string[]{ "F" } );
					graphProperties.SetProperty( "h", false );
					graphProperties.SetProperty( "i", "I" );
					tx.Success();
			  }

			  using ( Transaction tx = database.BeginTx() )
			  {
					assertArrayEquals( new string[]{ "A", "B", "C", "D", "E" }, ( string[] ) graphProperties.GetProperty( "a" ) );
					assertTrue( ( bool ) graphProperties.GetProperty( "b" ) );
					assertEquals( "C", graphProperties.GetProperty( "c" ) );

					assertArrayEquals( new string[]{ "A", "F" }, ( string[] ) graphProperties.GetProperty( "d" ) );
					assertTrue( ( bool ) graphProperties.GetProperty( "e" ) );
					assertEquals( "F", graphProperties.GetProperty( "f" ) );

					assertArrayEquals( new string[]{ "F" }, ( string[] ) graphProperties.GetProperty( "g" ) );
					assertFalse( ( bool ) graphProperties.GetProperty( "h" ) );
					assertEquals( "I", graphProperties.GetProperty( "i" ) );
					tx.Success();
			  }
			  database.Shutdown();
		 }

		 private class State
		 {
			  internal readonly GraphDatabaseAPI Db;
			  internal readonly PropertyContainer Properties;
			  internal Transaction Tx;

			  internal State( GraphDatabaseAPI db )
			  {
					this.Db = db;
					this.Properties = Properties( db );
			  }
		 }

		 private class Worker : OtherThreadExecutor<State>
		 {
			  internal Worker( string name, State initialState ) : base( name, initialState )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasProperty(final String key) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public virtual bool HasProperty( string key )
			  {
					return Execute( StateConflict => StateConflict.properties.hasProperty( key ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void commitTx() throws Exception
			  public virtual void CommitTx()
			  {
					Execute((WorkerCommand<State, Void>) StateConflict =>
					{
					 StateConflict.tx.success();
					 StateConflict.tx.close();
					 return null;
					});
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginTx() throws Exception
			  internal virtual void BeginTx()
			  {
					Execute((WorkerCommand<State, Void>) StateConflict =>
					{
					 StateConflict.tx = StateConflict.db.beginTx();
					 return null;
					});
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: java.util.concurrent.Future<Void> setProperty(final String key, final Object value)
			  internal virtual Future<Void> SetProperty( string key, object value )
			  {
					return ExecuteDontWait(StateConflict =>
					{
					 StateConflict.properties.setProperty( key, value );
					 return null;
					});
			  }
		 }

		 private EphemeralFileSystemAbstraction ProduceUncleanStore( EphemeralFileSystemAbstraction fileSystem, File storeDir )
		 {
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(fileSystem).newImpermanentDatabase(storeDir);
			  Transaction tx = Db.beginTx();
			  Node node = Db.createNode();
			  node.SetProperty( "name", "Something" );
			  Properties( ( GraphDatabaseAPI ) db ).setProperty( "prop", "Some value" );
			  tx.Success();
			  tx.Close();
			  EphemeralFileSystemAbstraction snapshot = fileSystem.Snapshot();
			  Db.shutdown();
			  return snapshot;
		 }
	}

}