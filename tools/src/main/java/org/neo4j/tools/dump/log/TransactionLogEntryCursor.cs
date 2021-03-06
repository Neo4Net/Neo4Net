﻿using System.Collections.Generic;

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
namespace Org.Neo4j.tools.dump.log
{

	using Org.Neo4j.Cursor;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_COMMIT;

	/// <summary>
	/// Groups <seealso cref="LogEntry"/> instances transaction by transaction
	/// </summary>
	public class TransactionLogEntryCursor : IOCursor<LogEntry[]>
	{
		 private readonly IOCursor<LogEntry> @delegate;
		 private readonly IList<LogEntry> _transaction = new List<LogEntry>();

		 public TransactionLogEntryCursor( IOCursor<LogEntry> @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override LogEntry[] Get()
		 {
			  return _transaction.ToArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  _transaction.Clear();
			  LogEntry entry;
			  while ( @delegate.next() )
			  {
					entry = @delegate.get();
					_transaction.Add( entry );
					if ( IsBreakPoint( entry ) )
					{
						 return true;
					}
			  }
			  return _transaction.Count > 0;
		 }

		 private static bool IsBreakPoint( LogEntry entry )
		 {
			  sbyte type = entry.Type;
			  return type == TX_COMMIT || type == CHECK_POINT;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.close();
		 }
	}

}