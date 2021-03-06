﻿using System;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.fusion
{

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using ExistsPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExistsPredicate;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using BridgingIndexProgressor = Org.Neo4j.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongResourceCollections.concat;

	internal class FusionIndexReader : FusionIndexBase<IndexReader>, IndexReader
	{
		 private readonly IndexDescriptor _descriptor;

		 internal FusionIndexReader( SlotSelector slotSelector, LazyInstanceSelector<IndexReader> instanceSelector, IndexDescriptor descriptor ) : base( slotSelector, instanceSelector )
		 {
			  this._descriptor = descriptor;
		 }

		 public override void Close()
		 {
			  InstanceSelector.close( Resource.close );
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  return InstanceSelector.select( SlotSelector.selectSlot( propertyValues, GroupOf ) ).countIndexedNodes( nodeId, propertyKeyIds, propertyValues );
		 }

		 public override IndexSampler CreateSampler()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return new FusionIndexSampler( InstanceSelector.transform( IndexReader::createSampler ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IndexSlot slot = SlotSelector.selectSlot( predicates, IndexQuery::valueGroup );
			  return slot != null ? InstanceSelector.select( slot ).query( predicates ) : concat( InstanceSelector.transform( reader => reader.query( predicates ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient cursor, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IndexSlot slot = SlotSelector.selectSlot( predicates, IndexQuery::valueGroup );
			  if ( slot != null )
			  {
					InstanceSelector.select( slot ).query( cursor, indexOrder, needsValues, predicates );
			  }
			  else
			  {
					if ( indexOrder != IndexOrder.NONE )
					{
						 throw new System.NotSupportedException( format( "Tried to query index with unsupported order %s. Supported orders for query %s are %s.", indexOrder, Arrays.ToString( predicates ), IndexOrder.NONE ) );
					}
					BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
					cursor.Initialize( _descriptor, multiProgressor, predicates, indexOrder, needsValues );
					try
					{
						 InstanceSelector.forAll(reader =>
						 {
						  try
						  {
								reader.query( multiProgressor, indexOrder, needsValues, predicates );
						  }
						  catch ( IndexNotApplicableKernelException e )
						  {
								throw new InnerException( e );
						  }
						 });
					}
					catch ( InnerException e )
					{
						 throw e.InnerException;
					}
			  }
		 }

		 private sealed class InnerException : Exception
		 {
			  internal InnerException( IndexNotApplicableKernelException e ) : base( e )
			  {
			  }

			  public override IndexNotApplicableKernelException Cause
			  {
				  get
				  {
					  lock ( this )
					  {
							return ( IndexNotApplicableKernelException ) base.InnerException;
					  }
				  }
			  }
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
			  cursor.Initialize( _descriptor, multiProgressor, new IndexQuery[0], IndexOrder.NONE, needsValues );
			  InstanceSelector.forAll( reader => reader.distinctValues( multiProgressor, propertyAccessor, needsValues ) );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IndexSlot slot = SlotSelector.selectSlot( predicates, IndexQuery::valueGroup );
			  if ( slot != null )
			  {
					return InstanceSelector.select( slot ).hasFullValuePrecision( predicates );
			  }
			  else
			  {
					// UNKNOWN slot which basically means the EXISTS predicate
					if ( !( predicates.Length == 1 && predicates[0] is IndexQuery.ExistsPredicate ) )
					{
						 throw new System.InvalidOperationException( "Selected IndexReader null for predicates " + Arrays.ToString( predicates ) );
					}
					return true;
			  }
		 }
	}

}