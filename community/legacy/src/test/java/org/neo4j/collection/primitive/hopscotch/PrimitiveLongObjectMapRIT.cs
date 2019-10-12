using System;
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
	using Test = org.junit.jupiter.api.Test;


	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Test.randomized;
	using LinePrinter = Org.Neo4j.Test.randomized.LinePrinter;
	using Printable = Org.Neo4j.Test.randomized.Printable;
	using Org.Neo4j.Test.randomized;
	using Org.Neo4j.Test.randomized.RandomizedTester;
	using Org.Neo4j.Test.randomized.RandomizedTester;
	using Org.Neo4j.Test.randomized;
	using TestResource = Org.Neo4j.Test.randomized.TestResource;

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

	internal class PrimitiveLongObjectMapRIT
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

		 private static void FullVerification( Maps target, Random random )
		 {
			  foreach ( KeyValuePair<long, int> entry in target.NormalMap.SetOfKeyValuePairs() )
			  {
					assertTrue( target.Map.containsKey( entry.Key ) );
					assertEquals( entry.Value, target.Map.get( entry.Key ) );
			  }

			  for ( int i = 0; i < target.NormalMap.Count; i++ )
			  {
					assertFalse( target.Map.containsKey( RandomNonExisting( random, target.NormalMap ) ) );
			  }
		 }

		 private static Printable Given()
		 {
			  return @out => @out.println( typeof( PrimitiveLongObjectMap ).Name + "<Integer> map = " + typeof( Primitive ).Name + ".longObjectMap();" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.test.randomized.RandomizedTester.ActionFactory<Maps, String> actionFactory(final java.util.Random random)
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
			  int? value = random.Next( 100 );

			  int typeOfAction = random.Next( 5 );
			  if ( typeOfAction == 0 )
			  { // remove
					return new RemoveAction( key );
			  }

			  // add
			  return new AddAction( key, value.Value );
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
			  internal readonly int? Value;

			  internal AddAction( long key, int? value )
			  {
					this.Key = key;
					this.Value = value;
			  }

			  public override string Apply( Maps target )
			  {
					int? existingValue = target.NormalMap[Key];
					int actualSizeBefore = target.NormalMap.Count;

					int sizeBefore = target.Map.size();
					bool existedBefore = target.Map.containsKey( Key );
					int? valueBefore = target.Map.get( Key );
					int? previous = target.Map.put( Key, Value.Value );
					bool existsAfter = target.Map.containsKey( Key );
					int? valueAfter = target.Map.get( Key );
					target.NormalMap[Key] = Value.Value;
					int sizeAfter = target.Map.size();

					int actualSizeAfter = target.NormalMap.Count;
					bool existing = existingValue != null;
					bool ok = ( sizeBefore == actualSizeBefore ) & ( existedBefore == existing ) & ( existing ? existingValue.Equals( valueBefore ) : valueBefore == null ) & ( existing ? previous.Equals( existingValue ) : previous == null ) & ( valueAfter != null && valueAfter.Equals( Value ) ) & existsAfter & ( sizeAfter == actualSizeAfter );
					return ok ? null : "" + Key + ":" + Value + "," + existingValue + "," + existedBefore +
							  "," + previous + "," + existsAfter;
			  }

			  public override void PrintAsCode( Maps source, LinePrinter @out, bool includeChecks )
			  {
					int? existingValue = source.NormalMap[Key];

					string addition = "map.put( " + Key + ", " + Value + " );";
					if ( includeChecks )
					{
						 bool existing = existingValue != null;
						 @out.Println( "int sizeBefore = map.size();" );
						 @out.Println( format( "boolean existedBefore = map.containsKey( %d );", Key ) );
						 @out.Println( format( "Integer valueBefore = map.get( %d );", Key ) );
						 @out.Println( format( "Integer previous = %s", addition ) );
						 @out.Println( format( "boolean existsAfter = map.containsKey( %d );", Key ) );
						 @out.Println( format( "Integer valueAfter = map.get( %d );", Key ) );
						 @out.Println( "int sizeAfter = map.size();" );

						 int actualSizeBefore = source.NormalMap.Count;
						 @out.Println( format( "assertEquals( \"%s\", %d, sizeBefore );", "Size before put should have been " + actualSizeBefore, actualSizeBefore ) );
						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( existing ), Key + " should " + ( existing ? "" : "not " ) + "exist before putting here" ) );
						 if ( existing )
						 {
							  @out.Println( format( "assertEquals( \"%s\", (Integer)%d, valueBefore );", "value before should be " + existingValue, existingValue ) );
							  @out.Println( format( "assertEquals( \"%s\", (Integer)%d, previous );", "value returned from put should be " + existingValue, existingValue ) );
						 }
						 else
						 {
							  @out.Println( format( "assertNull( \"%s\", valueBefore );", "value before putting should be null" ) );
							  @out.Println( format( "assertNull( \"%s\", previous );", "value returned from putting should be null" ) );
						 }
						 @out.Println( format( "assertTrue( \"%s\", existsAfter );", Key + " should exist" ) );
						 @out.Println( format( "assertEquals( \"%s\", (Integer)%d, valueAfter );", "value after putting should be " + Value, Value ) );
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
					int? existingValue = target.NormalMap[Key];

					bool existedBefore = target.Map.containsKey( Key );
					int? valueBefore = target.Map.get( Key );
					int? removed = target.Map.remove( Key );
					bool existsAfter = target.Map.containsKey( Key );
					int? valueAfter = target.Map.get( Key );
					target.NormalMap.Remove( Key );

					bool existing = existingValue != null;
					bool ok = ( existedBefore == existing ) & ( existing ? valueBefore.Equals( existingValue ) : valueBefore == null ) & ( existing ? removed.Equals( existingValue ) : removed == null ) & ( valueAfter == null ) & !existsAfter;
					return ok ? null : "" + Key + "," + existingValue + "," + existedBefore +
							  "," + removed + "," + existsAfter;
			  }

			  public override void PrintAsCode( Maps source, LinePrinter @out, bool includeChecks )
			  {
					int? existingValue = source.NormalMap[Key];

					string removal = "map.remove( " + Key + " );";
					if ( includeChecks )
					{
						 bool existing = existingValue != null;
						 @out.Println( format( "boolean existedBefore = map.containsKey( %d );", Key ) );
						 @out.Println( format( "Integer valueBefore = map.get( %d );", Key ) );
						 @out.Println( format( "Integer removed = %s", removal ) );
						 @out.Println( format( "boolean existsAfter = map.containsKey( %d );", Key ) );
						 @out.Println( format( "Integer valueAfter = map.get( %d );", Key ) );

						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( existing ), Key + " should " + ( existing ? "" : "not " ) + "exist before putting here" ) );
						 if ( existing )
						 {
							  @out.Println( format( "assertEquals( \"%s\", (Integer)%d, valueBefore );", "value before should be " + existingValue, existingValue ) );
							  @out.Println( format( "assertEquals( \"%s\", (Integer)%d, removed );", "value returned from put should be " + existingValue, existingValue ) );
						 }
						 else
						 {
							  @out.Println( format( "assertNull( \"%s\", valueBefore );", "value before putting should be null" ) );
							  @out.Println( format( "assertNull( \"%s\", removed );", "value returned from putting should be null" ) );
						 }
						 @out.Println( format( "assertFalse( \"%s\", existsAfter );", Key + " should not exist" ) );
						 @out.Println( format( "assertNull( \"%s\", valueAfter );", "value after removing should be null" ) );
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
			  internal readonly PrimitiveLongObjectMap<int> Map = Primitive.longObjectMap();

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