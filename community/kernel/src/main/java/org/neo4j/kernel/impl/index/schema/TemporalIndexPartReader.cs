﻿/*
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
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Values = Org.Neo4j.Values.Storable.Values;

	internal class TemporalIndexPartReader<KEY> : NativeIndexReader<KEY, NativeIndexValue> where KEY : NativeIndexSingleValueKey<KEY>
	{
		 internal TemporalIndexPartReader( GBPTree<KEY, NativeIndexValue> tree, IndexLayout<KEY, NativeIndexValue> layout, IndexDescriptor descriptor ) : base( tree, layout, descriptor )
		 {
		 }

		 protected internal override void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  if ( predicates.Length != 1 )
			  {
					throw new System.NotSupportedException();
			  }

			  CapabilityValidator.ValidateQuery( TemporalIndexProvider.Capability, indexOrder, predicates );
		 }

		 protected internal override bool InitializeRangeForQuery( KEY treeKeyFrom, KEY treeKeyTo, IndexQuery[] predicates )
		 {
			  IndexQuery predicate = predicates[0];
			  switch ( predicate.Type() )
			  {
			  case exists:
					treeKeyFrom.initValueAsLowest( ValueGroup.UNKNOWN );
					treeKeyTo.initValueAsHighest( ValueGroup.UNKNOWN );
					break;

			  case exact:
					IndexQuery.ExactPredicate exactPredicate = ( IndexQuery.ExactPredicate ) predicate;
					treeKeyFrom.from( exactPredicate.Value() );
					treeKeyTo.from( exactPredicate.Value() );
					break;

			  case range:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> rangePredicate = (org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?>) predicate;
					IndexQuery.RangePredicate<object> rangePredicate = ( IndexQuery.RangePredicate<object> ) predicate;
					InitFromForRange( rangePredicate, treeKeyFrom );
					InitToForRange( rangePredicate, treeKeyTo );
					break;

			  default:
					throw new System.ArgumentException( "IndexQuery of type " + predicate.Type() + " is not supported." );
			  }
			  return false; // no filtering
		 }

		 private void InitFromForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, KEY treeKeyFrom )
		 {
			  Value fromValue = rangePredicate.FromValue();
			  if ( fromValue == Values.NO_VALUE )
			  {
					treeKeyFrom.initValueAsLowest( ValueGroup.UNKNOWN );
			  }
			  else
			  {
					treeKeyFrom.initialize( rangePredicate.FromInclusive() ? long.MinValue : long.MaxValue );
					treeKeyFrom.from( fromValue );
					treeKeyFrom.CompareId = true;
			  }
		 }

		 private void InitToForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, KEY treeKeyTo )
		 {
			  Value toValue = rangePredicate.ToValue();
			  if ( toValue == Values.NO_VALUE )
			  {
					treeKeyTo.initValueAsHighest( ValueGroup.UNKNOWN );
			  }
			  else
			  {
					treeKeyTo.initialize( rangePredicate.ToInclusive() ? long.MaxValue : long.MinValue );
					treeKeyTo.from( toValue );
					treeKeyTo.CompareId = true;
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return true;
		 }
	}

}