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

	using static Neo4Net.@internal.Kernel.Api.IndexQuery.StringPrefixPredicate;

	internal class StringIndexReader : NativeIndexReader<StringIndexKey, NativeIndexValue>
	{
		 internal StringIndexReader( GBPTree<StringIndexKey, NativeIndexValue> tree, IndexLayout<StringIndexKey, NativeIndexValue> layout, IndexDescriptor descriptor ) : base( tree, layout, descriptor )
		 {
		 }

		 internal override void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  if ( predicates.Length != 1 )
			  {
					throw new System.NotSupportedException();
			  }

			  CapabilityValidator.ValidateQuery( StringIndexProvider.Capability, indexOrder, predicates );
		 }

		 internal override bool InitializeRangeForQuery( StringIndexKey treeKeyFrom, StringIndexKey treeKeyTo, IndexQuery[] predicates )
		 {
			  IndexQuery predicate = predicates[0];
			  switch ( predicate.Type() )
			  {
			  case exists:
					treeKeyFrom.InitValueAsLowest( ValueGroup.TEXT );
					treeKeyTo.InitValueAsHighest( ValueGroup.TEXT );
					return false;
			  case exact:
					IndexQuery.ExactPredicate exactPredicate = ( IndexQuery.ExactPredicate ) predicate;
					treeKeyFrom.From( exactPredicate.Value() );
					treeKeyTo.From( exactPredicate.Value() );
					return false;
			  case range:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?> rangePredicate = (org.neo4j.internal.kernel.api.IndexQuery.RangePredicate<?>)predicate;
					IndexQuery.RangePredicate<object> rangePredicate = ( IndexQuery.RangePredicate<object> )predicate;
					InitFromForRange( rangePredicate, treeKeyFrom );
					InitToForRange( rangePredicate, treeKeyTo );
					return false;
			  case stringPrefix:
					StringPrefixPredicate prefixPredicate = ( StringPrefixPredicate ) predicate;
					treeKeyFrom.InitAsPrefixLow( prefixPredicate.prefix() );
					treeKeyTo.InitAsPrefixHigh( prefixPredicate.prefix() );
					return false;
			  case stringSuffix:
			  case stringContains:
					treeKeyFrom.InitValueAsLowest( ValueGroup.TEXT );
					treeKeyTo.InitValueAsHighest( ValueGroup.TEXT );
					return true;
			  default:
					throw new System.ArgumentException( "IndexQuery of type " + predicate.Type() + " is not supported." );
			  }
		 }

		 private static void InitFromForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, StringIndexKey treeKeyFrom )
		 {
			  Value fromValue = rangePredicate.FromValue();
			  if ( fromValue == Values.NO_VALUE )
			  {
					treeKeyFrom.InitValueAsLowest( ValueGroup.TEXT );
			  }
			  else
			  {
					treeKeyFrom.Initialize( rangePredicate.FromInclusive() ? long.MinValue : long.MaxValue );
					treeKeyFrom.From( fromValue );
					treeKeyFrom.CompareId = true;
			  }
		 }

		 private static void InitToForRange<T1>( IndexQuery.RangePredicate<T1> rangePredicate, StringIndexKey treeKeyTo )
		 {
			  Value toValue = rangePredicate.ToValue();
			  if ( toValue == Values.NO_VALUE )
			  {
					treeKeyTo.InitValueAsHighest( ValueGroup.TEXT );
			  }
			  else
			  {
					treeKeyTo.Initialize( rangePredicate.ToInclusive() ? long.MaxValue : long.MinValue );
					treeKeyTo.From( toValue );
					treeKeyTo.CompareId = true;
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return true;
		 }
	}

}