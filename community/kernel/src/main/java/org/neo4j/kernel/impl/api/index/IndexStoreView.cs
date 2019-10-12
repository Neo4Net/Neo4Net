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
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;

	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Kernel.Api.Index;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using Register_DoubleLongRegister = Org.Neo4j.Register.Register_DoubleLongRegister;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	/// <summary>
	/// The indexing services view of the universe. </summary>
	public interface IndexStoreView : NodePropertyAccessor, PropertyLoader
	{
		 /// <summary>
		 /// Retrieve all nodes in the database which has got one or more of the given labels AND
		 /// one or more of the given property key ids. This scan additionally accepts a visitor
		 /// for label updates for a joint scan.
		 /// </summary>
		 /// <param name="labelIds"> array of label ids to generate updates for. Empty array means all. </param>
		 /// <param name="propertyKeyIdFilter"> property key ids to generate updates for. </param>
		 /// <param name="propertyUpdateVisitor"> visitor which will see all generated <seealso cref="EntityUpdates"/>. </param>
		 /// <param name="labelUpdateVisitor"> visitor which will see all generated <seealso cref="NodeLabelUpdate"/>. </param>
		 /// <param name="forceStoreScan"> overrides decision about which source to scan from. If {@code true}
		 /// then store scan will be used, otherwise if {@code false} then the best suited will be used. </param>
		 /// <returns> a <seealso cref="StoreScan"/> to start and to stop the scan. </returns>
		 StoreScan<FAILURE> visitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan );

		 /// <summary>
		 /// Retrieve all relationships in the database which has any of the the given relationship types AND
		 /// one or more of the given property key ids.
		 /// </summary>
		 /// <param name="relationshipTypeIds"> array of relationsip type ids to generate updates for. Empty array means all. </param>
		 /// <param name="propertyKeyIdFilter"> property key ids to generate updates for. </param>
		 /// <param name="propertyUpdateVisitor"> visitor which will see all generated <seealso cref="EntityUpdates"/> </param>
		 /// <returns> a <seealso cref="StoreScan"/> to start and to stop the scan. </returns>
		 StoreScan<FAILURE> visitRelationships<FAILURE>( int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor );

		 /// <summary>
		 /// Produces <seealso cref="EntityUpdates"/> objects from reading node {@code entityId}, its labels and properties
		 /// and puts those updates into node updates container.
		 /// </summary>
		 /// <param name="entityId"> id of entity to load. </param>
		 /// <returns> node updates container </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting EntityUpdates nodeAsUpdates(long entityId);
		 EntityUpdates NodeAsUpdates( long entityId );

		 Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Register_DoubleLongRegister output );

		 Register_DoubleLongRegister IndexSample( long indexId, Register_DoubleLongRegister output );

		 void ReplaceIndexCounts( long indexId, long uniqueElements, long maxUniqueElements, long indexSize );

		 void IncrementIndexUpdates( long indexId, long updatesDelta );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 @@SuppressWarnings("rawtypes") StoreScan EMPTY_SCAN = new StoreScan()
	//	 {
	//		  @@Override public void run()
	//		  {
	//		  }
	//
	//		  @@Override public void stop()
	//		  {
	//		  }
	//
	//		  @@Override public void acceptUpdate(MultipleIndexPopulator.MultipleIndexUpdater updater, IndexEntryUpdate update, long currentlyIndexedNodeId)
	//		  {
	//		  }
	//
	//		  @@Override public PopulationProgress getProgress()
	//		  {
	//				return PopulationProgress.DONE;
	//		  }
	//	 };
	}

	public static class IndexStoreView_Fields
	{
		 public static readonly IndexStoreView Empty = new IndexStoreView_Adaptor();
	}

	 public class IndexStoreView_Adaptor : IndexStoreView
	 {
		  public override void LoadProperties( long nodeId, EntityType type, MutableIntSet propertyIds, PropertyLoader_PropertyLoadSink sink )
		  {
		  }

		  public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		  {
				return Values.NO_VALUE;
		  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <FAILURE extends Exception> StoreScan<FAILURE> visitNodes(int[] labelIds, System.Func<int, boolean> propertyKeyIdFilter, org.neo4j.helpers.collection.Visitor<EntityUpdates,FAILURE> propertyUpdateVisitor, org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.api.labelscan.NodeLabelUpdate,FAILURE> labelUpdateVisitor, boolean forceStoreScan)
		  public override StoreScan<FAILURE> VisitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
		  {
				return EMPTY_SCAN;
		  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <FAILURE extends Exception> StoreScan<FAILURE> visitRelationships(int[] relationshipTypeIds, System.Func<int, boolean> propertyKeyIdFilter, org.neo4j.helpers.collection.Visitor<EntityUpdates,FAILURE> propertyUpdateVisitor)
		  public override StoreScan<FAILURE> VisitRelationships<FAILURE>( int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor ) where FAILURE : Exception
		  {
				return EMPTY_SCAN;
		  }

		  public override void ReplaceIndexCounts( long indexId, long uniqueElements, long maxUniqueElements, long indexSize )
		  {
		  }

		  public override EntityUpdates NodeAsUpdates( long nodeId )
		  {
				return null;
		  }

		  public override Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Register_DoubleLongRegister output )
		  {
				return output;
		  }

		  public override Register_DoubleLongRegister IndexSample( long indexId, Register_DoubleLongRegister output )
		  {
				return output;
		  }

		  public override void IncrementIndexUpdates( long indexId, long updatesDelta )
		  {
		  }
	 }

}