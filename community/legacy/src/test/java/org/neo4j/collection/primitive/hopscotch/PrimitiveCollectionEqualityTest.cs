﻿using System;
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
namespace Org.Neo4j.Collection.primitive.hopscotch
{
	using DataPoint = org.junit.experimental.theories.DataPoint;
	using Theories = org.junit.experimental.theories.Theories;
	using Theory = org.junit.experimental.theories.Theory;
	using RunWith = org.junit.runner.RunWith;


	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Function;
	using GlobalMemoryTracker = Org.Neo4j.Memory.GlobalMemoryTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @RunWith(Theories.class) public class PrimitiveCollectionEqualityTest
	public class PrimitiveCollectionEqualityTest
	{
		 private interface Value<T> where T : Org.Neo4j.Collection.primitive.PrimitiveCollection
		 {
			  void Add( T coll );

			  /// <returns> 'true' if what was removed was exactly the value that was put in. </returns>
			  bool Remove( T coll );
		 }

		 private abstract class ValueProducer<T> where T : Org.Neo4j.Collection.primitive.PrimitiveCollection
		 {
			  internal readonly Type<T> ApplicableType;

			  internal ValueProducer( Type applicableType )
			  {
					  applicableType = typeof( T );
					this.ApplicableType = applicableType;
			  }

			  public virtual bool IsApplicable<T1>( Factory<T1> factory ) where T1 : Org.Neo4j.Collection.primitive.PrimitiveCollection
			  {
					using ( PrimitiveCollection coll = factory.NewInstance() )
					{
						 return ApplicableType.IsInstanceOfType( coll );
					}
			  }

			  public abstract Value<T> RandomValue();
		 }

		 // ==== Test Value Producers ====

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveIntSet> intV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveIntSet>(org.neo4j.collection.primitive.PrimitiveIntSet.class)
		 public static ValueProducer<PrimitiveIntSet> intV = new ValueProducerAnonymousInnerClass( typeof( PrimitiveIntSet ) );

		 private class ValueProducerAnonymousInnerClass : ValueProducer<PrimitiveIntSet>
		 {
			 public ValueProducerAnonymousInnerClass( Type class )
			 {
				 private readonly PrimitiveCollectionEqualityTest.ValueProducerAnonymousInnerClass _outerInstance;

//JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
//ORIGINAL LINE: public ()
				 public ( PrimitiveCollectionEqualityTest.ValueProducerAnonymousInnerClass outerInstance )
				 {
					 this._outerInstance = outerInstance;
				 }

				 base( class );
			 }

