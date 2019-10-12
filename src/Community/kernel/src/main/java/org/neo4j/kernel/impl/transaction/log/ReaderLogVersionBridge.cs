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

	using IncompleteLogHeaderException = Neo4Net.Kernel.impl.transaction.log.entry.IncompleteLogHeaderException;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;

	/// <summary>
	/// <seealso cref="LogVersionBridge"/> naturally transitioning from one <seealso cref="LogVersionedStoreChannel"/> to the next,
	/// i.e. to log version with one higher version than the current.
	/// </summary>
	public class ReaderLogVersionBridge : LogVersionBridge
	{
		 private readonly LogFiles _logFiles;

		 public ReaderLogVersionBridge( LogFiles logFiles )
		 {
			  this._logFiles = logFiles;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogVersionedStoreChannel next(LogVersionedStoreChannel channel) throws java.io.IOException
		 public override LogVersionedStoreChannel Next( LogVersionedStoreChannel channel )
		 {
			  PhysicalLogVersionedStoreChannel nextChannel;
			  try
			  {
					nextChannel = _logFiles.openForVersion( channel.Version + 1 );
			  }
			  catch ( Exception e ) when ( e is FileNotFoundException || e is IncompleteLogHeaderException )
			  {
					// See PhysicalLogFile#rotate() for description as to why these exceptions are OK
					return channel;
			  }
			  channel.close();
			  return nextChannel;
		 }
	}

}