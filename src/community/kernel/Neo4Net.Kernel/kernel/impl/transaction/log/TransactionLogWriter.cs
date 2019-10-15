﻿/*
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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;

	public class TransactionLogWriter
	{
		 private readonly LogEntryWriter _writer;

		 public TransactionLogWriter( LogEntryWriter writer )
		 {
			  this._writer = writer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void append(org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction, long transactionId) throws java.io.IOException
		 public virtual void Append( TransactionRepresentation transaction, long transactionId )
		 {
			  _writer.writeStartEntry( transaction.MasterId, transaction.AuthorId, transaction.TimeStarted, transaction.LatestCommittedTxWhenStarted, transaction.AdditionalHeader() );

			  // Write all the commands to the log channel
			  _writer.serialize( transaction );

			  // Write commit record
			  _writer.writeCommitEntry( transactionId, transaction.TimeCommitted );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkPoint(LogPosition logPosition) throws java.io.IOException
		 public virtual void CheckPoint( LogPosition logPosition )
		 {
			  _writer.writeCheckPointEntry( logPosition );
		 }
	}

}