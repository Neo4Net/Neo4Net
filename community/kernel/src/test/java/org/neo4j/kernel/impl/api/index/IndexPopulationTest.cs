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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;

	using Org.Neo4j.Helpers.Collection;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using Values = Org.Neo4j.Values.Storable.Values;

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
			  Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter populator = EmptyPopulatorWithThrowingUpdater();
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
			  return new OnlineIndexProxy( DummyMeta(), Org.Neo4j.Kernel.Api.Index.IndexAccessor_Fields.Empty, storeView, false );
		 }

		 private FailedIndexProxy FailedIndexProxy( IndexStoreView storeView, Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter populator )
		 {
			  return new FailedIndexProxy( DummyMeta(), "userDescription", populator, IndexPopulationFailure.Failure("failure"), new IndexCountsRemover(storeView, 0), NullLogProvider.Instance );
		 }

		 private Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter EmptyPopulatorWithThrowingUpdater()
		 {
			  return new IndexPopulator_AdapterAnonymousInnerClass( this );
		 }

		 private class IndexPopulator_AdapterAnonymousInnerClass : Org.Neo4j.Kernel.Api.Index.IndexPopulator_Adapter
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
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
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