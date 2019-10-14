using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using SimpleNodeValueClient = Neo4Net.Storageengine.Api.schema.SimpleNodeValueClient;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Race = Neo4Net.Test.Race;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.toArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesBySubProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexEntryUpdate.remove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Config.defaults;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Race.throwing;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexPopulationStressTest
	public class IndexPopulationStressTest
	{
		 private static readonly IndexProviderDescriptor _provider = new IndexProviderDescriptor( "provider", "1.0" );
		 private const int THREADS = 50;
		 private const int MAX_BATCH_SIZE = 100;
		 private const int BATCHES_PER_THREAD = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> providers()
		 public static ICollection<object[]> Providers()
		 {
			  ICollection<object[]> parameters = new List<object[]>();
			  // GenericNativeIndexProvider
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  parameters.Add( Of( "generic", true, RandomValues::nextValue, test => new GenericNativeIndexProvider( test.directory(), test.rules.pageCache(), test.rules.fileSystem(), EMPTY, immediate(), false, defaults() ) ) );
			  // NumberIndexProvider
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  parameters.Add( Of( "number", true, RandomValues::nextNumberValue, test => new NumberIndexProvider( test.rules.pageCache(), test.rules.fileSystem(), test.directory(), EMPTY, immediate(), false ) ) );
			  // StringIndexProvider
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  parameters.Add( Of( "string", true, RandomValues::nextAlphaNumericTextValue, test => new StringIndexProvider( test.rules.pageCache(), test.rules.fileSystem(), test.directory(), EMPTY, immediate(), false ) ) );
			  // SpatialIndexProvider
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  parameters.Add( Of( "spatial", false, RandomValues::nextPointValue, test => new SpatialIndexProvider( test.rules.pageCache(), test.rules.fileSystem(), test.directory(), EMPTY, immediate(), false, defaults() ) ) );
			  // TemporalIndexProvider
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  parameters.Add( Of( "temporal", true, RandomValues::nextTemporalValue, test => new TemporalIndexProvider( test.rules.pageCache(), test.rules.fileSystem(), test.directory(), EMPTY, immediate(), false ) ) );
			  return parameters;
		 }

		 private static object[] Of( string name, bool hasValues, System.Func<RandomValues, Value> valueGenerator, System.Func<IndexPopulationStressTest, IndexProvider> providerCreator )
		 {
			  return toArray( name, hasValues, valueGenerator, providerCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheAndDependenciesRule rules = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public PageCacheAndDependenciesRule Rules = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

		 protected internal readonly StoreIndexDescriptor Descriptor = forSchema( forLabel( 0, 0 ), _provider ).withId( 0 );
		 private readonly StoreIndexDescriptor _descriptor2 = forSchema( forLabel( 1, 0 ), _provider ).withId( 1 );
		 private readonly IndexSamplingConfig _samplingConfig = new IndexSamplingConfig( 1000, 0.2, true );
		 private readonly NodePropertyAccessor _nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public String name;
		 public string Name;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public boolean hasValues;
		 public bool HasValues;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public System.Func<org.neo4j.values.storable.RandomValues,org.neo4j.values.storable.Value> valueGenerator;
		 public System.Func<RandomValues, Value> ValueGenerator;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public System.Func<IndexPopulationStressTest,org.neo4j.kernel.api.index.IndexProvider> providerCreator;
		 public System.Func<IndexPopulationStressTest, IndexProvider> ProviderCreator;

		 private IndexPopulator _populator;
		 private IndexProvider _indexProvider;
		 private bool _prevAccessCheck;

		 private IndexDirectoryStructure.Factory Directory()
		 {
			  File storeDir = Rules.directory().databaseDir();
			  return directoriesBySubProvider( directoriesByProvider( storeDir ).forProvider( _provider ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _indexProvider = ProviderCreator.apply( this );
			  Rules.fileSystem().mkdirs(_indexProvider.directoryStructure().rootDirectory());
			  _populator = _indexProvider.getPopulator( Descriptor, _samplingConfig, heapBufferFactory( 1024 ) );
			  when( _nodePropertyAccessor.getNodePropertyValue( anyLong(), anyInt() ) ).thenThrow(typeof(System.NotSupportedException));
			  _prevAccessCheck = UnsafeUtil.exchangeNativeAccessCheckEnabled( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  UnsafeUtil.exchangeNativeAccessCheckEnabled( _prevAccessCheck );
			  if ( _populator != null )
			  {
					_populator.close( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressIt()
		 {
			  Race race = new Race();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.atomic.AtomicReferenceArray<java.util.List<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>>> lastBatches = new java.util.concurrent.atomic.AtomicReferenceArray<>(THREADS);
			  AtomicReferenceArray<IList<IndexEntryUpdate<object>>> lastBatches = new AtomicReferenceArray<IList<IndexEntryUpdate<object>>>( THREADS );
			  Generator[] generators = new Generator[THREADS];

			  _populator.create();
			  System.Threading.CountdownEvent insertersDone = new System.Threading.CountdownEvent( THREADS );
			  ReadWriteLock updateLock = new ReentrantReadWriteLock( true );
			  for ( int i = 0; i < THREADS; i++ )
			  {
					race.AddContestant( Inserter( lastBatches, generators, insertersDone, updateLock, i ), 1 );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
			  ICollection<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
			  race.AddContestant( Updater( lastBatches, insertersDone, updateLock, updates ) );

			  race.Go();
			  _populator.close( true );
			  _populator = null; // to let the after-method know that we've closed it ourselves

			  // then assert that a tree built by a single thread ends up exactly the same
			  BuildReferencePopulatorSingleThreaded( generators, updates );
			  using ( IndexAccessor accessor = _indexProvider.getOnlineAccessor( Descriptor, _samplingConfig ), IndexAccessor referenceAccessor = _indexProvider.getOnlineAccessor( _descriptor2, _samplingConfig ), IndexReader reader = accessor.NewReader(), IndexReader referenceReader = referenceAccessor.NewReader() )
			  {
					SimpleNodeValueClient entries = new SimpleNodeValueClient();
					SimpleNodeValueClient referenceEntries = new SimpleNodeValueClient();
					reader.Query( entries, IndexOrder.NONE, HasValues, IndexQuery.exists( 0 ) );
					referenceReader.Query( referenceEntries, IndexOrder.NONE, HasValues, IndexQuery.exists( 0 ) );
					while ( referenceEntries.Next() )
					{
						 assertTrue( entries.Next() );
						 assertEquals( referenceEntries.Reference, entries.Reference );
						 if ( HasValues )
						 {
							  assertEquals( ValueTuple.of( referenceEntries.Values ), ValueTuple.of( entries.Values ) );
						 }
					}
					assertFalse( entries.Next() );
			  }
		 }

		 private ThreadStart Updater<T1, T2>( AtomicReferenceArray<T1> lastBatches, System.Threading.CountdownEvent insertersDone, ReadWriteLock updateLock, ICollection<T2> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  return throwing(() =>
			  {
				// Entity ids that have been removed, so that additions can reuse them
				IList<long> removed = new List<long>();
				RandomValues randomValues = RandomValues.create( new Random( Random.seed() + THREADS ) );
				while ( insertersDone.CurrentCount > 0 )
				{
					 // Do updates now and then
					 Thread.Sleep( 10 );
					 updateLock.writeLock().@lock();
					 try
					 {
						 using ( IndexUpdater updater = _populator.newPopulatingUpdater( _nodePropertyAccessor ) )
						 {
							  for ( int i = 0; i < THREADS; i++ )
							  {
									IList<IndexEntryUpdate<object>> batch = lastBatches.get( i );
									if ( batch != null )
									{
										 IndexEntryUpdate<object> update = null;
										 switch ( randomValues.Next( 3 ) )
										 {
										 case 0: // add
											  if ( !removed.Empty )
											  {
													long? id = removed.remove( randomValues.Next( removed.size() ) );
													update = add( id, Descriptor, ValueGenerator.apply( randomValues ) );
											  }
											  break;
										 case 1: // remove
											  IndexEntryUpdate<object> removal = batch.get( randomValues.Next( batch.size() ) );
											  update = remove( removal.EntityId, Descriptor, removal.values() );
											  removed.add( removal.EntityId );
											  break;
										 case 2: // change
											  removal = batch.get( randomValues.Next( batch.size() ) );
											  change( removal.EntityId, Descriptor, removal.values(), toArray(ValueGenerator.apply(randomValues)) );
											  break;
										 default:
											  throw new System.ArgumentException();
										 }
										 if ( update != null )
										 {
											  updater.process( update );
											  updates.Add( update );
										 }
									}
							  }
						 }
					 }
					 finally
					 {
						  updateLock.writeLock().unlock();
					 }
				}
			  });
		 }

		 private ThreadStart Inserter<T1>( AtomicReferenceArray<T1> lastBatches, Generator[] generators, System.Threading.CountdownEvent insertersDone, ReadWriteLock updateLock, int slot ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  int worstCaseEntriesPerThread = BATCHES_PER_THREAD * MAX_BATCH_SIZE;
			  return throwing(() =>
			  {
				try
				{
					 Generator generator = generators[slot] = new Generator( this, MAX_BATCH_SIZE, Random.seed() + slot, slot * worstCaseEntriesPerThread );
					 for ( int j = 0; j < BATCHES_PER_THREAD; j++ )
					 {
						  IList<IndexEntryUpdate<object>> batch = generator.Batch();
						  updateLock.readLock().@lock();
						  try
						  {
								_populator.add( batch );
						  }
						  finally
						  {
								updateLock.readLock().unlock();
						  }
						  lastBatches.set( slot, batch );
					 }
				}
				finally
				{
					 // This helps the updater know when to stop updating
					 insertersDone.Signal();
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void buildReferencePopulatorSingleThreaded(Generator[] generators, java.util.Collection<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void BuildReferencePopulatorSingleThreaded<T1>( Generator[] generators, ICollection<T1> updates )
		 {
			  IndexPopulator referencePopulator = _indexProvider.getPopulator( _descriptor2, _samplingConfig, heapBufferFactory( 1024 ) );
			  referencePopulator.Create();
			  bool referenceSuccess = false;
			  try
			  {
					foreach ( Generator generator in generators )
					{
						 generator.Reset();
						 for ( int i = 0; i < BATCHES_PER_THREAD; i++ )
						 {
							  referencePopulator.Add( generator.Batch() );
						 }
					}
					using ( IndexUpdater updater = referencePopulator.NewPopulatingUpdater( _nodePropertyAccessor ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
						 foreach ( IndexEntryUpdate<object> update in updates )
						 {
							  updater.Process( update );
						 }
					}
					referenceSuccess = true;
			  }
			  finally
			  {
					referencePopulator.Close( referenceSuccess );
			  }
		 }

		 private class Generator
		 {
			 private readonly IndexPopulationStressTest _outerInstance;

			  internal readonly int MaxBatchSize;
			  internal readonly long Seed;
			  internal readonly long StartEntityId;

			  internal RandomValues RandomValues;
			  internal long NextEntityId;

			  internal Generator( IndexPopulationStressTest outerInstance, int maxBatchSize, long seed, long startEntityId )
			  {
				  this._outerInstance = outerInstance;
					this.StartEntityId = startEntityId;
					this.NextEntityId = startEntityId;
					this.MaxBatchSize = maxBatchSize;
					this.Seed = seed;
					Reset();
			  }

			  internal virtual void Reset()
			  {
					RandomValues = RandomValues.create( new Random( Seed ) );
					NextEntityId = StartEntityId;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<? extends org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch()
			  internal virtual IList<IndexEntryUpdate<object>> Batch()
			  {
					int n = RandomValues.Next( MaxBatchSize ) + 1;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updates = new java.util.ArrayList<>(n);
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>( n );
					for ( int i = 0; i < n; i++ )
					{
						 updates.Add( add( NextEntityId++, outerInstance.Descriptor, outerInstance.ValueGenerator.apply( RandomValues ) ) );
					}
					return updates;
			  }
		 }
	}

}