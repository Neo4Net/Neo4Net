/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class RelationshipReferenceEncodingTest
	{
		 /// <summary>
		 /// The current scheme only allows us to use 58 bits for a reference. Adhere to that limit here.
		 /// </summary>
		 private static readonly long _mask = NumberOfBits( Reference.MAX_BITS );
		 private const int PAGE_SIZE = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
		 private readonly StubPageCursor _cursor = new StubPageCursor( 0, PAGE_SIZE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeRandomLongs()
		 public virtual void ShouldEncodeRandomLongs()
		 {
			  for ( int i = 0; i < 100_000_000; i++ )
			  {
					long reference = Limit( Random.nextLong() );
					AssertDecodedMatchesEncoded( reference );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relativeReferenceConversion()
		 public virtual void RelativeReferenceConversion()
		 {
			  long basis = 0xBABE;
			  long absoluteReference = 0xCAFEBABE;

			  long relative = Reference.toRelative( absoluteReference, basis );
			  assertEquals( "Should be equal to difference of reference and base reference", 0xCAFE0000, relative );

			  long absoluteCandidate = Reference.toAbsolute( relative, basis );
			  assertEquals( "Converted reference should be equal to initial value", absoluteReference, absoluteCandidate );
		 }

		 private static long NumberOfBits( int count )
		 {
			  long result = 0;
			  for ( int i = 0; i < count; i++ )
			  {
					result = ( result << 1 ) | 1;
			  }
			  return result;
		 }

		 private static long Limit( long reference )
		 {
			  bool positive = true;
			  if ( reference < 0 )
			  {
					positive = false;
					reference = ~reference;
			  }

			  reference &= _mask;

			  if ( !positive )
			  {
					reference = ~reference;
			  }
			  return reference;
		 }

		 private void AssertDecodedMatchesEncoded( long reference )
		 {
			  _cursor.Offset = 0;
			  Reference.encode( reference, _cursor );

			  _cursor.Offset = 0;
			  long read = Reference.decode( _cursor );
			  assertEquals( reference, read );
		 }
	}

}