using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using ExistsPredicate = Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExistsPredicate;
	using BridgingIndexProgressor = Org.Neo4j.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using FusionIndexSampler = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexSampler;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;

	internal class TemporalIndexReader : TemporalIndexCache<TemporalIndexPartReader<JavaToDotNetGenericWildcard>>, IndexReader
	{
		 private readonly IndexDescriptor _descriptor;

		 internal TemporalIndexReader( IndexDescriptor descriptor, TemporalIndexAccessor accessor ) : base( new PartFactory( accessor ) )
		 {
			  this._descriptor = descriptor;
		 }

		 public override void Close()
		 {
			  forAll( Resource.close, this );
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: NativeIndexReader<?,NativeIndexValue> partReader = uncheckedSelect(propertyValues[0].valueGroup());
			  NativeIndexReader<object, NativeIndexValue> partReader = UncheckedSelect( propertyValues[0].ValueGroup() );
			  return partReader == null ? 0L : partReader.CountIndexedNodes( nodeId, propertyKeyIds, propertyValues );
		 }

		 public override IndexSampler CreateSampler()
		 {
			  IList<IndexSampler> samplers = new List<IndexSampler>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (TemporalIndexPartReader<?> partReader : this)
			  foreach ( TemporalIndexPartReader<object> partReader in this )
			  {
					samplers.Add( partReader.createSampler() );
			  }
			  return new FusionIndexSampler( samplers );
		 }

		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  NodeValueIterator nodeValueIterator = new NodeValueIterator();
			  Query( nodeValueIterator, IndexOrder.NONE, nodeValueIterator.NeedsValues(), predicates );
			  return nodeValueIterator;
		 }

		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
			  if ( predicates.Length != 1 )
			  {
					throw new System.ArgumentException( "Only single property temporal indexes are supported." );
			  }
			  IndexQuery predicate = predicates[0];
			  if ( predicate is IndexQuery.ExistsPredicate )
			  {
					LoadAll();
					BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
					cursor.Initialize( _descriptor, multiProgressor, predicates, indexOrder, needsValues );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexReader<?,NativeIndexValue> reader : this)
					foreach ( NativeIndexReader<object, NativeIndexValue> reader in this )
					{
						 reader.Query( multiProgressor, indexOrder, needsValues, predicates );
					}
			  }
			  else
			  {
					if ( ValidPredicate( predicate ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: NativeIndexReader<?,NativeIndexValue> part = uncheckedSelect(predicate.valueGroup());
						 NativeIndexReader<object, NativeIndexValue> part = UncheckedSelect( predicate.ValueGroup() );
						 if ( part != null )
						 {
							  part.Query( cursor, indexOrder, needsValues, predicates );
						 }
						 else
						 {
							  cursor.Initialize( _descriptor, IndexProgressor.EMPTY, predicates, indexOrder, needsValues );
						 }
					}
					else
					{
						 cursor.Initialize( _descriptor, IndexProgressor.EMPTY, predicates, indexOrder, needsValues );
					}
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return true;
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  LoadAll();
			  BridgingIndexProgressor multiProgressor = new BridgingIndexProgressor( cursor, _descriptor.schema().PropertyIds );
			  cursor.Initialize( _descriptor, multiProgressor, new IndexQuery[0], IndexOrder.NONE, needsValues );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (NativeIndexReader<?,NativeIndexValue> reader : this)
			  foreach ( NativeIndexReader<object, NativeIndexValue> reader in this )
			  {
					reader.DistinctValues( multiProgressor, propertyAccessor, needsValues );
			  }
		 }

		 private bool ValidPredicate( IndexQuery predicate )
		 {
			  return predicate is IndexQuery.ExactPredicate || predicate is IndexQuery.RangePredicate;
		 }

		 /// <summary>
		 /// To create TemporalIndexPartReaders on demand, the PartFactory maintains a reference to the parent TemporalIndexAccessor.
		 /// The creation of a part reader can then be delegated to the correct PartAccessor.
		 /// </summary>
		 internal class PartFactory : TemporalIndexCache.Factory<TemporalIndexPartReader<JavaToDotNetGenericWildcard>>
		 {
			  internal readonly TemporalIndexAccessor Accessor;

			  internal PartFactory( TemporalIndexAccessor accessor )
			  {
					this.Accessor = accessor;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newDate()
			  public override TemporalIndexPartReader<object> NewDate()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.DATE, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newLocalDateTime()
			  public override TemporalIndexPartReader<object> NewLocalDateTime()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.LOCAL_DATE_TIME, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newZonedDateTime()
			  public override TemporalIndexPartReader<object> NewZonedDateTime()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.ZONED_DATE_TIME, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newLocalTime()
			  public override TemporalIndexPartReader<object> NewLocalTime()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.LOCAL_TIME, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newZonedTime()
			  public override TemporalIndexPartReader<object> NewZonedTime()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.ZONED_TIME, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TemporalIndexPartReader<?> newDuration()
			  public override TemporalIndexPartReader<object> NewDuration()
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return Accessor.selectOrElse( ValueGroup.DURATION, TemporalIndexAccessor.PartAccessor::newReader, null );
			  }
		 }
	}

}