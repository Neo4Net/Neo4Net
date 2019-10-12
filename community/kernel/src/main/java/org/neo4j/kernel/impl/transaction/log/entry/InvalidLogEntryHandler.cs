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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{

	/// <summary>
	/// Decides what happens to invalid log entries read by <seealso cref="LogEntryReader"/>.
	/// </summary>
	public abstract class InvalidLogEntryHandler
	{
		 /// <summary>
		 /// Allows no invalid log entries.
		 /// </summary>
		 public static readonly InvalidLogEntryHandler STRICT = new InvalidLogEntryHandlerAnonymousInnerClass();

		 private class InvalidLogEntryHandlerAnonymousInnerClass : InvalidLogEntryHandler
		 {
		 }

		 /// <summary>
		 /// Log entry couldn't be read correctly. Could be invalid log entry in the log.
		 /// </summary>
		 /// <param name="e"> error during reading a log entry. </param>
		 /// <param name="position"> <seealso cref="LogPosition"/> of the start of the log entry attempted to be read. </param>
		 /// <returns> {@code true} if this error is accepted, otherwise {@code false} which means the exception
		 /// causing this will be thrown by the caller. </returns>
		 public virtual bool HandleInvalidEntry( Exception e, LogPosition position )
		 { // consider invalid by default
			  return false;
		 }

		 /// <summary>
		 /// Tells this handler that, given that there were invalid entries, handler thinks they are OK
		 /// to skip and that one or more entries after a bad section could be read then a certain number
		 /// of bytes contained invalid log data and were therefore skipped. Log entry reading continues
		 /// after this call.
		 /// </summary>
		 /// <param name="bytesSkipped"> number of bytes skipped. </param>
		 public virtual void BytesSkipped( long bytesSkipped )
		 { // do nothing by default
		 }
	}

}