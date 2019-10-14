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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.array;

	public class ThrowingConflictDetectorTest
	{
		 private readonly ThrowingConflictDetector<NumberIndexKey, NativeIndexValue> _detector = new ThrowingConflictDetector<NumberIndexKey, NativeIndexValue>( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportConflictOnSameValueAndDifferentEntityIds()
		 public virtual void ShouldReportConflictOnSameValueAndDifferentEntityIds()
		 {
			  // given
			  Value value = Values.of( 123 );
			  long entityId1 = 10;
			  long entityId2 = 20;

			  // when
			  NativeIndexValue merged = _detector.merge( Key( entityId1, value ), Key( entityId2, value ), NativeIndexValue.Instance, NativeIndexValue.Instance );

			  // then
			  assertNull( merged );
			  try
			  {
					_detector.checkConflict( array( value ) );
					fail( "Should've detected conflict" );
			  }
			  catch ( IndexEntryConflictException e )
			  {
					assertEquals( entityId1, e.ExistingNodeId );
					assertEquals( entityId2, e.AddedNodeId );
					assertEquals( value, e.SinglePropertyValue );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportConflictOnSameValueSameEntityId() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportConflictOnSameValueSameEntityId()
		 {
			  // given
			  Value value = Values.of( 123 );
			  long entityId = 10;

			  // when
			  NativeIndexValue merged = _detector.merge( Key( entityId, value ), Key( entityId, value ), NativeIndexValue.Instance, NativeIndexValue.Instance );

			  // then
			  assertNull( merged );
			  _detector.checkConflict( array() ); // <-- should not throw conflict exception
		 }

		 private static NumberIndexKey Key( long entityId, Value value )
		 {
			  NumberIndexKey key = new NumberIndexKey();
			  key.initialize( entityId );
			  key.initFromValue( 0, value, NativeIndexKey.Inclusion.Low );
			  return key;
		 }
	}

}