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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.store
{
	using Test = org.junit.Test;

	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class SecondaryUnitPrepareIdSequenceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIdImmediatelyAfterRecordId()
		 public virtual void ShouldReturnIdImmediatelyAfterRecordId()
		 {
			  // given
			  PrepareIdSequence idSequence = new SecondaryUnitPrepareIdSequence();
			  IdSequence actual = mock( typeof( IdSequence ) );

			  // when
			  long recordId = 10;
			  IdSequence prepared = idSequence.apply( actual ).apply( recordId );
			  long nextRecordId = prepared.NextId();

			  // then
			  assertEquals( 10 + 1, nextRecordId );
			  verifyNoMoreInteractions( actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnIdImmediatelyAfterRecordIdOnlyOnce()
		 public virtual void ShouldReturnIdImmediatelyAfterRecordIdOnlyOnce()
		 {
			  // given
			  PrepareIdSequence idSequence = new SecondaryUnitPrepareIdSequence();
			  IdSequence actual = mock( typeof( IdSequence ) );

			  // when
			  long recordId = 10;
			  IdSequence prepared = idSequence.apply( actual ).apply( recordId );
			  long nextRecordId = prepared.NextId();
			  assertEquals( 10 + 1, nextRecordId );
			  verifyNoMoreInteractions( actual );
			  try
			  {
					prepared.NextId();
					fail( "Should've failed" );
			  }
			  catch ( System.InvalidOperationException )
			  { // good
			  }

			  // and when
			  recordId = 20;
			  prepared = idSequence.apply( actual ).apply( recordId );
			  nextRecordId = prepared.NextId();

			  // then
			  assertEquals( 20 + 1, nextRecordId );
			  verifyNoMoreInteractions( actual );
		 }
	}

}