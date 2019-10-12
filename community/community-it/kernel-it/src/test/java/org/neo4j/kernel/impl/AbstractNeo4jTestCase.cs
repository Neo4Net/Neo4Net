using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using TestRule = org.junit.rules.TestRule;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using Org.Neo4j.Kernel.impl.store;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AbstractNeo4jTestCase.RequiresPersistentGraphDatabase(false) public abstract class AbstractNeo4jTestCase
	public abstract class AbstractNeo4jTestCase
	{
		 [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
		 public class RequiresPersistentGraphDatabase : System.Attribute
		 {
			 private readonly AbstractNeo4jTestCase _outerInstance;

			 public RequiresPersistentGraphDatabase;
			 {
			 }

			  internal bool value;

			 public RequiresPersistentGraphDatabase( public RequiresPersistentGraphDatabase, boolean value = true )
			 {
				 this.RequiresPersistentGraphDatabase = RequiresPersistentGraphDatabase;
				 this.value = value;
			 }
		 }

		 protected internal static readonly File Neo4jBaseDir = new File( "target", "var" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.junit.rules.TestRule START_GRAPHDB = (super, description) ->
		 public static readonly TestRule StartGraphdb = ( @base, description ) =>
		 {
		  TearDownDb();
		  SetupGraphDatabase( description.TestClass.Name, description.TestClass.getAnnotation( typeof( RequiresPersistentGraphDatabase ) ).value() );
		  return @base;
		 };

		 private static ThreadLocal<GraphDatabaseAPI> _threadLocalGraphDb = new ThreadLocal<GraphDatabaseAPI>();
		 private static ThreadLocal<string> _currentTestClassName = new ThreadLocal<string>();
		 private static ThreadLocal<bool> _requiresPersistentGraphDatabase = new ThreadLocal<bool>();

		 private GraphDatabaseAPI _graphDb;

		 private Transaction _tx;

		 protected internal AbstractNeo4jTestCase()
		 {
			  _graphDb = _threadLocalGraphDb.get();
		 }

		 public virtual GraphDatabaseService GraphDb
		 {
			 get
			 {
				  return _graphDb;
			 }
		 }

		 private static void SetupGraphDatabase( string testClassName, bool requiresPersistentGraphDatabase )
		 {
			  AbstractNeo4jTestCase._requiresPersistentGraphDatabase.set( requiresPersistentGraphDatabase );
			  AbstractNeo4jTestCase._currentTestClassName.set( testClassName );
			  if ( requiresPersistentGraphDatabase )
			  {
					try
					{
						 FileUtils.deleteRecursively( GetStorePath( "neo-test" ) );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  _threadLocalGraphDb.set( ( GraphDatabaseAPI )( requiresPersistentGraphDatabase ? ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(GetStorePath("neo-test")) : (new TestGraphDatabaseFactory()).newImpermanentDatabase() ) );
		 }

		 public virtual GraphDatabaseAPI GraphDbAPI
		 {
			 get
			 {
				  return _graphDb;
			 }
		 }

		 protected internal virtual bool RestartGraphDbBetweenTests()
		 {
			  return false;
		 }

		 public virtual Transaction Transaction
		 {
			 get
			 {
				  return _tx;
			 }
			 set
			 {
				  this._tx = value;
			 }
		 }

		 public static File GetStorePath( string endPath )
		 {
			  return ( new File( Neo4jBaseDir, _currentTestClassName.get() + "-" + endPath ) ).AbsoluteFile;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpTest()
		 public virtual void SetUpTest()
		 {
			  if ( RestartGraphDbBetweenTests() && _graphDb == null )
			  {
					SetupGraphDatabase( _currentTestClassName.get(), _requiresPersistentGraphDatabase.get() );
					_graphDb = _threadLocalGraphDb.get();
			  }
			  _tx = _graphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDownTest()
		 public virtual void TearDownTest()
		 {
			  if ( _tx != null )
			  {
					_tx.close();
			  }

			  if ( RestartGraphDbBetweenTests() )
			  {
					TearDownDb();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownDb()
		 public static void TearDownDb()
		 {
			  try
			  {
					if ( _threadLocalGraphDb.get() != null )
					{
						 _threadLocalGraphDb.get().shutdown();
					}
			  }
			  finally
			  {
					_threadLocalGraphDb.remove();
			  }
		 }


		 public virtual Transaction NewTransaction()
		 {
			  if ( _tx != null )
			  {
					_tx.success();
					_tx.close();
			  }
			  _tx = _graphDb.beginTx();
			  return _tx;
		 }

		 public virtual void Commit()
		 {
			  if ( _tx != null )
			  {
					try
					{
						 _tx.success();
						 _tx.close();
					}
					finally
					{
						 _tx = null;
					}
			  }
		 }

		 public virtual void Finish()
		 {
			  if ( _tx != null )
			  {
					try
					{
						 _tx.close();
					}
					finally
					{
						 _tx = null;
					}
			  }
		 }

		 public virtual void Rollback()
		 {
			  if ( _tx != null )
			  {
					try
					{
						 _tx.failure();
						 _tx.close();
					}
					finally
					{
						 _tx = null;
					}
			  }
		 }

		 public virtual IdGenerator GetIdGenerator( IdType idType )
		 {
			  return _graphDb.DependencyResolver.resolveDependency( typeof( IdGeneratorFactory ) ).get( idType );
		 }

		 public static void DeleteFileOrDirectory( string dir )
		 {
			  DeleteFileOrDirectory( new File( dir ) );
		 }

		 public static void DeleteFileOrDirectory( File file )
		 {
			  if ( !file.exists() )
			  {
					return;
			  }

			  if ( file.Directory )
			  {
					foreach ( File child in file.listFiles() )
					{
						 DeleteFileOrDirectory( child );
					}
			  }
			  else
			  {
					file.delete();
			  }
		 }

		 protected internal virtual long PropertyRecordsInUse()
		 {
			  return NumberOfRecordsInUse( PropertyStore() );
		 }

		 public static int NumberOfRecordsInUse<RECORD>( RecordStore<RECORD> store ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  int inUse = 0;
			  for ( long id = store.NumberOfReservedLowIds; id < store.HighId; id++ )
			  {
					RECORD record = store.GetRecord( id, store.NewRecord(), RecordLoad.FORCE );
					if ( record.inUse() )
					{
						 inUse++;
					}
			  }
			  return inUse;
		 }

		 protected internal virtual long DynamicStringRecordsInUse()
		 {
			  return NumberOfRecordsInUse( PropertyStore().StringStore );
		 }

		 protected internal virtual long DynamicArrayRecordsInUse()
		 {
			  return NumberOfRecordsInUse( PropertyStore().ArrayStore );
		 }

		 protected internal virtual PropertyStore PropertyStore()
		 {
			  return _graphDb.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores().PropertyStore;
		 }
	}

}