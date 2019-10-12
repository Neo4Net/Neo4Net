using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.log.reverse
{


	/// <summary>
	/// Eagerly exhausts a <seealso cref="TransactionCursor"/> and allows moving through it in reverse order.
	/// The idea is that this should only be done for a subset of a bigger transaction log stream, typically
	/// for one log file.
	/// 
	/// For reversing a transaction log consisting of multiple log files <seealso cref="ReversedMultiFileTransactionCursor"/>
	/// should be used (it will use this class internally though).
	/// </summary>
	/// <seealso cref= ReversedMultiFileTransactionCursor </seealso>
	public class EagerlyReversedTransactionCursor : TransactionCursor
	{
		 private readonly IList<CommittedTransactionRepresentation> _txs = new List<CommittedTransactionRepresentation>();
		 private readonly TransactionCursor _cursor;
		 private int _indexToReturn;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public EagerlyReversedTransactionCursor(org.neo4j.kernel.impl.transaction.log.TransactionCursor cursor) throws java.io.IOException
		 public EagerlyReversedTransactionCursor( TransactionCursor cursor )
		 {
			  this._cursor = cursor;
			  while ( cursor.next() )
			  {
					_txs.Add( cursor.get() );
			  }
			  this._indexToReturn = _txs.Count;
		 }

		 public override bool Next()
		 {
			  if ( _indexToReturn > 0 )
			  {
					_indexToReturn--;
					return true;
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _cursor.close();
		 }

		 public override CommittedTransactionRepresentation Get()
		 {
			  return _txs[_indexToReturn];
		 }

		 public override LogPosition Position()
		 {
			  throw new System.NotSupportedException( "Should not be called" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.log.TransactionCursor eagerlyReverse(org.neo4j.kernel.impl.transaction.log.TransactionCursor cursor) throws java.io.IOException
		 public static TransactionCursor EagerlyReverse( TransactionCursor cursor )
		 {
			  return new EagerlyReversedTransactionCursor( cursor );
		 }
	}

}