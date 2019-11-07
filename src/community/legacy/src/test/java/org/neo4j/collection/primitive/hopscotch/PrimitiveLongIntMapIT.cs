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
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Test.randomized;
	using LinePrinter = Neo4Net.Test.randomized.LinePrinter;
	using Printable = Neo4Net.Test.randomized.Printable;
	using Neo4Net.Test.randomized;
	using Neo4Net.Test.randomized.RandomizedTester;
	using Neo4Net.Test.randomized.RandomizedTester;
	using Neo4Net.Test.randomized;
	using TestResource = Neo4Net.Test.randomized.TestResource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class PrimitiveLongIntMapIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void thoroughlyTestIt()
		 internal virtual void ThoroughlyTestIt()
		 {
			  long endTime = currentTimeMillis() + SECONDS.toMillis(5);
			  while ( currentTimeMillis() < endTime )
			  {
					long seed = currentTimeMillis();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Random random = new java.util.Random(seed);
					Random random = new Random( seed );
					int max = random.Next( 10_000 ) + 100;
					RandomizedTester<Maps, string> actions = new RandomizedTester<Maps, string>( MapFactory(), ActionFactory(random) );

					Result<Maps, string> result = actions.Run( max );
					if ( result.Failure )
					{
						 Console.WriteLine( "Found failure at " + result );
						 actions.TestCaseWriter( "shouldOnlyContainAddedValues", Given() ).print(System.out);
						 Console.WriteLine( "Actually, minimal reproducible test of that is..." );
						 actions.FindMinimalReproducible().testCaseWriter("shouldOnlyContainAddedValues", Given()).print(System.out);
						 fail( "Failed, see printed test case for how to reproduce. Seed:" + seed );
					}

					FullVerification( result.Target, random );
			  }
		 }

		 private class ValueAnonymousInnerClass : Value<PrimitiveLongSet>
		 {
			 private readonly ValueProducerAnonymousInnerClass outerInstance;

			 private long x;

			 public ValueAnonymousInnerClass( ValueProducerAnonymousInnerClass outerInstance, long x )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
			 }

			 public override void add( PrimitiveLongSet coll )
			 {
				  coll.Add( x );
			 }

			 public override bool remove( PrimitiveLongSet coll )
			 {
				  return coll.Remove( x );
			 }
		 }

		 private class ValueAnonymousInnerClass2 : Value<PrimitiveIntLongMap>
		 {
			 private readonly PrimitiveIntObjectHashMapTest outerInstance;

			 private int x;
			 private long y;

			 public ValueAnonymousInnerClass2( PrimitiveIntObjectHashMapTest outerInstance, int x, long y )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
				 this.y = y;
			 }

			 public override void add( PrimitiveIntLongMap coll )
			 {
				  coll.Put( x, y );
			 }

			 public override bool remove( PrimitiveIntLongMap coll )
			 {
				  return coll.Remove( x ) == y;
			 }
		 }

		 private class ValueAnonymousInnerClass3 : Value<PrimitiveLongIntMap>
		 {
			 private readonly PrimitiveIntObjectHashMapTest outerInstance;

			 private long x;
			 private int y;

			 public ValueAnonymousInnerClass3( PrimitiveIntObjectHashMapTest outerInstance, long x, int y )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
				 this.y = y;
			 }

			 public override void add( PrimitiveLongIntMap coll )
			 {
				  coll.Put( x, y );
			 }

			 public override bool remove( PrimitiveLongIntMap coll )
			 {
				  return coll.Remove( x ) == y;
			 }
		 }

		 private class ValueAnonymousInnerClass4 : Value<PrimitiveLongLongMap>
		 {
			 private readonly PrimitiveIntObjectHashMapTest outerInstance;

			 private long x;
			 private long y;

			 public ValueAnonymousInnerClass4( PrimitiveIntObjectHashMapTest outerInstance, long x, long y )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
				 this.y = y;
			 }

			 public override void add( PrimitiveLongLongMap coll )
			 {
				  coll.Put( x, y );
			 }

			 public override bool remove( PrimitiveLongLongMap coll )
			 {
				  return coll.Remove( x ) == y;
			 }
		 }

		 private class ValueAnonymousInnerClass5 : Value<Neo4Net.Collections.primitive.PrimitiveIntObjectMap>
		 {
			 private readonly PrimitiveIntObjectHashMapTest outerInstance;

			 private int x;
			 private object y;

			 public ValueAnonymousInnerClass5( PrimitiveIntObjectHashMapTest outerInstance, int x, object y )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
				 this.y = y;
			 }

			 public override void add( PrimitiveIntObjectMap coll )
			 {
				  coll.put( x, y );
			 }

			 public override bool remove( PrimitiveIntObjectMap coll )
			 {
				  return coll.remove( x ) == y;
			 }
		 }

		 private class ValueAnonymousInnerClass6 : Value<PrimitiveLongObjectMap>
		 {
			 private readonly PrimitiveIntObjectHashMapTest outerInstance;

			 private long x;
			 private object y;

			 public ValueAnonymousInnerClass6( PrimitiveIntObjectHashMapTest outerInstance, long x, object y )
			 {
				 this.outerInstance = outerInstance;
				 this.x = x;
				 this.y = y;
			 }

			 public override void add( PrimitiveLongObjectMap coll )
			 {
				  coll.put( x, y );
			 }

			 public override bool remove( PrimitiveLongObjectMap coll )
			 {
				  return coll.remove( x ) == y;
			 }
		 }

		 private static void FullVerification( Maps target, Random random )
		 {
			  foreach ( KeyValuePair<long, int> entry in target.NormalMap.SetOfKeyValuePairs() )
			  {
					assertTrue( target.Map.containsKey( entry.Key ) );
					assertEquals( entry.Value.intValue(), target.Map.get(entry.Key) );
			  }

			  for ( int i = 0; i < target.NormalMap.Count; i++ )
			  {
					assertFalse( target.Map.containsKey( RandomNonExisting( random, target.NormalMap ) ) );
			  }
		 }

		 private static Printable Given()
		 {
			  return @out => @out.println( typeof( PrimitiveLongIntMap ).Name + " map = " + typeof( Primitive ).Name + ".longIntMap();" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Neo4Net.test.randomized.RandomizedTester.ActionFactory<Maps, String> actionFactory(final java.util.Random random)
		 private static RandomizedTester.ActionFactory<Maps, string> ActionFactory( Random random )
		 {
			  return from => GenerateAction( random, from );
		 }

		 private static RandomizedTester.TargetFactory<Maps> MapFactory()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Maps::new;
		 }

		 private static Action<Maps, string> GenerateAction( Random random, Maps from )
		 {
			  bool anExisting = from.NormalMap.Count > 0 && random.Next( 3 ) == 0;
			  long key = anExisting ? RandomExisting( random, from.NormalMap ) : RandomNonExisting( random, from.NormalMap );
			  int value = random.Next( 100 );

			  int typeOfAction = random.Next( 5 );
			  if ( typeOfAction == 0 )
			  { // remove
					return new RemoveAction( key );
			  }

			  // add
			  return new AddAction( key, value );
		 }

		 private static long RandomNonExisting( Random random, IDictionary<long, int> existing )
		 {
			  while ( true )
			  {
					long key = Math.Abs( random.nextLong() );
					if ( !existing.ContainsKey( key ) )
					{
						 return key;
					}
			  }
		 }

		 private static long RandomExisting( Random random, IDictionary<long, int> existing )
		 {
			  int index = random.Next( existing.Count ) + 1;
			  IEnumerator<long> iterator = existing.Keys.GetEnumerator();
			  long value = 0;
			  for ( int i = 0; i < index; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					value = iterator.next();
			  }
			  return value;
		 }

		 private class AddAction : Action<Maps, string>
		 {
			  internal readonly long Key;
			  internal readonly int Value;

			  internal AddAction( long key, int value )
			  {
					this.Key = key;
					this.Value = value;
			  }

			  public override string Apply( Maps target )
			  {
					bool existing = target.NormalMap.ContainsKey( Key );
					int existingValue = existing ? target.NormalMap[Key] : -1;
					int actualSizeBefore = target.NormalMap.Count;

					int sizeBefore = target.Map.size();
					bool existedBefore = target.Map.containsKey( Key );
					int valueBefore = target.Map.get( Key );
					int previous = target.Map.put( Key, Value );
					bool existsAfter = target.Map.containsKey( Key );
					int valueAfter = target.Map.get( Key );
					target.NormalMap[Key] = Value;
					int sizeAfter = target.Map.size();

					int actualSizeAfter = target.NormalMap.Count;
					bool ok = ( sizeBefore == actualSizeBefore ) & ( existedBefore == existing ) & ( existingValue == valueBefore ) & ( existingValue == previous ) & ( valueAfter == Value ) & existsAfter & ( sizeAfter == actualSizeAfter );
					return ok ? null : "" + Key + ":" + Value + "," + existingValue + "," + existedBefore +
							  "," + previous + "," + existsAfter;
			  }

			  public override void PrintAsCode( Maps source, LinePrinter @out, bool includeChecks )
			  {
					string addition = "map.put( " + Key + ", " + Value + " );";
					if ( includeChecks )
					{
						 bool existing = source.NormalMap.ContainsKey( Key );
						 int existingValue = existing ? source.NormalMap[Key] : -1;
						 @out.Println( "int sizeBefore = map.size();" );
						 @out.Println( format( "boolean existedBefore = map.containsKey( %d );", Key ) );
						 @out.Println( format( "int valueBefore = map.get( %d );", Key ) );
						 @out.Println( format( "int previous = %s", addition ) );
						 @out.Println( format( "boolean existsAfter = map.containsKey( %d );", Key ) );
						 @out.Println( format( "int valueAfter = map.get( %d );", Key ) );
						 @out.Println( "int sizeAfter = map.size();" );

						 int actualSizeBefore = source.NormalMap.Count;
						 @out.Println( format( "assertEquals( \"%s\", %d, sizeBefore );", "Size before put should have been " + actualSizeBefore, actualSizeBefore ) );
						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( existing ), Key + " should " + ( existing ? "" : "not " ) + "exist before putting here" ) );
						 @out.Println( format( "assertEquals( \"%s\", %d, valueBefore );", "value before should be " + existingValue, existingValue ) );
						 @out.Println( format( "assertEquals( \"%s\", %d, previous );", "value returned from put should be " + existingValue, existingValue ) );
						 @out.Println( format( "assertTrue( \"%s\", existsAfter );", Key + " should exist" ) );
						 @out.Println( format( "assertEquals( \"%s\", %d, valueAfter );", "value after putting should be " + Value, Value ) );
						 int actualSizeAfter = existing ? actualSizeBefore : actualSizeBefore + 1;
						 @out.Println( format( "assertEquals( \"%s\", %d, sizeAfter );", "Size after put should have been " + actualSizeAfter, actualSizeAfter ) );
					}
					else
					{
						 @out.Println( addition );
					}
			  }
		 }

		 private class RemoveAction : Action<Maps, string>
		 {
			  internal readonly long Key;

			  internal RemoveAction( long key )
			  {
					this.Key = key;
			  }

			  public override string Apply( Maps target )
			  {
					bool existing = target.NormalMap.ContainsKey( Key );
					int existingValue = existing ? target.NormalMap[Key] : -1;

					bool existedBefore = target.Map.containsKey( Key );
					int valueBefore = target.Map.get( Key );
					int removed = target.Map.remove( Key );
					bool existsAfter = target.Map.containsKey( Key );
					int valueAfter = target.Map.get( Key );
					target.NormalMap.Remove( Key );

					bool ok = ( existedBefore == existing ) & ( existingValue == valueBefore ) & ( existingValue == removed ) & ( valueAfter == -1 ) & !existsAfter;
					return ok ? null : "" + Key + "," + existingValue + "," + existedBefore +
							  "," + removed + "," + existsAfter;
			  }

			  public override void PrintAsCode( Maps source, LinePrinter @out, bool includeChecks )
			  {
					string removal = "map.remove( " + Key + " );";
					if ( includeChecks )
					{
						 bool existing = source.NormalMap.ContainsKey( Key );
						 int existingValue = existing ? source.NormalMap[Key] : -1;
						 @out.Println( format( "boolean existedBefore = map.containsKey( %d );", Key ) );
						 @out.Println( format( "int valueBefore = map.get( %d );", Key ) );
						 @out.Println( format( "int removed = %s", removal ) );
						 @out.Println( format( "boolean existsAfter = map.containsKey( %d );", Key ) );
						 @out.Println( format( "int valueAfter = map.get( %d );", Key ) );

						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( existing ), Key + " should " + ( existing ? "" : "not " ) + "exist before removing here" ) );
						 @out.Println( format( "assertEquals( \"%s\", %d, valueBefore );", "value before should be " + existingValue, existingValue ) );
						 @out.Println( format( "assertEquals( \"%s\", %d, removed );", "value returned from remove should be " + existingValue, existingValue ) );
						 @out.Println( format( "assertFalse( \"%s\", existsAfter );", Key + " should not exist" ) );
						 @out.Println( format( "assertEquals( \"%s\", -1, valueAfter );", "value after removing should be -1" ) );
					}
					else
					{
						 @out.Println( removal );
					}
			  }
		 }

		 private static string Capitilize( bool @bool )
		 {
			  string @string = Convert.ToBoolean( @bool ).ToString();
			  return @string.Substring( 0, 1 ).ToUpper() + @string.Substring(1).ToLower();
		 }

		 private class Maps : TestResource
		 {
			  internal readonly IDictionary<long, int> NormalMap = new Dictionary<long, int>();
			  internal readonly PrimitiveLongIntMap Map = Primitive.longIntMap();

			  public override string ToString()
			  {
					return Map.ToString();
			  }

			  public override void Close()
			  {
			  }
		 }
	}

}