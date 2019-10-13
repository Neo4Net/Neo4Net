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
namespace Neo4Net.Utils.Concurrent
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class DecayingFlagsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrackToggle()
		 internal virtual void ShouldTrackToggle()
		 {
			  // Given
			  DecayingFlags.Key myFeature = new DecayingFlags.Key( 1 );
			  DecayingFlags set = new DecayingFlags( 1 );

			  // When
			  set.Flag( myFeature );

			  // Then
			  assertEquals( "4000", set.AsHex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrackMultipleFlags()
		 internal virtual void ShouldTrackMultipleFlags()
		 {
			  // Given
			  DecayingFlags.Key featureA = new DecayingFlags.Key( 1 );
			  DecayingFlags.Key featureB = new DecayingFlags.Key( 3 );
			  DecayingFlags set = new DecayingFlags( 2 );

			  // When
			  set.Flag( featureA );
			  set.Flag( featureA );
			  set.Flag( featureB );

			  // Then
			  assertEquals( "5000", set.AsHex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toggleShouldDecay()
		 internal virtual void ToggleShouldDecay()
		 {
			  // Given
			  DecayingFlags.Key featureA = new DecayingFlags.Key( 1 );
			  DecayingFlags.Key featureB = new DecayingFlags.Key( 3 );
			  DecayingFlags set = new DecayingFlags( 2 );

			  // And given Feature A has been used quite a bit, while
			  // feature B is not quite as popular..
			  set.Flag( featureA );
			  set.Flag( featureA );
			  set.Flag( featureB );

			  // When
			  set.Sweep();

			  // Then
			  assertEquals( "4000", set.AsHex() );

			  // When
			  set.Sweep();

			  // Then
			  assertEquals( "0000", set.AsHex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void resetFlagShouldRecoverIfToggledAgain()
		 internal virtual void ResetFlagShouldRecoverIfToggledAgain()
		 {
			  // Given
			  DecayingFlags.Key featureA = new DecayingFlags.Key( 9 );
			  DecayingFlags set = new DecayingFlags( 2 );

			  set.Flag( featureA );

			  // When
			  set.Sweep();

			  // Then
			  assertEquals( "0000", set.AsHex() );

			  // When
			  set.Flag( featureA );

			  // Then
			  assertEquals( "0040", set.AsHex() );
		 }
	}

}