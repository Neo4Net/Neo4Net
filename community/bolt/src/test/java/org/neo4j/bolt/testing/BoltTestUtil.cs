﻿/*
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
namespace Org.Neo4j.Bolt.testing
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class BoltTestUtil
	{
		 public static BoltChannel NewTestBoltChannel()
		 {
			  return NewTestBoltChannel( "bolt-1" );
		 }

		 public static BoltChannel NewTestBoltChannel( string id )
		 {
			  return new BoltChannel( id, "bolt", new EmbeddedChannel() );
		 }

		 public static void AssertByteBufEquals( ByteBuf expected, ByteBuf actual )
		 {
			  try
			  {
					assertEquals( expected, actual );
			  }
			  finally
			  {
					ReleaseIfPossible( expected );
					ReleaseIfPossible( actual );
			  }
		 }

		 private static void ReleaseIfPossible( ByteBuf buf )
		 {
			  if ( buf.refCnt() > 0 )
			  {
					buf.release();
			  }
		 }
	}

}