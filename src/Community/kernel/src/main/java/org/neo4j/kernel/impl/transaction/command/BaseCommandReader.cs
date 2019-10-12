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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using LogPositionMarker = Neo4Net.Kernel.impl.transaction.log.LogPositionMarker;
	using PositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.PositionAwareChannel;
	using ReadableClosableChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosableChannel;
	using CommandReader = Neo4Net.Storageengine.Api.CommandReader;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;

	/// <summary>
	/// Basic functionality for <seealso cref="CommandReader"/> for <seealso cref="RecordStorageEngine"/>.
	/// </summary>
	public abstract class BaseCommandReader : CommandReader
	{
		 /// <summary>
		 /// Handles format back to 1.9 where the command format didn't have a version.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final Command read(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public override Command Read( ReadableChannel channel )
		 {
			  sbyte commandType;
			  do
			  {
					commandType = channel.Get();
			  } while ( commandType == NeoCommandType_Fields.None );

			  return Read( commandType, channel );
		 }

		 /// <summary>
		 /// Reads the next <seealso cref="Command"/> from {@code channel}.
		 /// </summary>
		 /// <param name="commandType"> type of command to read, f.ex. node command, relationship command a.s.o. </param>
		 /// <param name="channel">     <seealso cref="ReadableClosableChannel"/> to read from. </param>
		 /// <returns> <seealso cref="Command"/> or {@code null} if end reached. </returns>
		 /// <exception cref="IOException"> if channel throws exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract Command read(byte commandType, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException;
		 protected internal abstract Command Read( sbyte commandType, ReadableChannel channel );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.io.IOException unknownCommandType(byte commandType, org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 protected internal virtual IOException UnknownCommandType( sbyte commandType, ReadableChannel channel )
		 {
			  string message = "Unknown command type[" + commandType + "]";
			  if ( channel is PositionAwareChannel )
			  {
					PositionAwareChannel logChannel = ( PositionAwareChannel ) channel;
					LogPositionMarker position = new LogPositionMarker();
					logChannel.GetCurrentPosition( position );
					message += " near " + position.NewPosition();
			  }
			  return new IOException( message );
		 }
	}

}