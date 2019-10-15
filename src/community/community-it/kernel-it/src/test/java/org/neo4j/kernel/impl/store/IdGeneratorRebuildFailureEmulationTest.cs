﻿using System.Collections.Generic;

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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.Graphdb.factory.module.edition.CommunityEditionModule;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ImpermanentGraphDatabase = Neo4Net.Test.ImpermanentGraphDatabase;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class IdGeneratorRebuildFailureEmulationTest
	{
		 private FileSystem _fs;
		 private StoreFactory _factory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();
		 private DatabaseLayout _databaseLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void initialize()
		 public virtual void Initialize()
		 {
			  _fs = new FileSystem();
			  _databaseLayout = TestDirectory.databaseLayout();
			  GraphDatabaseService graphdb = new Database( this, _databaseLayout.databaseDirectory() );
			  CreateInitialData( graphdb );
			  graphdb.Shutdown();
			  IDictionary<string, string> @params = new Dictionary<string, string>();
			  @params[GraphDatabaseSettings.rebuild_idgenerators_fast.name()] = Settings.FALSE;
			  Config config = Config.defaults( @params );
			  _factory = new StoreFactory( _databaseLayout, config, new DefaultIdGeneratorFactory( _fs ), PageCacheRule.getPageCache( _fs ), _fs, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void verifyAndDispose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyAndDispose()
		 {
			  GraphDatabaseService graphdb = null;
			  try
			  {
					graphdb = new Database( this, _databaseLayout.databaseDirectory() );
					VerifyData( graphdb );
			  }
			  finally
			  {
					if ( graphdb != null )
					{
						 graphdb.Shutdown();
					}
					if ( _fs != null )
					{
						 _fs.disposeAndAssertNoOpenFiles();
					}
					_fs = null;
			  }
		 }

		 private void PerformTest( File idFile )
		 {
			  // emulate the need for rebuilding id generators by deleting it
			  _fs.deleteFile( idFile );
			  try
			  {
					  using ( NeoStores neoStores = _factory.openAllNeoStores() )
					  {
						// emulate a failure during rebuild:
					  }
			  }
			  catch ( UnderlyingStorageException expected )
			  {
					assertThat( expected.Message, startsWith( "Id capacity exceeded" ) );
			  }
		 }

		 private void VerifyData( GraphDatabaseService graphdb )
		 {
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					int nodecount = 0;
					foreach ( Node node in graphdb.AllNodes )
					{
						 int propcount = ReadProperties( node );
						 int relcount = 0;
						 foreach ( Relationship rel in node.Relationships )
						 {
							  assertEquals( "all relationships should have 3 properties.", 3, ReadProperties( rel ) );
							  relcount++;
						 }
						 assertEquals( "all created nodes should have 3 properties.", 3, propcount );
						 assertEquals( "all created nodes should have 2 relationships.", 2, relcount );

						 nodecount++;
					}
					assertEquals( "The database should have 2 nodes.", 2, nodecount );
			  }
		 }

		 private void CreateInitialData( GraphDatabaseService graphdb )
		 {
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					Node first = Properties( graphdb.CreateNode() );
					Node other = Properties( graphdb.CreateNode() );
					Properties( first.CreateRelationshipTo( other, RelationshipType.withName( "KNOWS" ) ) );
					Properties( other.CreateRelationshipTo( first, RelationshipType.withName( "DISTRUSTS" ) ) );

					tx.Success();
			  }
		 }

		 private E Properties<E>( E entity ) where E : Neo4Net.Graphdb.PropertyContainer
		 {
			  entity.setProperty( "short thing", "short" );
			  entity.setProperty( "long thing", "this is quite a long string, don't you think, it sure is long enough at least" );
			  entity.setProperty( "string array", new string[]{ "these are a few", "cool strings", "for your viewing pleasure" } );
			  return entity;
		 }

		 private int ReadProperties( PropertyContainer entity )
		 {
			  int count = 0;
			  foreach ( string key in entity.PropertyKeys )
			  {
					entity.GetProperty( key );
					count++;
			  }
			  return count;
		 }

		 private class FileSystem : EphemeralFileSystemAbstraction
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void disposeAndAssertNoOpenFiles() throws Exception
			  internal virtual void DisposeAndAssertNoOpenFiles()
			  {
					AssertNoOpenFiles();
					base.Dispose();
			  }

			  public override void Close()
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") private class Database extends org.neo4j.test.ImpermanentGraphDatabase
		 private class Database : ImpermanentGraphDatabase
		 {
			 private readonly IdGeneratorRebuildFailureEmulationTest _outerInstance;

			  internal Database( IdGeneratorRebuildFailureEmulationTest outerInstance, File storeDir ) : base( storeDir )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void Create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
			  {
					File absoluteStoreDir = storeDir.AbsoluteFile;
					File databasesRoot = absoluteStoreDir.ParentFile;
					@params[GraphDatabaseSettings.active_database.name()] = absoluteStoreDir.Name;
					@params[GraphDatabaseSettings.databases_root_path.name()] = databasesRoot.AbsolutePath;
					new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, storeDir, dependencies )
					.initFacade( databasesRoot, @params, dependencies, this );
			  }

			  private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			  {
				  private readonly Database _outerInstance;

				  private File _storeDir;
				  private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				  public GraphDatabaseFacadeFactoryAnonymousInnerClass( Database outerInstance, DatabaseInfo community, File storeDir, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
				  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._dependencies = dependencies;
				  }

				  protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				  {
						return new ImpermanentPlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
				  }

				  private class ImpermanentPlatformModuleAnonymousInnerClass : ImpermanentPlatformModule
				  {
					  private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

					  public ImpermanentPlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
					  {
						  this.outerInstance = outerInstance;
					  }

					  protected internal override FileSystemAbstraction createFileSystemAbstraction()
					  {
							return outerInstance.outerInstance.outerInstance.fs;
					  }
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore()
		 public virtual void Neostore()
		 {
			  PerformTest( _databaseLayout.idMetadataStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_nodestore_db()
		 public virtual void NeostoreNodestoreDb()
		 {
			  PerformTest( _databaseLayout.idNodeStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_propertystore_db_arrays()
		 public virtual void NeostorePropertystoreDbArrays()
		 {
			  PerformTest( _databaseLayout.idPropertyArrayStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_propertystore_db()
		 public virtual void NeostorePropertystoreDb()
		 {
			  PerformTest( _databaseLayout.idPropertyStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_propertystore_db_index()
		 public virtual void NeostorePropertystoreDbIndex()
		 {
			  PerformTest( _databaseLayout.idPropertyKeyTokenStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_propertystore_db_index_keys()
		 public virtual void NeostorePropertystoreDbIndexKeys()
		 {
			  PerformTest( _databaseLayout.idPropertyKeyTokenNamesStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_propertystore_db_strings()
		 public virtual void NeostorePropertystoreDbStrings()
		 {
			  PerformTest( _databaseLayout.idPropertyStringStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_relationshipstore_db()
		 public virtual void NeostoreRelationshipstoreDb()
		 {
			  PerformTest( _databaseLayout.idRelationshipStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_relationshiptypestore_db()
		 public virtual void NeostoreRelationshiptypestoreDb()
		 {
			  PerformTest( _databaseLayout.idRelationshipTypeTokenStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void neostore_relationshiptypestore_db_names()
		 public virtual void NeostoreRelationshiptypestoreDbNames()
		 {
			  PerformTest( _databaseLayout.idRelationshipTypeTokenNamesStore() );
		 }
	}

}