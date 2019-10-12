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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{

	using Org.Neo4j.Helpers.Collection;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.COMMAND;

	public class StorageCommandSerializer : Visitor<StorageCommand, IOException>
	{
		 private readonly WritableChannel _channel;

		 public StorageCommandSerializer( WritableChannel channel )
		 {
			  this._channel = channel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.storageengine.api.StorageCommand command) throws java.io.IOException
		 public override bool Visit( StorageCommand command )
		 {
			  LogEntryWriter.WriteLogEntryHeader( COMMAND, _channel );
			  command.Serialize( _channel );
			  return false;
		 }
	}

}