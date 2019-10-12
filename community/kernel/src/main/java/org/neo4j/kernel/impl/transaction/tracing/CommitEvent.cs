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
namespace Org.Neo4j.Kernel.impl.transaction.tracing
{
	/// <summary>
	/// A trace event that represents the commit process of a transaction.
	/// </summary>
	public interface CommitEvent : AutoCloseable
	{
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CommitEvent NULL = new CommitEvent()
	//	 {
	//		  @@Override public void close()
	//		  {
	//		  }
	//
	//		  @@Override public LogAppendEvent beginLogAppend()
	//		  {
	//				return LogAppendEvent.NULL;
	//		  }
	//
	//		  @@Override public StoreApplyEvent beginStoreApply()
	//		  {
	//				return StoreApplyEvent.NULL;
	//		  }
	//	 };

		 /// <summary>
		 /// Mark the end of the commit process.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Begin appending commands for the committing transaction, to the transaction log.
		 /// </summary>
		 LogAppendEvent BeginLogAppend();

		 /// <summary>
		 /// Begin applying the commands of the committed transaction to the stores.
		 /// </summary>
		 StoreApplyEvent BeginStoreApply();
	}

}