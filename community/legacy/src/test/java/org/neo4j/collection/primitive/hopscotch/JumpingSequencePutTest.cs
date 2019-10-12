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


	internal class JumpingSequencePutTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandlePathologicalSequenceCase()
		 internal virtual void ShouldHandlePathologicalSequenceCase()
		 {
			  // Given
			  PrimitiveLongSet set = Primitive.longSet();
			  Sequence seqGen = new Sequence( this );

			  // When
			  for ( int i = 0; i < 10000; i++ )
			  {
					set.Add( seqGen.Next() );
			  }
		 }

		 /// <summary>
		 /// To be frank, I don't understand the intricacies of how this works, but
		 /// this is a cut-out version of the sequence generator that triggered the original bug.
		 /// The gist is that it generates sequences of ids that "jump" to a much higher number
		 /// every one hundred ids or so.
		 /// </summary>
		 private class Sequence
		 {
			 private readonly JumpingSequencePutTest _outerInstance;

			 public Sequence( JumpingSequencePutTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal const int SIZE_PER_JUMP = 100;
			  internal readonly AtomicLong NextId = new AtomicLong();
			  internal int LeftToNextJump = SIZE_PER_JUMP / 2;
			  internal long HighBits;

			  public virtual long Next()
			  {
					long result = TryNextId();
					if ( --LeftToNextJump == 0 )
					{
						 LeftToNextJump = SIZE_PER_JUMP;
						 NextId.set( ( 0xFFFFFFFFL | ( HighBits++ << 32 ) ) - SIZE_PER_JUMP / 2 + 1 );
					}
					return result;
			  }

			  internal virtual long TryNextId()
			  {
					long result = NextId.AndIncrement;
					if ( result == 0xFFFFFFFFL ) // 4294967295L
					{
						 result = NextId.AndIncrement;
						 LeftToNextJump--;
					}
					return result;
			  }
		 }
	}

}