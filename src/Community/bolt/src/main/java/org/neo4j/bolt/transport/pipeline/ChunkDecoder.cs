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
namespace Neo4Net.Bolt.transport.pipeline
{
	using LengthFieldBasedFrameDecoder = io.netty.handler.codec.LengthFieldBasedFrameDecoder;

	public class ChunkDecoder : LengthFieldBasedFrameDecoder
	{
		 private const int MAX_CHUNK_LENGTH = 0xFFFF;
		 private const int LENGTH_FIELD_OFFSET = 0;
		 private const int LENGTH_FIELD_SIZE = 2;
		 private const int LENGTH_ADJUSTMENT = 0;
		 private const int INITIAL_BYTES_TO_STRIP = LENGTH_FIELD_SIZE;

		 public ChunkDecoder() : base(MAX_CHUNK_LENGTH + LENGTH_FIELD_SIZE, LENGTH_FIELD_OFFSET, LENGTH_FIELD_SIZE, LENGTH_ADJUSTMENT, INITIAL_BYTES_TO_STRIP)
		 {
		 }
	}

}