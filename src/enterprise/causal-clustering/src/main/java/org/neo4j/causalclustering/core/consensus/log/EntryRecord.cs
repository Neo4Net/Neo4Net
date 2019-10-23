﻿/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log
{

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadPastEndException = Neo4Net.Kernel.Api.StorageEngine.ReadPastEndException;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// A log entry and its log index.
	/// </summary>
	public class EntryRecord
	{
		 private readonly long _logIndex;
		 private readonly RaftLogEntry _logEntry;

		 public EntryRecord( long logIndex, RaftLogEntry logEntry )
		 {
			  this._logIndex = logIndex;
			  this._logEntry = logEntry;
		 }

		 public virtual RaftLogEntry LogEntry()
		 {
			  return _logEntry;
		 }

		 public virtual long LogIndex()
		 {
			  return _logIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static EntryRecord read(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel, org.Neo4Net.causalclustering.messaging.marshalling.ChannelMarshal<org.Neo4Net.causalclustering.core.replication.ReplicatedContent> contentMarshal) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
		 public static EntryRecord Read( ReadableChannel channel, ChannelMarshal<ReplicatedContent> contentMarshal )
		 {
			  try
			  {
					long appendIndex = channel.Long;
					long term = channel.Long;
					ReplicatedContent content = contentMarshal.Unmarshal( channel );
					return new EntryRecord( appendIndex, new RaftLogEntry( term, content ) );
			  }
			  catch ( ReadPastEndException e )
			  {
					throw new EndOfStreamException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void write(org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel, org.Neo4Net.causalclustering.messaging.marshalling.ChannelMarshal<org.Neo4Net.causalclustering.core.replication.ReplicatedContent> contentMarshal, long logIndex, long term, org.Neo4Net.causalclustering.core.replication.ReplicatedContent content) throws java.io.IOException
		 public static void Write( WritableChannel channel, ChannelMarshal<ReplicatedContent> contentMarshal, long logIndex, long term, ReplicatedContent content )
		 {
			  channel.PutLong( logIndex );
			  channel.PutLong( term );
			  contentMarshal.Marshal( content, channel );
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0:D}: {1}", _logIndex, _logEntry );
		 }
	}

}