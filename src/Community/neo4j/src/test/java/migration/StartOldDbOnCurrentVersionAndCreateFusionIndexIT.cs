using System;
using System.Collections.Generic;

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
namespace Migration
{
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using SchemaRead = Neo4Net.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using ZipUtils = Neo4Net.Io.compress.ZipUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using LuceneIndexProviderFactory = Neo4Net.Kernel.Api.Impl.Schema.LuceneIndexProviderFactory;
	using NativeLuceneFusionIndexProviderFactory10 = Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory10;
	using NativeLuceneFusionIndexProviderFactory20 = Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory20;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GenericNativeIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.GenericNativeIndexProvider;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.concat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Unzip.unzip;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class StartOldDbOnCurrentVersionAndCreateFusionIndexIT
	internal class StartOldDbOnCurrentVersionAndCreateFusionIndexIT
	{
		 private const string ZIP_FILE_3_2 = "3_2-db.zip";
		 private const string ZIP_FILE_3_3 = "3_3-db.zip";
		 private const string ZIP_FILE_3_4 = "3_4-db.zip";

		 private const string KEY1 = "key1";
		 private const string KEY2 = "key2";

		 private sealed class Provider
		 {
			  // in order of appearance
			  public static readonly Provider Lucene_10 = new Provider( "Lucene_10", InnerEnum.Lucene_10, "Label1", Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex.Lucene10, Neo4Net.Kernel.Api.Impl.Schema.LuceneIndexProviderFactory.ProviderDescriptor );
			  public static readonly Provider Fusion_10 = new Provider( "Fusion_10", InnerEnum.Fusion_10, "Label2", Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex.Native10, Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory10.Descriptor );
			  public static readonly Provider Fusion_20 = new Provider( "Fusion_20", InnerEnum.Fusion_20, "Label3", Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex.Native20, Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory20.Descriptor );
			  public static readonly Provider Btree_10 = new Provider( "Btree_10", InnerEnum.Btree_10, "Label4", Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex.NativeBtree10, Neo4Net.Kernel.Impl.Index.Schema.GenericNativeIndexProvider.Descriptor );

			  private static readonly IList<Provider> valueList = new List<Provider>();

			  static Provider()
			  {
				  valueList.Add( Lucene_10 );
				  valueList.Add( Fusion_10 );
				  valueList.Add( Fusion_20 );
				  valueList.Add( Btree_10 );
			  }

			  public enum InnerEnum
			  {
				  Lucene_10,
				  Fusion_10,
				  Fusion_20,
				  Btree_10
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;