			 public Value<PrimitiveIntSet> randomValue()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int x = randomInt();
				  int x = RandomInt();
				  return new ValueAnonymousInnerClass( this, x );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveLongSet> longV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveLongSet>(org.neo4j.collection.primitive.PrimitiveLongSet.class)
		 public static ValueProducer<PrimitiveLongSet> longV = new ValueProducerAnonymousInnerClass( this, typeof( PrimitiveLongSet ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveIntLongMap> intLongV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveIntLongMap>(org.neo4j.collection.primitive.PrimitiveIntLongMap.class)
		 public static ValueProducer<PrimitiveIntLongMap> intLongV = new ValueProducerAnonymousInnerClass2( this, typeof( PrimitiveIntLongMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveLongIntMap> longIntV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveLongIntMap>(org.neo4j.collection.primitive.PrimitiveLongIntMap.class)
		 public static ValueProducer<PrimitiveLongIntMap> longIntV = new ValueProducerAnonymousInnerClass3( this, typeof( PrimitiveLongIntMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveLongLongMap> longLongV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveLongLongMap>(org.neo4j.collection.primitive.PrimitiveLongLongMap.class)
		 public static ValueProducer<PrimitiveLongLongMap> longLongV = new ValueProducerAnonymousInnerClass4( this, typeof( PrimitiveLongLongMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveIntObjectMap> intObjV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveIntObjectMap>(org.neo4j.collection.primitive.PrimitiveIntObjectMap.class)
		 public static ValueProducer<PrimitiveIntObjectMap> intObjV = new ValueProducerAnonymousInnerClass5( this, typeof( PrimitiveIntObjectMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.neo4j.collection.primitive.PrimitiveLongObjectMap> longObjV = new ValueProducer<org.neo4j.collection.primitive.PrimitiveLongObjectMap>(org.neo4j.collection.primitive.PrimitiveLongObjectMap.class)
		 public static ValueProducer<PrimitiveLongObjectMap> longObjV = new ValueProducerAnonymousInnerClass6( this, typeof( PrimitiveLongObjectMap ) );

		 // ==== Primitive Collection Implementations ====

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntSet> intSet = org.neo4j.collection.primitive.Primitive::intSet;
		 public static Factory<PrimitiveIntSet> IntSet = Primitive.intSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntSet> intSetWithCapacity = () -> org.neo4j.collection.primitive.Primitive.intSet(randomCapacity());
		 public static Factory<PrimitiveIntSet> IntSetWithCapacity = () => Primitive.intSet(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntSet> offheapIntSet = () -> org.neo4j.collection.primitive.Primitive.offHeapIntSet(org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveIntSet> OffheapIntSet = () => Primitive.offHeapIntSet(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntSet> offheapIntSetWithCapacity = () -> org.neo4j.collection.primitive.Primitive.offHeapIntSet(randomCapacity(), org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveIntSet> OffheapIntSetWithCapacity = () => Primitive.offHeapIntSet(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongSet> longSet = org.neo4j.collection.primitive.Primitive::longSet;
		 public static Factory<PrimitiveLongSet> LongSet = Primitive.longSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongSet> longSetWithCapacity = () -> org.neo4j.collection.primitive.Primitive.longSet(randomCapacity());
		 public static Factory<PrimitiveLongSet> LongSetWithCapacity = () => Primitive.longSet(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongSet> offheapLongSet = () -> org.neo4j.collection.primitive.Primitive.offHeapLongSet(org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveLongSet> OffheapLongSet = () => Primitive.offHeapLongSet(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongSet> offheapLongSetWithCapacity = () -> org.neo4j.collection.primitive.Primitive.offHeapLongSet(randomCapacity(), org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveLongSet> OffheapLongSetWithCapacity = () => Primitive.offHeapLongSet(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntLongMap> intLongMap = org.neo4j.collection.primitive.Primitive::intLongMap;
		 public static Factory<PrimitiveIntLongMap> IntLongMap = Primitive.intLongMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntLongMap> intLongMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.intLongMap(randomCapacity());
		 public static Factory<PrimitiveIntLongMap> IntLongMapWithCapacity = () => Primitive.intLongMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongIntMap> longIntMap = org.neo4j.collection.primitive.Primitive::longIntMap;
		 public static Factory<PrimitiveLongIntMap> LongIntMap = Primitive.longIntMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongIntMap> longIntMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.longIntMap(randomCapacity());
		 public static Factory<PrimitiveLongIntMap> LongIntMapWithCapacity = () => Primitive.longIntMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongLongMap> longLongMap = org.neo4j.collection.primitive.Primitive::longLongMap;
		 public static Factory<PrimitiveLongLongMap> LongLongMap = Primitive.longLongMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongLongMap> longLongMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.longLongMap(randomCapacity());
		 public static Factory<PrimitiveLongLongMap> LongLongMapWithCapacity = () => Primitive.longLongMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongLongMap> offheapLongLongMap = () -> org.neo4j.collection.primitive.Primitive.offHeapLongLongMap(org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveLongLongMap> OffheapLongLongMap = () => Primitive.offHeapLongLongMap(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongLongMap> offheapLongLongMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.offHeapLongLongMap(randomCapacity(), org.neo4j.memory.GlobalMemoryTracker.INSTANCE);
		 public static Factory<PrimitiveLongLongMap> OffheapLongLongMapWithCapacity = () => Primitive.offHeapLongLongMap(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntObjectMap> intObjMap = org.neo4j.collection.primitive.Primitive::intObjectMap;
		 public static Factory<PrimitiveIntObjectMap> IntObjMap = Primitive.intObjectMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveIntObjectMap> intObjMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.intObjectMap(randomCapacity());
		 public static Factory<PrimitiveIntObjectMap> IntObjMapWithCapacity = () => Primitive.intObjectMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongObjectMap> longObjectMap = org.neo4j.collection.primitive.Primitive::longObjectMap;
		 public static Factory<PrimitiveLongObjectMap> LongObjectMap = Primitive.longObjectMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveLongObjectMap> longObjectMapWithCapacity = () -> org.neo4j.collection.primitive.Primitive.longObjectMap(randomCapacity());
		 public static Factory<PrimitiveLongObjectMap> LongObjectMapWithCapacity = () => Primitive.longObjectMap(RandomCapacity());

		 private static final PrimitiveIntSet _observedRandomInts = Primitive.intSet();
		 private static final PrimitiveLongSet _observedRandomLongs = Primitive.longSet();

		 /// <summary>
		 /// Produce a random int that hasn't been seen before by any test.
		 /// </summary>
		 private static int RandomInt()
		 {
			  int n;
			  do
			  {
					n = ThreadLocalRandom.current().Next();
			  } while ( n == -1 || !_observedRandomInts.add( n ) );
			  return n;
		 }

		 /// <summary>
		 /// Produce a random long that hasn't been seen before by any test.
		 /// </summary>
		 private static long RandomLong()
		 {
			  long n;
			  do
			  {
					n = ThreadLocalRandom.current().nextLong();
			  } while ( n == -1 || !_observedRandomLongs.add( n ) );
			  return n;
		 }

		 private static int RandomCapacity()
		 {
			  return ThreadLocalRandom.current().Next(30, 1200);
		 }

		 private static void assertEquals( PrimitiveCollection a, PrimitiveCollection b )
		 {
			  assertThat( a, @is( equalTo( b ) ) );
			  assertThat( b, @is( equalTo( a ) ) );
			  assertThat( a.GetHashCode(), @is(equalTo(b.GetHashCode())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void collectionsAreNotEqualToObjectsOfOtherTypes(org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factory)
		 public void collectionsAreNotEqualToObjectsOfOtherTypes( Factory<PrimitiveCollection> factory )
		 {
			  using ( PrimitiveCollection coll = factory.newInstance() )
			  {
					assertNotEquals( coll, new object() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void emptyCollectionsAreEqual(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryA, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryB)
		 public void emptyCollectionsAreEqual( ValueProducer values, Factory<PrimitiveCollection> factoryA, Factory<PrimitiveCollection> factoryB )
		 {
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					AssertEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void addingTheSameValuesMustProduceEqualCollections(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryA, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryB)
		 public void addingTheSameValuesMustProduceEqualCollections( ValueProducer values, Factory<PrimitiveCollection> factoryA, Factory<PrimitiveCollection> factoryB )
		 {
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					Value value = values.randomValue();
					value.add( a );
					value.add( b );
					AssertEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void addingDifferentValuesMustProduceUnequalCollections(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryA, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryB)
		 public void addingDifferentValuesMustProduceUnequalCollections( ValueProducer values, Factory<PrimitiveCollection> factoryA, Factory<PrimitiveCollection> factoryB )
		 {
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					values.randomValue().add(a);
					values.randomValue().add(b);
					assertNotEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void differentButEquivalentMutationsShouldProduceEqualCollections(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryA, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryB)
		 public void differentButEquivalentMutationsShouldProduceEqualCollections( ValueProducer values, Factory<PrimitiveCollection> factoryA, Factory<PrimitiveCollection> factoryB )
		 {
			  // Note that this test, cute as it is, also verifies that the hashCode implementation is order-invariant :)
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					Value x = values.randomValue();
					Value y = values.randomValue();
					Value z = values.randomValue();

					x.add( a );
					z.add( a );

					z.add( b );
					y.add( b );
					x.add( b );
					y.remove( b );

					AssertEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void capacityDifferencesMustNotInfluenceEquality(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryA, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factoryB)
		 public void capacityDifferencesMustNotInfluenceEquality( ValueProducer values, Factory<PrimitiveCollection> factoryA, Factory<PrimitiveCollection> factoryB )
		 {
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					IList<Value> tmps = new List<Value>();
					for ( int i = 0; i < 5000; i++ )
					{
						 Value value = values.randomValue();
						 value.add( b );
						 tmps.Add( value );
					}

					Value specificValue = values.randomValue();
					specificValue.add( a );
					specificValue.add( b );

					for ( int i = 0; i < 5000; i++ )
					{
						 Value value = values.randomValue();
						 value.add( b );
						 tmps.Add( value );
					}

					Collections.shuffle( tmps );
					foreach ( Value value in tmps )
					{
						 value.remove( b );
					}

					AssertEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void hashCodeMustFollowValues(ValueProducer values, org.neo4j.function.Factory<org.neo4j.collection.primitive.PrimitiveCollection> factory)
		 public void hashCodeMustFollowValues( ValueProducer values, Factory<PrimitiveCollection> factory )
		 {
			  assumeTrue( values.isApplicable( factory ) );
			  using ( PrimitiveCollection a = factory.newInstance() )
			  {
					Value x = values.randomValue();
					Value y = values.randomValue();
					Value z = values.randomValue();

					int i = a.GetHashCode();
					x.add( a );
					int j = a.GetHashCode();
					y.add( a );
					int k = a.GetHashCode();
					z.add( a );
					int l = a.GetHashCode();
					z.remove( a );
					int m = a.GetHashCode();
					y.remove( a );
					int n = a.GetHashCode();
					x.remove( a );
					int o = a.GetHashCode();

					assertThat( "0 elm hashcode equal", o, @is( i ) );
					assertThat( "1 elm hashcode equal", n, @is( j ) );
					assertThat( "2 elm hashcode equal", m, @is( k ) );
					assertThat( "3 elm hashcode distinct", l, not( isOneOf( i, j, k, m, n, o ) ) );
					assertThat( "2 elm hashcode distinct", k, not( isOneOf( i, j, l, n, o ) ) );
					assertThat( "1 elm hashcode distinct", n, not( isOneOf( i, k, l, m, o ) ) );
					assertThat( "0 elm hashcode distinct", i, not( isOneOf( j, k, l, m, n ) ) );
			  }
		 }
	}

}