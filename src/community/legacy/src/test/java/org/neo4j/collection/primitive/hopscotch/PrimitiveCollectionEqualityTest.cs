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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using DataPoint = org.junit.experimental.theories.DataPoint;
	using Theories = org.junit.experimental.theories.Theories;
	using Theory = org.junit.experimental.theories.Theory;
	using RunWith = org.junit.runner.RunWith;


	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Neo4Net.Functions;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;

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
		 private interface Value<T> where T : Neo4Net.Collections.primitive.PrimitiveCollection
		 {
			  void Add( T coll );

			  /// <returns> 'true' if what was removed was exactly the value that was put in. </returns>
			  bool Remove( T coll );
		 }

		 private abstract class ValueProducer<T> where T : Neo4Net.Collections.primitive.PrimitiveCollection
		 {
			  internal readonly Type<T> ApplicableType;

			  internal ValueProducer( Type applicableType )
			  {
					  applicableType = typeof( T );
					this.ApplicableType = applicableType;
			  }

			  public virtual bool IsApplicable<T1>( IFactory<T1> factory ) where T1 : Neo4Net.Collections.primitive.PrimitiveCollection
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
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntSet> intV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntSet>(org.Neo4Net.collection.primitive.PrimitiveIntSet.class)
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
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongSet> longV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongSet>(org.Neo4Net.collection.primitive.PrimitiveLongSet.class)
		 public static ValueProducer<PrimitiveLongSet> longV = new ValueProducerAnonymousInnerClass( this, typeof( PrimitiveLongSet ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntLongMap> intLongV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntLongMap>(org.Neo4Net.collection.primitive.PrimitiveIntLongMap.class)
		 public static ValueProducer<PrimitiveIntLongMap> intLongV = new ValueProducerAnonymousInnerClass2( this, typeof( PrimitiveIntLongMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongIntMap> longIntV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongIntMap>(org.Neo4Net.collection.primitive.PrimitiveLongIntMap.class)
		 public static ValueProducer<PrimitiveLongIntMap> longIntV = new ValueProducerAnonymousInnerClass3( this, typeof( PrimitiveLongIntMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongLongMap> longLongV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongLongMap>(org.Neo4Net.collection.primitive.PrimitiveLongLongMap.class)
		 public static ValueProducer<PrimitiveLongLongMap> longLongV = new ValueProducerAnonymousInnerClass4( this, typeof( PrimitiveLongLongMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntObjectMap> intObjV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveIntObjectMap>(org.Neo4Net.collection.primitive.PrimitiveIntObjectMap.class)
		 public static ValueProducer<PrimitiveIntObjectMap> intObjV = new ValueProducerAnonymousInnerClass5( this, typeof( PrimitiveIntObjectMap ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongObjectMap> longObjV = new ValueProducer<org.Neo4Net.collection.primitive.PrimitiveLongObjectMap>(org.Neo4Net.collection.primitive.PrimitiveLongObjectMap.class)
		 public static ValueProducer<PrimitiveLongObjectMap> longObjV = new ValueProducerAnonymousInnerClass6( this, typeof( PrimitiveLongObjectMap ) );

		 // ==== Primitive Collection Implementations ====

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntSet> intSet = org.Neo4Net.collection.primitive.Primitive::intSet;
		 public static IFactory<PrimitiveIntSet> IntSet = Primitive.intSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntSet> intSetWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.intSet(randomCapacity());
		 public static IFactory<PrimitiveIntSet> IntSetWithCapacity = () => Primitive.intSet(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntSet> offheapIntSet = () -> org.Neo4Net.collection.primitive.Primitive.offHeapIntSet(org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveIntSet> OffheapIntSet = () => Primitive.offHeapIntSet(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntSet> offheapIntSetWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.offHeapIntSet(randomCapacity(), org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveIntSet> OffheapIntSetWithCapacity = () => Primitive.offHeapIntSet(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongSet> longSet = org.Neo4Net.collection.primitive.Primitive::longSet;
		 public static IFactory<PrimitiveLongSet> LongSet = Primitive.longSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongSet> longSetWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.longSet(randomCapacity());
		 public static IFactory<PrimitiveLongSet> LongSetWithCapacity = () => Primitive.longSet(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongSet> offheapLongSet = () -> org.Neo4Net.collection.primitive.Primitive.offHeapLongSet(org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveLongSet> OffheapLongSet = () => Primitive.offHeapLongSet(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongSet> offheapLongSetWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.offHeapLongSet(randomCapacity(), org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveLongSet> OffheapLongSetWithCapacity = () => Primitive.offHeapLongSet(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntLongMap> intLongMap = org.Neo4Net.collection.primitive.Primitive::intLongMap;
		 public static IFactory<PrimitiveIntLongMap> IntLongMap = Primitive.intLongMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntLongMap> intLongMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.intLongMap(randomCapacity());
		 public static IFactory<PrimitiveIntLongMap> IntLongMapWithCapacity = () => Primitive.intLongMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongIntMap> longIntMap = org.Neo4Net.collection.primitive.Primitive::longIntMap;
		 public static IFactory<PrimitiveLongIntMap> LongIntMap = Primitive.longIntMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongIntMap> longIntMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.longIntMap(randomCapacity());
		 public static IFactory<PrimitiveLongIntMap> LongIntMapWithCapacity = () => Primitive.longIntMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongLongMap> longLongMap = org.Neo4Net.collection.primitive.Primitive::longLongMap;
		 public static IFactory<PrimitiveLongLongMap> LongLongMap = Primitive.longLongMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongLongMap> longLongMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.longLongMap(randomCapacity());
		 public static IFactory<PrimitiveLongLongMap> LongLongMapWithCapacity = () => Primitive.longLongMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongLongMap> offheapLongLongMap = () -> org.Neo4Net.collection.primitive.Primitive.offHeapLongLongMap(org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveLongLongMap> OffheapLongLongMap = () => Primitive.offHeapLongLongMap(GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongLongMap> offheapLongLongMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.offHeapLongLongMap(randomCapacity(), org.Neo4Net.memory.GlobalMemoryTracker.INSTANCE);
		 public static IFactory<PrimitiveLongLongMap> OffheapLongLongMapWithCapacity = () => Primitive.offHeapLongLongMap(RandomCapacity(), GlobalMemoryTracker.INSTANCE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntObjectMap> intObjMap = org.Neo4Net.collection.primitive.Primitive::intObjectMap;
		 public static IFactory<PrimitiveIntObjectMap> IntObjMap = Primitive.intObjectMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveIntObjectMap> intObjMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.intObjectMap(randomCapacity());
		 public static IFactory<PrimitiveIntObjectMap> IntObjMapWithCapacity = () => Primitive.intObjectMap(RandomCapacity());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongObjectMap> longObjectMap = org.Neo4Net.collection.primitive.Primitive::longObjectMap;
		 public static IFactory<PrimitiveLongObjectMap> LongObjectMap = Primitive.longObjectMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoint public static org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveLongObjectMap> longObjectMapWithCapacity = () -> org.Neo4Net.collection.primitive.Primitive.longObjectMap(randomCapacity());
		 public static IFactory<PrimitiveLongObjectMap> LongObjectMapWithCapacity = () => Primitive.longObjectMap(RandomCapacity());

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
//ORIGINAL LINE: @Theory public void collectionsAreNotEqualToObjectsOfOtherTypes(org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factory)
		 public void collectionsAreNotEqualToObjectsOfOtherTypes( IFactory<PrimitiveCollection> factory )
		 {
			  using ( PrimitiveCollection coll = factory.newInstance() )
			  {
					assertNotEquals( coll, new object() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void emptyCollectionsAreEqual(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryA, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryB)
		 public void emptyCollectionsAreEqual( ValueProducer values, IFactory<PrimitiveCollection> factoryA, IFactory<PrimitiveCollection> factoryB )
		 {
			  assumeTrue( values.isApplicable( factoryA ) );
			  assumeTrue( values.isApplicable( factoryB ) );
			  using ( PrimitiveCollection a = factoryA.newInstance(), PrimitiveCollection b = factoryB.newInstance() )
			  {
					AssertEquals( a, b );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void addingTheSameValuesMustProduceEqualCollections(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryA, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryB)
		 public void addingTheSameValuesMustProduceEqualCollections( ValueProducer values, IFactory<PrimitiveCollection> factoryA, IFactory<PrimitiveCollection> factoryB )
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
//ORIGINAL LINE: @Theory public void addingDifferentValuesMustProduceUnequalCollections(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryA, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryB)
		 public void addingDifferentValuesMustProduceUnequalCollections( ValueProducer values, IFactory<PrimitiveCollection> factoryA, IFactory<PrimitiveCollection> factoryB )
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
//ORIGINAL LINE: @Theory public void differentButEquivalentMutationsShouldProduceEqualCollections(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryA, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryB)
		 public void differentButEquivalentMutationsShouldProduceEqualCollections( ValueProducer values, IFactory<PrimitiveCollection> factoryA, IFactory<PrimitiveCollection> factoryB )
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
//ORIGINAL LINE: @Theory public void capacityDifferencesMustNotInfluenceEquality(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryA, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factoryB)
		 public void capacityDifferencesMustNotInfluenceEquality( ValueProducer values, IFactory<PrimitiveCollection> factoryA, IFactory<PrimitiveCollection> factoryB )
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
//ORIGINAL LINE: @Theory public void hashCodeMustFollowValues(ValueProducer values, org.Neo4Net.function.Factory<org.Neo4Net.collection.primitive.PrimitiveCollection> factory)
		 public void hashCodeMustFollowValues( ValueProducer values, IFactory<PrimitiveCollection> factory )
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