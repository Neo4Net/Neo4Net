using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using Org.Neo4j.Helpers.Collection;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using FlushableChannel = Org.Neo4j.Kernel.impl.transaction.log.FlushableChannel;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;

	/// <summary>
	/// Serialized <seealso cref="CommittedTransactionRepresentation transactions"/> to raw bytes on the {@link ChannelBuffer
	/// network}.
	/// One serializer can be instantiated per response and is able to serialize one or many transactions.
	/// </summary>
	public class CommittedTransactionSerializer : Visitor<CommittedTransactionRepresentation, Exception>
	{
		 private readonly LogEntryWriter _writer;

		 public CommittedTransactionSerializer( FlushableChannel networkFlushableChannel )
		 {
			  this._writer = new LogEntryWriter( networkFlushableChannel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation tx) throws java.io.IOException
		 public override bool Visit( CommittedTransactionRepresentation tx )
		 {
			  _writer.serialize( tx );
			  return false;
		 }
	}

}