using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.locking
{
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using ExactPredicate = Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.IndexQuery.exact;

	public class IndexEntryResourceTypesTest
	{

		 public const int LABEL_ID = 1;
		 public const int PROPERTY_ID = 2;
		 public static readonly Value Value = Values.of( "value" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceBackwardsCompatibleId()
		 public virtual void ShouldProduceBackwardsCompatibleId()
		 {
			  long id = ResourceTypes.indexEntryResourceId( LABEL_ID, exact( PROPERTY_ID, Value ) );
			  assertThat( id, equalTo( 155667838465249649L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDifferentiateBetweenIndexes()
		 public virtual void ShouldDifferentiateBetweenIndexes()
		 {
			  ExactPredicate pred1 = exact( 1, "value" );
			  ExactPredicate pred2 = exact( 1, "value2" );
			  ExactPredicate pred3 = exact( 2, "value" );
			  ExactPredicate pred4 = exact( 2, "value2" );

			  IList<long> ids = Arrays.asList( ResourceTypes.indexEntryResourceId( 1, array( pred1 ) ), ResourceTypes.indexEntryResourceId( 1, array( pred2 ) ), ResourceTypes.indexEntryResourceId( 1, array( pred3 ) ), ResourceTypes.indexEntryResourceId( 1, array( pred4 ) ), ResourceTypes.indexEntryResourceId( 2, array( pred1 ) ), ResourceTypes.indexEntryResourceId( 1, array( pred1, pred2 ) ), ResourceTypes.indexEntryResourceId( 1, array( pred1, pred2, pred3 ) ), ResourceTypes.indexEntryResourceId( 2, array( pred1, pred2, pred3, pred4 ) ) );

			  ISet<long> uniqueIds = Iterables.asSet( ids );
			  assertThat( ids.Count, equalTo( uniqueIds.Count ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToHashAllTypesWith220HashFunction()
		 public virtual void MustBeAbleToHashAllTypesWith220HashFunction()
		 {
			  VerifyCanHashAllTypes( ResourceTypes.indexEntryResourceId_2_2_0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeAbleToHashAllTypesWith4xHashFunction()
		 public virtual void MustBeAbleToHashAllTypesWith4xHashFunction()
		 {
			  VerifyCanHashAllTypes( ResourceTypes.indexEntryResourceId_4_x );
		 }

		 private interface IIndexEntryHasher
		 {
			  long Hash( long labelId, ExactPredicate[] predicates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"UnnecessaryBoxing", "BooleanConstructorCall"}) private void verifyCanHashAllTypes(IndexEntryHasher hasher)
		 private void VerifyCanHashAllTypes( IndexEntryHasher hasher )
		 {
			  hasher.Hash( 42, array( exact( 1, "" ) ) );
			  hasher.Hash( 42, array( exact( 1, "a" ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "" } ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "a" } ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "a", "b" } ) ) );
			  hasher.Hash( 42, array( exact( 1, true ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{ true } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{ true, false } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?( true ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{ true } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{ true, false } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( sbyte ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToSByte( ( sbyte ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( short ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt16( ( short ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, 'a' ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{ 'a' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{ 'a', 'b' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?( 'a' ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{ 'a' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{ 'a', 'b' } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( float ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?( ( float ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{ 1.0f } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{ 1.0f, 2.0f } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt32( 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt64( 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{ 1L } ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{ 1L, 2L } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1.0 ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?( 1.0 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{ 1.0 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{ 1.0, 2.0 } ) ) );

			  hasher.Hash( 42, array( exact( 1, "" ), exact( ~1, "" ) ) );
			  hasher.Hash( 42, array( exact( 1, "a" ), exact( ~1, "a" ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{} ), exact( ~1, new string[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "" } ), exact( ~1, new string[]{ "" } ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "a" } ), exact( ~1, new string[]{ "a" } ) ) );
			  hasher.Hash( 42, array( exact( 1, new string[]{ "a", "b" } ), exact( ~1, new string[]{ "a", "b" } ) ) );
			  hasher.Hash( 42, array( exact( 1, true ), exact( ~1, true ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{} ), exact( ~1, new bool[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{ true } ), exact( ~1, new bool[]{ true } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool[]{ true, false } ), exact( ~1, new bool[]{ true, false } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?( true ) ), exact( ~1, new bool?( true ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{} ), exact( ~1, new bool?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{ true } ), exact( ~1, new bool?[]{ true } ) ) );
			  hasher.Hash( 42, array( exact( 1, new bool?[]{ true, false } ), exact( ~1, new bool?[]{ true, false } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( sbyte ) 1 ), exact( ~1, ( sbyte ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{} ), exact( ~1, new sbyte[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{ 1 } ), exact( ~1, new sbyte[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte[]{ 1, 2 } ), exact( ~1, new sbyte[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToSByte( ( sbyte ) 1 ) ), exact( ~1, Convert.ToSByte( ( sbyte ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{} ), exact( ~1, new sbyte?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{ 1 } ), exact( ~1, new sbyte?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new sbyte?[]{ 1, 2 } ), exact( ~1, new sbyte?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( short ) 1 ), exact( ~1, ( short ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{} ), exact( ~1, new short[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{ 1 } ), exact( ~1, new short[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new short[]{ 1, 2 } ), exact( ~1, new short[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt16( ( short ) 1 ) ), exact( ~1, Convert.ToInt16( ( short ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{} ), exact( ~1, new short?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{ 1 } ), exact( ~1, new short?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new short?[]{ 1, 2 } ), exact( ~1, new short?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, 'a' ), exact( ~1, 'a' ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{} ), exact( ~1, new char[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{ 'a' } ), exact( ~1, new char[]{ 'a' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char[]{ 'a', 'b' } ), exact( ~1, new char[]{ 'a', 'b' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?( 'a' ) ), exact( ~1, new char?( 'a' ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{} ), exact( ~1, new char?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{ 'a' } ), exact( ~1, new char?[]{ 'a' } ) ) );
			  hasher.Hash( 42, array( exact( 1, new char?[]{ 'a', 'b' } ), exact( ~1, new char?[]{ 'a', 'b' } ) ) );
			  hasher.Hash( 42, array( exact( 1, ( float ) 1 ), exact( ~1, ( float ) 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{} ), exact( ~1, new float[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{ 1 } ), exact( ~1, new float[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float[]{ 1, 2 } ), exact( ~1, new float[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?( ( float ) 1 ) ), exact( ~1, new float?( ( float ) 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{} ), exact( ~1, new float?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{ 1.0f } ), exact( ~1, new float?[]{ 1.0f } ) ) );
			  hasher.Hash( 42, array( exact( 1, new float?[]{ 1.0f, 2.0f } ), exact( ~1, new float?[]{ 1.0f, 2.0f } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1 ), exact( ~1, 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{} ), exact( ~1, new int[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{ 1 } ), exact( ~1, new int[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new int[]{ 1, 2 } ), exact( ~1, new int[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt32( 1 ) ), exact( ~1, Convert.ToInt32( 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{} ), exact( ~1, new int?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{ 1 } ), exact( ~1, new int?[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new int?[]{ 1, 2 } ), exact( ~1, new int?[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1 ), exact( ~1, 1 ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{} ), exact( ~1, new long[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{ 1 } ), exact( ~1, new long[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new long[]{ 1, 2 } ), exact( ~1, new long[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, Convert.ToInt64( 1 ) ), exact( ~1, Convert.ToInt64( 1 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{} ), exact( ~1, new long?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{ 1L } ), exact( ~1, new long?[]{ 1L } ) ) );
			  hasher.Hash( 42, array( exact( 1, new long?[]{ 1L, 2L } ), exact( ~1, new long?[]{ 1L, 2L } ) ) );
			  hasher.Hash( 42, array( exact( 1, 1.0 ), exact( ~1, 1.0 ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{} ), exact( ~1, new double[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{ 1 } ), exact( ~1, new double[]{ 1 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double[]{ 1, 2 } ), exact( ~1, new double[]{ 1, 2 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?( 1.0 ) ), exact( ~1, new double?( 1.0 ) ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{} ), exact( ~1, new double?[]{} ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{ 1.0 } ), exact( ~1, new double?[]{ 1.0 } ) ) );
			  hasher.Hash( 42, array( exact( 1, new double?[]{ 1.0, 2.0 } ), exact( ~1, new double?[]{ 1.0, 2.0 } ) ) );
		 }
	}

}