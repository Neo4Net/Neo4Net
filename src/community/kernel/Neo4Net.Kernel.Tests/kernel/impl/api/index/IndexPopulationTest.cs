using System;

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
	using Test = org.junit.Test;

	using Neo4Net.Collections.Helpers;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using NodeLabelUpdate = Neo4Net.Kernel.Api.LabelScan.NodeLabelUpdate;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class IndexPopulationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFlipToFailedIfFailureToApplyLastBatchWhileFlipping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustFlipToFailedIfFailureToApplyLastBatchWhileFlipping()
		 {
			  // given
			  NullLogProvider logProvider = NullLogProvider.Instance;
			  IndexStoreView storeView = EmptyIndexStoreViewThatProcessUpdates();
			  Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter populator = EmptyPopulatorWithThrowingUpdater();
			  FailedIndexProxy failedProxy = FailedIndexProxy( storeView, populator );
			  OnlineIndexProxy onlineProxy = OnlineIndexProxy( storeView );
			  FlippableIndexProxy flipper = new FlippableIndexProxy();
			  flipper.FlipTarget = () => onlineProxy;
			  MultipleIndexPopulator multipleIndexPopulator = new MultipleIndexPopulator( storeView, logProvider, EntityType.NODE, mock( typeof( SchemaState ) ) );

			  MultipleIndexPopulator.IndexPopulation indexPopulation = multipleIndexPopulator.AddPopulator( populator, DummyMeta(), flipper, t => failedProxy, "userDescription" );
			  multipleIndexPopulator.QueueUpdate( SomeUpdate() );
			  multipleIndexPopulator.IndexAllEntities().run();

			  // when
			  indexPopulation.Flip( false );

			  // then
			  assertSame( "flipper should have flipped to failing proxy", flipper.State, InternalIndexState.FAILED );
		 }

		 private OnlineIndexProxy OnlineIndexProxy( IndexStoreView storeView )
		 {
			  return new OnlineIndexProxy( DummyMeta(), Neo4Net.Kernel.Api.Index.IndexAccessor_Fields.Empty, storeView, false );
		 }

		 private FailedIndexProxy FailedIndexProxy( IndexStoreView storeView, Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter populator )
		 {
			  return new FailedIndexProxy( DummyMeta(), "userDescription", populator, IndexPopulationFailure.Failure("failure"), new IndexCountsRemover(storeView, 0), NullLogProvider.Instance );
		 }

		 private Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter EmptyPopulatorWithThrowingUpdater()
		 {
			  return new IndexPopulator_AdapterAnonymousInnerClass( this );
		 }

		 private class IndexPopulator_AdapterAnonymousInnerClass : Neo4Net.Kernel.Api.Index.IndexPopulator_Adapter
		 {
			 private readonly IndexPopulationTest _outerInstance;

			 public IndexPopulator_AdapterAnonymousInnerClass( IndexPopulationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override IndexUpdater newPopulatingUpdater( NodePropertyAccessor accessor )
			 {
				  return new IndexUpdaterAnonymousInnerClass( this );
			 }

			 private class IndexUpdaterAnonymousInnerClass : IndexUpdater
			 {
				 private readonly IndexPopulator_AdapterAnonymousInnerClass _outerInstance;

				 public IndexUpdaterAnonymousInnerClass( IndexPopulator_AdapterAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(Neo4Net.kernel.api.index.IndexEntryUpdate<?> update) throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
				 public void process<T1>( IndexEntryUpdate<T1> update )
				 {
					  throw new IndexEntryConflictException( 0, 1, Values.numberValue( 0 ) );
				 }

				 public void close()
				 {
				 }
			 }
		 }

		 private IndexStoreView_Adaptor EmptyIndexStoreViewThatProcessUpdates()
		 {
			  return new IndexStoreView_AdaptorAnonymousInnerClass( this );
		 }

		 private class IndexStoreView_AdaptorAnonymousInnerClass : IndexStoreView_Adaptor
		 {
			 private readonly IndexPopulationTest _outerInstance;

			 public IndexStoreView_AdaptorAnonymousInnerClass( IndexPopulationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override StoreScan<FAILURE> visitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
			 {
				  //noinspection unchecked
				  return new StoreScanAnonymousInnerClass( this );
			 }

			 private class StoreScanAnonymousInnerClass : StoreScan
			 {
				 private readonly IndexStoreView_AdaptorAnonymousInnerClass _outerInstance;

				 public StoreScanAnonymousInnerClass( IndexStoreView_AdaptorAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }


				 public void run()
				 {
				 }

				 public void stop()
				 {
				 }

				 public void acceptUpdate( MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate update, long currentlyIndexedNodeId )
				 {
					  if ( update.EntityId <= currentlyIndexedNodeId )
					  {
							updater.Process( update );
					  }
				 }

				 public PopulationProgress Progress
				 {
					 get
					 {
						  return null;
					 }
				 }
			 }
		 }

		 private CapableIndexDescriptor DummyMeta()
		 {
			  return TestIndexDescriptorFactory.forLabel( 0, 0 ).withId( 0 ).withoutCapabilities();
		 }

		 private IndexEntryUpdate<LabelSchemaDescriptor> SomeUpdate()
		 {
			  return IndexEntryUpdate.add( 0, SchemaDescriptorFactory.forLabel( 0, 0 ), Values.numberValue( 0 ) );
		 }
	}

}