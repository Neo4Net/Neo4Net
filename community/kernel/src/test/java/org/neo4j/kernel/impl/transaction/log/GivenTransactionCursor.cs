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
namespace Org.Neo4j.Kernel.impl.transaction.log
{

	public class GivenTransactionCursor : TransactionCursor
	{
		 private int _index = -1;
		 private readonly CommittedTransactionRepresentation[] _transactions;

		 private GivenTransactionCursor( params CommittedTransactionRepresentation[] transactions )
		 {
			  this._transactions = transactions;
		 }

		 public override CommittedTransactionRepresentation Get()
		 {
			  return _transactions[_index];
		 }

		 public override bool Next()
		 {
			  if ( _index + 1 < _transactions.Length )
			  {
					_index++;
					return true;
			  }
			  return false;
		 }

		 public override void Close()
		 {
		 }

		 public override LogPosition Position()
		 {
			  return null;
		 }

		 public static TransactionCursor Given( params CommittedTransactionRepresentation[] transactions )
		 {
			  return new GivenTransactionCursor( transactions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation[] exhaust(TransactionCursor cursor) throws java.io.IOException
		 public static CommittedTransactionRepresentation[] Exhaust( TransactionCursor cursor )
		 {
			  IList<CommittedTransactionRepresentation> list = new List<CommittedTransactionRepresentation>();
			  while ( cursor.next() )
			  {
					list.Add( cursor.get() );
			  }
			  return list.ToArray();
		 }
	}

}