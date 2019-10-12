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
namespace Org.Neo4j.Collection.primitive.hopscotch
{
	using Test = org.junit.jupiter.api.Test;

	using Org.Neo4j.Collection.primitive;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class PrimitiveIntObjectHashMapTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotComputeValueIfPresent()
		 internal virtual void DoNotComputeValueIfPresent()
		 {
			  PrimitiveIntObjectMap<object> intObjectMap = Primitive.intObjectMap();
			  intObjectMap.Put( 1, "a" );
			  assertEquals( "a", intObjectMap.ComputeIfAbsent( 1, value => "b" ) );
		 }

		 private class ValueAnonymousInnerClass : Value<Org.Neo4j.Collection.primitive.PrimitiveIntSet>
		 {
			 private readonly ValueProducerAnonymousInnerClass outerInstance;

			 private int x;

			 public ValueAnonymousInnerClass( ValueProducerAnonymousInnerClass outerInstance, int x )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
			 }

			 public void add( PrimitiveIntSet coll )
			 {
				  coll.Add( x );
			 }

			 public bool remove( PrimitiveIntSet coll )
			 {
				  return coll.Remove( x );
			 }
		 }

		 private class ValueProducerAnonymousInnerClass : ValueProducer<Org.Neo4j.Collection.primitive.PrimitiveLongSet>
		 {
			 private readonly PrimitiveCollectionEqualityTest outerInstance;

			 public ValueProducerAnonymousInnerClass( PrimitiveCollectionEqualityTest outerInstance, Type class )
			 {
				 private readonly PrimitiveIntObjectHashMapTest.ValueProducerAnonymousInnerClass _outerInstance;

//JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
//ORIGINAL LINE: public ()
				 public ( PrimitiveIntObjectHashMapTest.ValueProducerAnonymousInnerClass outerInstance )
				 {
					 this._outerInstance = outerInstance;
				 }

				 base( class );
				 this._outerInstance = _outerInstance;
			 }

			 public Value<PrimitiveLongSet> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long x = randomLong();
				  long x = randomLong();
				  return new ValueAnonymousInnerClass( this, x );
			 }
		 }

		 private static class ValueProducerAnonymousInnerClass2 extends ValueProducer<Org.Neo4j.Collection.primitive.PrimitiveIntLongMap>
		 {
			 private final PrimitiveCollectionEqualityTest outerInstance;

			 public ValueProducerAnonymousInnerClass2( PrimitiveCollectionEqualityTest outerInstance, Type class )
			 {
				 base( class );
				 this.outerInstance = outerInstance;
			 }

			 public Value<PrimitiveIntLongMap> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int x = randomInt();
				  int x = randomInt();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long y = randomLong();
				  long y = randomLong();
				  return new ValueAnonymousInnerClass2( this, x, y );
			 }
		 }

		 private static class ValueProducerAnonymousInnerClass3 extends ValueProducer<Org.Neo4j.Collection.primitive.PrimitiveLongIntMap>
		 {
			 private final PrimitiveCollectionEqualityTest _outerInstance;

			 public ValueProducerAnonymousInnerClass3( PrimitiveCollectionEqualityTest _outerInstance, Type class )
			 {
				 base( class );
				 this._outerInstance = _outerInstance;
			 }

			 public Value<PrimitiveLongIntMap> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long x = randomLong();
				  long x = randomLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int y = randomInt();
				  int y = randomInt();
				  return new ValueAnonymousInnerClass3( this, x, y );
			 }
		 }

		 private static class ValueProducerAnonymousInnerClass4 extends ValueProducer<Org.Neo4j.Collection.primitive.PrimitiveLongLongMap>
		 {
			 private final PrimitiveCollectionEqualityTest _outerInstance;

			 public ValueProducerAnonymousInnerClass4( PrimitiveCollectionEqualityTest _outerInstance, Type class )
			 {
				 base( class );
				 this._outerInstance = _outerInstance;
			 }

			 public Value<PrimitiveLongLongMap> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long x = randomLong();
				  long x = randomLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long y = randomLong();
				  long y = randomLong();
				  return new ValueAnonymousInnerClass4( this, x, y );
			 }
		 }

		 private static class ValueProducerAnonymousInnerClass5 extends ValueProducer<PrimitiveIntObjectMap>
		 {
			 private final PrimitiveCollectionEqualityTest _outerInstance;

			 public ValueProducerAnonymousInnerClass5( PrimitiveCollectionEqualityTest _outerInstance, Type class )
			 {
				 base( class );
				 this._outerInstance = _outerInstance;
			 }

			 public Value<PrimitiveIntObjectMap> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int x = randomInt();
				  int x = randomInt();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object y = new Object();
				  object y = new object();
				  return new ValueAnonymousInnerClass5( this, x, y );
			 }
		 }

		 private static class ValueProducerAnonymousInnerClass6 extends ValueProducer<Org.Neo4j.Collection.primitive.PrimitiveLongObjectMap>
		 {
			 private final PrimitiveCollectionEqualityTest _outerInstance;

			 public ValueProducerAnonymousInnerClass6( PrimitiveCollectionEqualityTest _outerInstance, Type class )
			 {
				 base( class );
				 this._outerInstance = _outerInstance;
			 }

			 public Value<PrimitiveLongObjectMap> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long x = randomLong();
				  long x = randomLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object y = new Object();
				  object y = new object();
				  return new ValueAnonymousInnerClass6( this, x, y );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void computeValueIfAbsent()
		 void computeValueIfAbsent()
		 {
			  PrimitiveIntObjectMap<object> intObjectMap = Primitive.intObjectMap();
			  intObjectMap.Put( 1, "a" );
			  assertEquals( "b", intObjectMap.ComputeIfAbsent( 2, value => "b" ) );
		 }
	}

}