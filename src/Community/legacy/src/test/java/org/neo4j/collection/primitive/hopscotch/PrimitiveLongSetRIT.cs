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
namespace Neo4Net.Collection.primitive.hopscotch
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
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class PrimitiveLongSetRIT
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
					RandomizedTester<Sets, string> actions = new RandomizedTester<Sets, string>( SetFactory(), ActionFactory(random) );

					Result<Sets, string> result = actions.Run( max );
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

		 private static void FullVerification( Sets target, Random random )
		 {
			  foreach ( long? value in target.NormalSet )
			  {
					assertTrue( target.Set.contains( value.Value ) );
			  }

			  for ( int i = 0; i < target.NormalSet.Count; i++ )
			  {
					assertFalse( target.Set.contains( RandomNonExisting( random, target.NormalSet ) ) );
			  }
		 }

		 private static Printable Given()
		 {
			  return @out => @out.println( typeof( PrimitiveLongSet ).Name + " set = " + typeof( Primitive ).Name + ".longSet();" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.test.randomized.RandomizedTester.ActionFactory<Sets, String> actionFactory(final java.util.Random random)
		 private static RandomizedTester.ActionFactory<Sets, string> ActionFactory( Random random )
		 {
			  return from => GenerateAction( random, from );
		 }

		 private static RandomizedTester.TargetFactory<Sets> SetFactory()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Sets::new;
		 }

		 private static Action<Sets, string> GenerateAction( Random random, Sets from )
		 {
			  bool anExisting = from.NormalSet.Count > 0 && random.Next( 3 ) == 0;
			  long value = anExisting ? RandomExisting( random, from.NormalSet ) : RandomNonExisting( random, from.NormalSet );

			  int typeOfAction = random.Next( 5 );
			  if ( typeOfAction == 0 )
			  { // remove
					return new RemoveAction( value );
			  }

			  // add
			  return new AddAction( value );
		 }

		 private static long RandomNonExisting( Random random, ISet<long> existing )
		 {
			  while ( true )
			  {
					long value = Math.Abs( random.nextLong() );
					if ( !existing.Contains( value ) )
					{
						 return value;
					}
			  }
		 }

		 private static long RandomExisting( Random random, ISet<long> existing )
		 {
			  int index = random.Next( existing.Count ) + 1;
			  IEnumerator<long> iterator = existing.GetEnumerator();
			  long value = 0;
			  for ( int i = 0; i < index; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					value = iterator.next();
			  }
			  return value;
		 }

		 private class AddAction : Action<Sets, string>
		 {
			  internal readonly long Value;

			  internal AddAction( long value )
			  {
					this.Value = value;
			  }

			  public override string Apply( Sets target )
			  {
					try
					{
						 bool alreadyExisting = target.NormalSet.Contains( Value );

						 PrimitiveLongSet set = target.Set;
						 bool existedBefore = set.Contains( Value );
						 bool added = set.Add( Value );
						 bool existsAfter = set.Contains( Value );
						 target.NormalSet.Add( Value );

						 bool ok = ( existedBefore == alreadyExisting ) & ( added == !alreadyExisting ) & existsAfter;
						 return ok ? null : "" + Value + alreadyExisting + "," + existedBefore + "," + added + "," + existsAfter;
					}
					catch ( Exception e )
					{
						 return "exception:" + e.Message;
					}
			  }

			  public override void PrintAsCode( Sets source, LinePrinter @out, bool includeChecks )
			  {
					bool alreadyExisting = source.NormalSet.Contains( Value );
					string addition = "set.add( " + Value + "L );";
					if ( includeChecks )
					{
						 @out.Println( format( "boolean existedBefore = set.contains( %dL );", Value ) );
						 @out.Println( format( "boolean added = %s", addition ) );
						 @out.Println( format( "boolean existsAfter = set.contains( %dL );", Value ) );
						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( alreadyExisting ), Value + " should " + ( alreadyExisting ? "" : "not " ) + "exist before adding here" ) );
						 @out.Println( format( "assert%s( \"%s\", added );", Capitilize( !alreadyExisting ), Value + " should " + ( !alreadyExisting ? "" : "not " ) + "be reported as added here" ) );
						 @out.Println( format( "assertTrue( \"%s\", existsAfter );", Value + " should exist" ) );
					}
					else
					{
						 @out.Println( addition );
					}
			  }
		 }

		 private class RemoveAction : Action<Sets, string>
		 {
			  internal readonly long Value;

			  internal RemoveAction( long value )
			  {
					this.Value = value;
			  }

			  public override string Apply( Sets target )
			  {
					try
					{
						 bool alreadyExisting = target.NormalSet.Contains( Value );
						 PrimitiveLongSet set = target.Set;
						 bool existedBefore = set.Contains( Value );
						 bool removed = set.Remove( Value );
						 bool existsAfter = set.Contains( Value );
						 target.NormalSet.remove( Value );

						 bool ok = ( existedBefore == alreadyExisting ) & ( removed == alreadyExisting ) & !existsAfter;
						 return ok ? null : "" + Value + alreadyExisting + "," + existedBefore + "," + removed + "," + existsAfter;
					}
					catch ( Exception e )
					{
						 return "exception: " + e.Message;
					}
			  }

			  public override void PrintAsCode( Sets source, LinePrinter @out, bool includeChecks )
			  {
					bool alreadyExisting = source.NormalSet.Contains( Value );
					string removal = "set.remove( " + Value + "L );";
					if ( includeChecks )
					{
						 @out.Println( format( "boolean existedBefore = set.contains( %dL );", Value ) );
						 @out.Println( format( "boolean removed = %s", removal ) );
						 @out.Println( format( "boolean existsAfter = set.contains( %dL );", Value ) );
						 @out.Println( format( "assert%s( \"%s\", existedBefore );", Capitilize( alreadyExisting ), Value + " should " + ( alreadyExisting ? "" : "not " ) + "exist before removing here" ) );
						 @out.Println( format( "assert%s( \"%s\", removed );", Capitilize( alreadyExisting ), Value + " should " + ( alreadyExisting ? "" : "not " ) + "be reported as removed here" ) );
						 @out.Println( format( "assertFalse( \"%s\", existsAfter );", Value + " should not exist" ) );
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

		 private class Sets : TestResource
		 {
			  internal readonly ISet<long> NormalSet = new HashSet<long>();
			  internal readonly PrimitiveLongSet Set = Primitive.longSet();

			  public override void Close()
			  {
					Set.close();
			  }
		 }
	}

}