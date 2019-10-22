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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

	using CommandReaderFactory = Neo4Net.Storageengine.Api.CommandReaderFactory;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;

	public enum LogEntryParsersV2_3
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: TX_START { @Override public LogEntry parse(LogEntryVersion version, org.Neo4Net.kernel.impl.transaction.log.ReadableClosableChannel channel, org.Neo4Net.kernel.impl.transaction.log.LogPositionMarker marker, org.Neo4Net.storageengine.api.CommandReaderFactory commandReader) throws java.io.IOException { org.Neo4Net.kernel.impl.transaction.log.LogPosition position = marker.newPosition(); int masterId = channel.getInt(); int authorId = channel.getInt(); long timeWritten = channel.getLong(); long latestCommittedTxWhenStarted = channel.getLong(); int additionalHeaderLength = channel.getInt(); byte[] additionalHeader = new byte[additionalHeaderLength]; channel.get(additionalHeader, additionalHeaderLength); return new LogEntryStart(version, masterId, authorId, timeWritten, latestCommittedTxWhenStarted, additionalHeader, position); } @Override public byte byteCode() { return LogEntryByteCodes.TX_START; } @Override public boolean skip() { return false; } },
		 TX_START
		 {
			 public LogEntry parse( LogEntryVersion version, ReadableClosableChannel channel, LogPositionMarker marker, CommandReaderFactory commandReader ) throws IOException { LogPosition position = marker.newPosition(); int masterId = channel.Int; int authorId = channel.Int; long timeWritten = channel.Long; long latestCommittedTxWhenStarted = channel.Long; int additionalHeaderLength = channel.Int; byte[] additionalHeader = new sbyte[additionalHeaderLength]; channel.get(additionalHeader, additionalHeaderLength); return new LogEntryStart(version, masterId, authorId, timeWritten, latestCommittedTxWhenStarted, additionalHeader, position); } public sbyte byteCode() { return LogEntryByteCodes.TxStart; } public bool skip() { return false; }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: COMMAND { @Override public LogEntry parse(LogEntryVersion version, org.Neo4Net.kernel.impl.transaction.log.ReadableClosableChannel channel, org.Neo4Net.kernel.impl.transaction.log.LogPositionMarker marker, org.Neo4Net.storageengine.api.CommandReaderFactory commandReader) throws java.io.IOException { org.Neo4Net.storageengine.api.StorageCommand command = commandReader.byVersion(version.byteCode()).read(channel); return command == null ? null : new LogEntryCommand(version, command); } @Override public byte byteCode() { return LogEntryByteCodes.COMMAND; } @Override public boolean skip() { return false; } },
		 COMMAND
		 {
			 public LogEntry parse( LogEntryVersion version, ReadableClosableChannel channel, LogPositionMarker marker, CommandReaderFactory commandReader ) throws IOException { StorageCommand command = commandReader.byVersion( version.byteCode() ).read(channel); return command == null ? null : new LogEntryCommand(version, command); } public sbyte byteCode() { return LogEntryByteCodes.Command; } public bool skip() { return false; }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: TX_COMMIT { @Override public LogEntry parse(LogEntryVersion version, org.Neo4Net.kernel.impl.transaction.log.ReadableClosableChannel channel, org.Neo4Net.kernel.impl.transaction.log.LogPositionMarker marker, org.Neo4Net.storageengine.api.CommandReaderFactory commandReader) throws java.io.IOException { long txId = channel.getLong(); long timeWritten = channel.getLong(); return new LogEntryCommit(version, txId, timeWritten); } @Override public byte byteCode() { return LogEntryByteCodes.TX_COMMIT; } @Override public boolean skip() { return false; } },
		 TX_COMMIT
		 {
			 public LogEntry parse( LogEntryVersion version, ReadableClosableChannel channel, LogPositionMarker marker, CommandReaderFactory commandReader ) throws IOException { long txId = channel.Long; long timeWritten = channel.Long; return new LogEntryCommit( version, txId, timeWritten ); } public sbyte byteCode() { return LogEntryByteCodes.TxCommit; } public bool skip() { return false; }
		 },

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: CHECK_POINT { @Override public LogEntry parse(LogEntryVersion version, org.Neo4Net.kernel.impl.transaction.log.ReadableClosableChannel channel, org.Neo4Net.kernel.impl.transaction.log.LogPositionMarker marker, org.Neo4Net.storageengine.api.CommandReaderFactory commandReader) throws java.io.IOException { long logVersion = channel.getLong(); long byteOffset = channel.getLong(); return new CheckPoint(version, new org.Neo4Net.kernel.impl.transaction.log.LogPosition(logVersion, byteOffset)); } @Override public byte byteCode() { return LogEntryByteCodes.CHECK_POINT; } @Override public boolean skip() { return false; } }
		 CHECK_POINT
		 {
			 public LogEntry parse( LogEntryVersion version, ReadableClosableChannel channel, LogPositionMarker marker, CommandReaderFactory commandReader ) throws IOException { long logVersion = channel.Long; long byteOffset = channel.Long; return new CheckPoint( version, new LogPosition( logVersion, byteOffset ) ); } public sbyte byteCode() { return LogEntryByteCodes.CheckPoint; } public bool skip() { return false; }
		 }
	}

}