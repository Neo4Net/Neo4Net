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
namespace Org.Neo4j.Util
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;

	internal class PreconditionsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requirePositiveOk()
		 internal virtual void RequirePositiveOk()
		 {
			  requirePositive( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requirePositiveFailsOnZero()
		 internal virtual void RequirePositiveFailsOnZero()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequirePositive(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requirePositiveFailsOnNegative()
		 internal virtual void RequirePositiveFailsOnNegative()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => requirePositive(-1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requireNonNegativeOk()
		 internal virtual void RequireNonNegativeOk()
		 {
			  Preconditions.RequireNonNegative( 0 );
			  Preconditions.RequireNonNegative( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requireNonNegativeFailsOnNegative()
		 internal virtual void RequireNonNegativeFailsOnNegative()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequireNonNegative(-1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkStateOk()
		 internal virtual void CheckStateOk()
		 {
			  Preconditions.CheckState( true, "must not fail" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkStateFails()
		 internal virtual void CheckStateFails()
		 {
			  assertThrows( typeof( System.InvalidOperationException ), () => Preconditions.checkState(false, "must fail") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requirePowerOfTwo()
		 internal virtual void RequirePowerOfTwo()
		 {
			  assertEquals( 1, Preconditions.RequirePowerOfTwo( 1 ) );
			  assertEquals( 2, Preconditions.RequirePowerOfTwo( 2 ) );
			  assertEquals( 128, Preconditions.RequirePowerOfTwo( 128 ) );
			  assertEquals( 0b01000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L, Preconditions.RequirePowerOfTwo( 0b01000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L ) );

			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequirePowerOfTwo(-1), "negative" );
			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequirePowerOfTwo(0), "zero" );
			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequirePowerOfTwo(3), "three" );
			  assertThrows( typeof( System.ArgumentException ), () => Preconditions.RequirePowerOfTwo(0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L), "sign bit" );
		 }
	}

}