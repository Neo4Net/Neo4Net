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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using IndexCapability = Neo4Net.Internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.DoubleLatch.awaitLatch;

	public class ControlledPopulationIndexProvider : IndexProvider
	{
		 private IndexPopulator _mockedPopulator = new Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter();
		 private readonly IndexAccessor _mockedWriter = mock( typeof( IndexAccessor ) );
		 private readonly System.Threading.CountdownEvent _writerLatch = new System.Threading.CountdownEvent( 1 );
		 private InternalIndexState _initialIndexState = POPULATING;
		 internal readonly AtomicInteger PopulatorCallCount = new AtomicInteger();
		 internal readonly AtomicInteger WriterCallCount = new AtomicInteger();

		 public static readonly IndexProviderDescriptor ProviderDescriptor = new IndexProviderDescriptor( "controlled-population", "1.0" );

		 public ControlledPopulationIndexProvider() : base(ProviderDescriptor, IndexDirectoryStructure.NONE)
		 {
			  InitialIndexState = _initialIndexState;
			  when( _mockedWriter.newReader() ).thenReturn(IndexReader.EMPTY);
		 }

		 public virtual DoubleLatch InstallPopulationJobCompletionLatch()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch populationCompletionLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch populationCompletionLatch = new DoubleLatch();
			  _mockedPopulator = new IndexPopulator_AdapterAnonymousInnerClass( this, populationCompletionLatch );
			  return populationCompletionLatch;
		 }

		 private class IndexPopulator_AdapterAnonymousInnerClass : Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			 private readonly ControlledPopulationIndexProvider _outerInstance;

			 private DoubleLatch _populationCompletionLatch;

			 public IndexPopulator_AdapterAnonymousInnerClass( ControlledPopulationIndexProvider outerInstance, DoubleLatch populationCompletionLatch )
			 {
				 this.outerInstance = outerInstance;
				 this._populationCompletionLatch = populationCompletionLatch;
			 }

			 public override void create()
			 {
				  _populationCompletionLatch.startAndWaitForAllToStartAndFinish();
				  base.create();
			 }

			 public override IndexSample sampleResult()
			 {
				  return new IndexSample();
			 }
		 }

		 public virtual void AwaitFullyPopulated()
		 {
			  awaitLatch( _writerLatch );
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  PopulatorCallCount.incrementAndGet();
			  return _mockedPopulator;
		 }

		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor indexConfig, IndexSamplingConfig samplingConfig )
		 {
			  WriterCallCount.incrementAndGet();
			  _writerLatch.Signal();
			  return _mockedWriter;
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  return _initialIndexState;
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  return IndexCapability.NO_CAPABILITY;
		 }

		 public virtual InternalIndexState InitialIndexState
		 {
			 set
			 {
				  this._initialIndexState = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  throw new System.InvalidOperationException();
		 }

		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating;
		 }
	}

}