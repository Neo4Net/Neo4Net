﻿using System.Collections.Generic;

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
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_COMMIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryVersion.CURRENT;

	public class LogEntryWriter
	{
		 protected internal readonly WritableChannel Channel;
		 private readonly Visitor<StorageCommand, IOException> _serializer;

		 /// <summary>
		 /// Create a writer that uses <seealso cref="LogEntryVersion.CURRENT"/> for versioning. </summary>
		 /// <param name="channel"> underlying channel </param>
		 public LogEntryWriter( WritableChannel channel )
		 {
			  this.Channel = channel;
			  this._serializer = new StorageCommandSerializer( channel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static void writeLogEntryHeader(byte type, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 protected internal static void WriteLogEntryHeader( sbyte type, WritableChannel channel )
		 {
			  channel.Put( CURRENT.byteCode() ).put(type);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeStartEntry(LogEntryStart entry) throws java.io.IOException
		 public virtual void WriteStartEntry( LogEntryStart entry )
		 {
			  WriteStartEntry( entry.MasterId, entry.LocalId, entry.TimeWritten, entry.LastCommittedTxWhenTransactionStarted, entry.AdditionalHeader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeStartEntry(int masterId, int authorId, long timeWritten, long latestCommittedTxWhenStarted, byte[] additionalHeaderData) throws java.io.IOException
		 public virtual void WriteStartEntry( int masterId, int authorId, long timeWritten, long latestCommittedTxWhenStarted, sbyte[] additionalHeaderData )
		 {
			  WriteLogEntryHeader( TX_START, Channel );
			  Channel.putInt( masterId ).putInt( authorId ).putLong( timeWritten ).putLong( latestCommittedTxWhenStarted ).putInt( additionalHeaderData.Length ).put( additionalHeaderData, additionalHeaderData.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeCommitEntry(LogEntryCommit entry) throws java.io.IOException
		 public virtual void WriteCommitEntry( LogEntryCommit entry )
		 {
			  WriteCommitEntry( entry.TxId, entry.TimeWritten );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeCommitEntry(long transactionId, long timeWritten) throws java.io.IOException
		 public virtual void WriteCommitEntry( long transactionId, long timeWritten )
		 {
			  WriteLogEntryHeader( TX_COMMIT, Channel );
			  Channel.putLong( transactionId ).putLong( timeWritten );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.kernel.impl.transaction.TransactionRepresentation tx) throws java.io.IOException
		 public virtual void Serialize( TransactionRepresentation tx )
		 {
			  tx.Accept( _serializer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws java.io.IOException
		 public virtual void Serialize( CommittedTransactionRepresentation tx )
		 {
			  WriteStartEntry( tx.StartEntry );
			  Serialize( tx.TransactionRepresentation );
			  WriteCommitEntry( tx.CommitEntry );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void serialize(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> commands) throws java.io.IOException
		 public virtual void Serialize( ICollection<StorageCommand> commands )
		 {
			  foreach ( StorageCommand command in commands )
			  {
					_serializer.visit( command );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeCheckPointEntry(org.neo4j.kernel.impl.transaction.log.LogPosition logPosition) throws java.io.IOException
		 public virtual void WriteCheckPointEntry( LogPosition logPosition )
		 {
			  WriteLogEntryHeader( CHECK_POINT, Channel );
			  Channel.putLong( logPosition.LogVersion ).putLong( logPosition.ByteOffset );
		 }
	}

}