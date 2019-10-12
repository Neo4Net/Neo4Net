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
namespace Org.Neo4j.Helpers
{
	using Test = org.junit.jupiter.api.Test;

	using Predicates = Org.Neo4j.Function.Predicates;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class TestExceptions
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canPeelExceptions()
		 internal virtual void CanPeelExceptions()
		 {
			  // given
			  Exception expected;
			  Exception exception = new LevelOneException( "", new LevelTwoException( "", new LevelThreeException( "", expected = new LevelThreeException( "include", new LevelFourException( "" ) ) ) ) );

			  // when
			  Exception peeled = Exceptions.Peel( exception, item => !( item is LevelThreeException ) || !item.Message.contains( "include" ) );

			  // then
			  assertEquals( expected, peeled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canPeelUsingConveniencePredicate()
		 internal virtual void CanPeelUsingConveniencePredicate()
		 {
			  // given
			  Exception expected;
			  Exception exception = new ARuntimeException( new AnotherRuntimeException( new LevelFourException( "", expected = new LevelThreeException( "", new LevelFourException( "" ) ) ) ) );

			  // when
			  Exception peeled = Exceptions.Peel( exception, Predicates.instanceOfAny( typeof( Exception ), typeof( LevelFourException ) ) );

			  // then
			  assertEquals( expected, peeled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDetectContainsOneOfSome()
		 internal virtual void ShouldDetectContainsOneOfSome()
		 {
			  // GIVEN
			  Exception cause = new ARuntimeException( new AnotherRuntimeException( new System.NullReferenceException( "Some words" ) ) );

			  // THEN
			  assertTrue( Exceptions.Contains( cause, typeof( System.NullReferenceException ) ) );
			  assertTrue( Exceptions.Contains( cause, "words", typeof( System.NullReferenceException ) ) );
			  assertFalse( Exceptions.Contains( cause, "not", typeof( System.NullReferenceException ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetMessage()
		 internal virtual void ShouldSetMessage()
		 {
			  // GIVEN
			  string initialMessage = "Initial message";
			  LevelOneException exception = new LevelOneException( initialMessage );

			  // WHEN
			  string prependedMessage = "Prepend this: " + exception.Message;
			  Exceptions.WithMessage( exception, prependedMessage );

			  // THEN
			  assertEquals( prependedMessage, exception.Message );
		 }

		 private class LevelOneException : Exception
		 {
			  internal LevelOneException( string message ) : base( message )
			  {
			  }

			  internal LevelOneException( string message, Exception cause ) : base( message, cause )
			  {
			  }
		 }

		 private class LevelTwoException : LevelOneException
		 {
			  internal LevelTwoException( string message ) : base( message )
			  {
			  }

			  internal LevelTwoException( string message, Exception cause ) : base( message, cause )
			  {
			  }
		 }

		 private class LevelThreeException : LevelTwoException
		 {
			  internal LevelThreeException( string message ) : base( message )
			  {
			  }

			  internal LevelThreeException( string message, Exception cause ) : base( message, cause )
			  {
			  }
		 }

		 private class LevelFourException : LevelThreeException
		 {
			  internal LevelFourException( string message ) : base( message )
			  {
			  }

			  internal LevelFourException( string message, Exception cause ) : base( message, cause )
			  {
			  }
		 }

		 private class ARuntimeException : Exception
		 {
			  internal ARuntimeException( Exception cause ) : base( cause )
			  {
			  }
		 }

		 private class AnotherRuntimeException : Exception
		 {
			  internal AnotherRuntimeException( Exception cause ) : base( cause )
			  {
			  }
		 }
	}

}