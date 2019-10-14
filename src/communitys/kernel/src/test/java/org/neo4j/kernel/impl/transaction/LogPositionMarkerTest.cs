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
namespace Neo4Net.Kernel.impl.transaction
{
	using Test = org.junit.Test;

	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LogPositionMarkerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUnspecifiedIfNothingHasBeenMarked()
		 public virtual void ShouldReturnUnspecifiedIfNothingHasBeenMarked()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPosition logPosition = marker.newPosition();
			  LogPosition logPosition = marker.NewPosition();

			  // given
			  assertEquals( LogPosition.UNSPECIFIED, logPosition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheMarkedPosition()
		 public virtual void ShouldReturnTheMarkedPosition()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

			  // when
			  marker.Mark( 1, 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPosition logPosition = marker.newPosition();
			  LogPosition logPosition = marker.NewPosition();

			  // given
			  assertEquals( new LogPosition( 1, 2 ), logPosition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUnspecifiedWhenAskedTo()
		 public virtual void ShouldReturnUnspecifiedWhenAskedTo()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPositionMarker marker = new org.neo4j.kernel.impl.transaction.log.LogPositionMarker();
			  LogPositionMarker marker = new LogPositionMarker();

			  // when
			  marker.Mark( 1, 2 );
			  marker.Unspecified();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogPosition logPosition = marker.newPosition();
			  LogPosition logPosition = marker.NewPosition();

			  // given
			  assertEquals( LogPosition.UNSPECIFIED, logPosition );
		 }
	}

}