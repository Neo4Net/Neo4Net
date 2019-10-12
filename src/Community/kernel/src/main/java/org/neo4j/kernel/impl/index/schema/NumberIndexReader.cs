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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Neo4Net.Index.@internal.gbptree;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using ExactPredicate = Neo4Net.@internal.Kernel.Api.IndexQuery.ExactPredicate;
	using Neo4Net.@internal.Kernel.Api.IndexQuery;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

	internal class NumberIndexReader<VALUE> : NativeIndexReader<NumberIndexKey, VALUE> where VALUE : NativeIndexValue
	{
		 internal NumberIndexReader( GBPTree<NumberIndexKey, VALUE> tree, IndexLayout<NumberIndexKey, VALUE> layout, IndexDescriptor descriptor ) : base( tree, layout, descriptor )
		 {
		 }

		 internal override void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  if ( predicates.Length != 1 )
			  {
					throw new System.NotSupportedException();
			  }

			  CapabilityValidator.ValidateQuery( NumberIndexProvider.Capability, indexOrder, predicates );
		 }

		 internal override bool InitializeRangeForQuery( NumberIndexKey treeKeyFrom, NumberIndexKey treeKeyTo, IndexQuery[] predicates )
		 {
			  IndexQuery predicate = predicates[0];
			  switch ( predicate.Type() )
			  {
			  case exists:
					treeKeyFrom.InitValueAsLowest( ValueGroup.NUMBER );
					treeKeyTo.InitValueAsHighest( ValueGroup.NUMBER );
					break;
			  case exact:
					IndexQuery.ExactPredicate exactPredicate = ( IndexQuery.ExactPredicate ) predicate;
					treeKeyFrom.From( exactPredicate.Value() );
					treeKeyTo.From( exactPredicate.Value() );
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
			  return false;
		 }

		 private static void InitToForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, NumberIndexKey treeKeyTo )
		 {
			  Value toValue = rangePredicate.ToValue();
			  if ( toValue == Values.NO_VALUE )
			  {
					treeKeyTo.InitValueAsHighest( ValueGroup.NUMBER );
			  }
			  else
			  {
					treeKeyTo.initialize( rangePredicate.ToInclusive() ? long.MaxValue : long.MinValue );
					treeKeyTo.From( toValue );
					treeKeyTo.CompareId = true;
			  }
		 }

		 private static void InitFromForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, NumberIndexKey treeKeyFrom )
		 {
			  Value fromValue = rangePredicate.FromValue();
			  if ( fromValue == Values.NO_VALUE )
			  {
					treeKeyFrom.InitValueAsLowest( ValueGroup.NUMBER );
			  }
			  else
			  {
					treeKeyFrom.initialize( rangePredicate.FromInclusive() ? long.MinValue : long.MaxValue );
					treeKeyFrom.From( fromValue );
					treeKeyFrom.CompareId = true;
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return true;
		 }
	}

}