			  internal Provider( string name, InnerEnum innerEnum, string labelName, Neo4Net.Graphdb.factory.GraphDatabaseSettings.SchemaIndex setting, Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor descriptor )
			  {
					this._label = Label.label( labelName );
					this._setting = setting;
					this._descriptor = descriptor;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<Provider> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Provider valueOf( string name )
			 {
				 foreach ( Provider enumInstance in Provider.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private const Provider DEFAULT_PROVIDER = Provider.Btree_10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("Here as reference for how 3.2 db was created") @Test void create3_2Database() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Create3_2Database()
		 {
			  File storeDir = TempStoreDirectory();
			  GraphDatabaseService db = ( new GraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  CreateIndexData( db, Provider.Lucene_10.label );
			  Db.shutdown();

			  File zipFile = new File( storeDir.ParentFile, storeDir.Name + ".zip" );
			  ZipUtils.zip( new DefaultFileSystemAbstraction(), storeDir, zipFile );
			  Console.WriteLine( "Db created in " + zipFile.AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("Here as reference for how 3.3 db was created") @Test void create3_3Database() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Create3_3Database()
		 {
			  File storeDir = TempStoreDirectory();
			  GraphDatabaseFactory factory = new GraphDatabaseFactory();
			  GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( storeDir );

			  builder.SetConfig( GraphDatabaseSettings.enable_native_schema_index, Settings.FALSE );
			  GraphDatabaseService db = builder.NewGraphDatabase();
			  CreateIndexData( db, Provider.Lucene_10.label );
			  Db.shutdown();

			  builder.SetConfig( GraphDatabaseSettings.enable_native_schema_index, Settings.TRUE );
			  db = builder.NewGraphDatabase();
			  CreateIndexData( db, Provider.Fusion_10.label );
			  Db.shutdown();

			  File zipFile = new File( storeDir.ParentFile, storeDir.Name + ".zip" );
			  ZipUtils.zip( new DefaultFileSystemAbstraction(), storeDir, zipFile );
			  Console.WriteLine( "Db created in " + zipFile.AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("Here as reference for how 3.4 db was created") @Test void create3_4Database() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void Create3_4Database()
		 {
			  File storeDir = TempStoreDirectory();
			  GraphDatabaseFactory factory = new GraphDatabaseFactory();
			  GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( storeDir );

			  CreateIndexDataAndShutdown( builder, GraphDatabaseSettings.SchemaIndex.LUCENE10.providerName(), Provider.Lucene_10.label );
			  CreateIndexDataAndShutdown( builder, GraphDatabaseSettings.SchemaIndex.NATIVE10.providerName(), Provider.Fusion_10.label );
			  CreateIndexDataAndShutdown( builder, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName(), Provider.Fusion_20.label );

			  File zipFile = new File( storeDir.ParentFile, storeDir.Name + ".zip" );
			  ZipUtils.zip( new DefaultFileSystemAbstraction(), storeDir, zipFile );
			  Console.WriteLine( "Db created in " + zipFile.AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOpen3_2DbAndCreateAndWorkWithSomeFusionIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldOpen3_2DbAndCreateAndWorkWithSomeFusionIndexes()
		 {
			  ShouldOpenOldDbAndCreateAndWorkWithSomeFusionIndexes( ZIP_FILE_3_2, Provider.Lucene_10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOpen3_3DbAndCreateAndWorkWithSomeFusionIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldOpen3_3DbAndCreateAndWorkWithSomeFusionIndexes()
		 {
			  ShouldOpenOldDbAndCreateAndWorkWithSomeFusionIndexes( ZIP_FILE_3_3, Provider.Fusion_10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOpen3_4DbAndCreateAndWorkWithSomeFusionIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldOpen3_4DbAndCreateAndWorkWithSomeFusionIndexes()
		 {
			  ShouldOpenOldDbAndCreateAndWorkWithSomeFusionIndexes( ZIP_FILE_3_4, Provider.Fusion_20 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldOpenOldDbAndCreateAndWorkWithSomeFusionIndexes(String zippedDbName, Provider highestProviderInOldVersion) throws Exception
		 private void ShouldOpenOldDbAndCreateAndWorkWithSomeFusionIndexes( string zippedDbName, Provider highestProviderInOldVersion )
		 {
			  // given
			  unzip( this.GetType(), zippedDbName, _directory.databaseDir() );
			  IndexRecoveryTracker indexRecoveryTracker = new IndexRecoveryTracker( this );
			  // when
			  GraphDatabaseAPI db = SetupDb( _directory.databaseDir(), indexRecoveryTracker );

			  // then
			  Provider[] providers = ProvidersUpToAndIncluding( highestProviderInOldVersion );
			  Provider[] providersIncludingSubject = concat( providers, DEFAULT_PROVIDER );
			  int expectedNumberOfIndexes = providers.Length * 2;
			  try
			  {
					VerifyInitialState( indexRecoveryTracker, expectedNumberOfIndexes, InternalIndexState.ONLINE );

					// then
					foreach ( Provider provider in providers )
					{
						 VerifyIndexes( db, provider.label );
					}

					// when
					CreateIndexesAndData( db, DEFAULT_PROVIDER.label );

					// then
					VerifyIndexes( db, DEFAULT_PROVIDER.label );

					// when
					foreach ( Provider provider in providersIncludingSubject )
					{
						 AdditionalUpdates( db, provider.label );

						 // then
						 VerifyAfterAdditionalUpdate( db, provider.label );
					}

					// and finally
					foreach ( Provider provider in providersIncludingSubject )
					{
						 VerifyExpectedProvider( db, provider.label, provider.descriptor );
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // when
			  db = SetupDb( _directory.databaseDir(), indexRecoveryTracker );
			  try
			  {
					// then
					VerifyInitialState( indexRecoveryTracker, expectedNumberOfIndexes + 2, InternalIndexState.ONLINE );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private Provider[] ProvidersUpToAndIncluding( Provider provider )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( Provider.values() ).filter(p => p.ordinal() <= provider.ordinal()).toArray(Provider[]::new);
		 }

		 private GraphDatabaseAPI SetupDb( File storeDir, IndexRecoveryTracker indexRecoveryTracker )
		 {
			  Monitors monitors = new Monitors();
			  monitors.AddMonitorListener( indexRecoveryTracker );
			  return ( GraphDatabaseAPI ) ( new GraphDatabaseFactory() ).setMonitors(monitors).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).newGraphDatabase();
		 }

		 private void VerifyInitialState( IndexRecoveryTracker indexRecoveryTracker, int expectedNumberOfIndexes, InternalIndexState expectedInitialState )
		 {
			  assertEquals( expectedNumberOfIndexes, indexRecoveryTracker.InitialStateMap.Count, "exactly " + expectedNumberOfIndexes + " indexes " );
			  foreach ( InternalIndexState actualInitialState in indexRecoveryTracker.InitialStateMap.Values )
			  {
					assertEquals( expectedInitialState, actualInitialState, "initial state is online, don't do recovery" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyExpectedProvider(org.neo4j.kernel.internal.GraphDatabaseAPI db, org.neo4j.graphdb.Label label, org.neo4j.internal.kernel.api.schema.IndexProviderDescriptor expectedDescriptor) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private static void VerifyExpectedProvider( GraphDatabaseAPI db, Label label, IndexProviderDescriptor expectedDescriptor )
		 {
			  using ( Transaction tx = Db.beginTx(), KernelTransaction kernelTransaction = Db.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true) )
			  {
					TokenRead tokenRead = kernelTransaction.TokenRead();
					SchemaRead schemaRead = kernelTransaction.SchemaRead();

					int labelId = tokenRead.NodeLabel( label.Name() );
					int key1Id = tokenRead.PropertyKey( KEY1 );
					int key2Id = tokenRead.PropertyKey( KEY2 );

					IndexReference index = schemaRead.Index( labelId, key1Id );
					AssertIndexHasExpectedProvider( expectedDescriptor, index );
					index = schemaRead.Index( labelId, key1Id, key2Id );
					AssertIndexHasExpectedProvider( expectedDescriptor, index );
					tx.Success();
			  }
		 }

		 private static void AssertIndexHasExpectedProvider( IndexProviderDescriptor expectedDescriptor, IndexReference index )
		 {
			  assertEquals( expectedDescriptor.Key, index.ProviderKey(), "same key" );
			  assertEquals( expectedDescriptor.Version, index.ProviderVersion(), "same version" );
		 }

		 private static void CreateIndexDataAndShutdown( GraphDatabaseBuilder builder, string indexProvider, Label label )
		 {
			  CreateIndexDataAndShutdown(builder, indexProvider, label, db =>
			  {
			  });
		 }

		 private static void CreateIndexDataAndShutdown( GraphDatabaseBuilder builder, string indexProvider, Label label, System.Action<GraphDatabaseService> otherActions )
		 {
			  builder.SetConfig( GraphDatabaseSettings.default_schema_provider, indexProvider );
			  GraphDatabaseService db = builder.NewGraphDatabase();
			  try
			  {
					otherActions( db );
					CreateIndexData( db, label );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private static void CreateIndexData( GraphDatabaseService db, Label label )
		 {
			  CreateIndexesAndData( db, label );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File tempStoreDirectory() throws java.io.IOException
		 private static File TempStoreDirectory()
		 {
			  File file = File.createTempFile( "create-db", "neo4j" );
			  File storeDir = new File( file.AbsoluteFile.ParentFile, file.Name );
			  FileUtils.deleteFile( file );
			  return storeDir;
		 }

		 private static void CreateIndexesAndData( GraphDatabaseService db, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label).on(KEY1).create();
					Db.schema().indexFor(label).on(KEY1).on(KEY2).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }

			  CreateData( db, label );
		 }

		 private static void CreateData( GraphDatabaseService db, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 Node node = Db.createNode( label );
						 object value = i % 2 == 0 ? i : i.ToString();
						 node.SetProperty( KEY1, value );
						 if ( i % 3 == 0 )
						 {
							  node.SetProperty( KEY2, value );
						 }
					}
					tx.Success();
			  }
		 }

		 private static void CreateSpatialAndTemporalData( GraphDatabaseAPI db, Label label )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 Node node = Db.createNode( label );
						 object value = i % 2 == 0 ? Values.pointValue( CoordinateReferenceSystem.Cartesian, i, i ) : DurationValue.duration( 0, 0, i, 0 );
						 node.SetProperty( KEY1, value );
						 if ( i % 3 == 0 )
						 {
							  node.SetProperty( KEY2, value );
						 }
					}
					tx.Success();
			  }
		 }

		 private static void AdditionalUpdates( GraphDatabaseAPI db, Label label )
		 {
			  CreateData( db, label );
			  CreateSpatialAndTemporalData( db, label );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyIndexes(org.neo4j.kernel.internal.GraphDatabaseAPI db, org.neo4j.graphdb.Label label) throws Exception
		 private static void VerifyIndexes( GraphDatabaseAPI db, Label label )
		 {
			  assertTrue( HasIndex( db, label, KEY1 ) );
			  assertEquals( 100, CountIndexedNodes( db, label, KEY1 ) );

			  assertTrue( HasIndex( db, label, KEY1, KEY2 ) );
			  assertEquals( 34, CountIndexedNodes( db, label, KEY1, KEY2 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyAfterAdditionalUpdate(org.neo4j.kernel.internal.GraphDatabaseAPI db, org.neo4j.graphdb.Label label) throws Exception
		 private static void VerifyAfterAdditionalUpdate( GraphDatabaseAPI db, Label label )
		 {
			  assertTrue( HasIndex( db, label, KEY1 ) );
			  assertEquals( 300, CountIndexedNodes( db, label, KEY1 ) );

			  assertTrue( HasIndex( db, label, KEY1, KEY2 ) );
			  assertEquals( 102, CountIndexedNodes( db, label, KEY1, KEY2 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int countIndexedNodes(org.neo4j.kernel.internal.GraphDatabaseAPI db, org.neo4j.graphdb.Label label, String... keys) throws Exception
		 private static int CountIndexedNodes( GraphDatabaseAPI db, Label label, params string[] keys )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );

					TokenRead tokenRead = ktx.TokenRead();
					int labelId = tokenRead.NodeLabel( label.Name() );
					int[] propertyKeyIds = new int[keys.Length];
					for ( int i = 0; i < propertyKeyIds.Length; i++ )
					{
						 propertyKeyIds[i] = tokenRead.PropertyKey( keys[i] );
					}
					IndexQuery[] predicates = new IndexQuery[propertyKeyIds.Length];
					for ( int i = 0; i < propertyKeyIds.Length; i++ )
					{
						 predicates[i] = IndexQuery.exists( propertyKeyIds[i] );
					}
					IndexReference index = ktx.SchemaRead().index(labelId, propertyKeyIds);
					NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor();
					ktx.DataRead().nodeIndexSeek(index, cursor, IndexOrder.NONE, false, predicates);
					int count = 0;
					while ( cursor.Next() )
					{
						 count++;
					}

					tx.Success();
					return count;
			  }
		 }

		 private static bool HasIndex( GraphDatabaseService db, Label label, params string[] keys )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IList<string> keyList = new IList<string> { keys };
					foreach ( IndexDefinition index in Db.schema().getIndexes(label) )
					{
						 if ( asList( index.PropertyKeys ).Equals( keyList ) )
						 {
							  return true;
						 }
					}
					tx.Success();
			  }
			  return false;
		 }

		 private class IndexRecoveryTracker : IndexingService.MonitorAdapter
		 {
			 private readonly StartOldDbOnCurrentVersionAndCreateFusionIndexIT _outerInstance;

			 public IndexRecoveryTracker( StartOldDbOnCurrentVersionAndCreateFusionIndexIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal IDictionary<IndexDescriptor, InternalIndexState> InitialStateMap = new Dictionary<IndexDescriptor, InternalIndexState>();

			  public override void InitialState( StoreIndexDescriptor descriptor, InternalIndexState state )
			  {
					InitialStateMap[descriptor] = state;
			  }

			  public virtual void Reset()
			  {
					InitialStateMap = new Dictionary<IndexDescriptor, InternalIndexState>();
			  }
		 }
	}

}