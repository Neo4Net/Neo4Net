using System;

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
namespace Neo4Net.Kernel.impl.transaction.log
{
	public class NoSuchTransactionException : MissingLogDataException
	{
		 public NoSuchTransactionException( long missingTxId ) : this( missingTxId, null )
		 {
		 }

		 public NoSuchTransactionException( long missingTxId, string additionalInformation ) : base( CombinedMessage( missingTxId, additionalInformation ) )
		 {
		 }

		 public NoSuchTransactionException( long missingTxId, string additionalInformation, Exception cause ) : base( CombinedMessage( missingTxId, additionalInformation ), cause )
		 {
		 }

		 private static string CombinedMessage( long missingTxId, string additionalInformation )
		 {
			  string result = "Unable to find transaction " + missingTxId + " in any of my logical logs";
			  if ( !string.ReferenceEquals( additionalInformation, null ) )
			  {
					result += ": " + additionalInformation;
			  }
			  return result;
		 }
	}

}