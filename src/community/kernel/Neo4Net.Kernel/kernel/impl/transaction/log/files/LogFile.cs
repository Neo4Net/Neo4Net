/*
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
namespace Neo4Net.Kernel.impl.transaction.log.files
{


	/// <summary>
	/// Sees a log file as bytes, including taking care of rotation of the file into optimal chunks.
	/// </summary>
	public interface LogFile
	{

		 /// <returns> <seealso cref="FlushableChannel"/> capable of appending data to this log. </returns>
		 FlushablePositionAwareChannel Writer { get; }

		 /// <summary>
		 /// Opens a <seealso cref="ReadableLogChannel reader"/> at the desired <seealso cref="LogPosition"/>, capable of reading log entries
		 /// from that position and onwards, through physical log versions.
		 /// </summary>
		 /// <param name="position"> <seealso cref="LogPosition"/> to position the returned reader at. </param>
		 /// <returns> <seealso cref="ReadableClosableChannel"/> capable of reading log data, starting from <seealso cref="LogPosition position"/>. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.log.ReadableLogChannel getReader(org.Neo4Net.kernel.impl.transaction.log.LogPosition position) throws java.io.IOException;
		 ReadableLogChannel GetReader( LogPosition position );

		 /// <summary>
		 /// Opens a <seealso cref="ReadableLogChannel reader"/> at the desired <seealso cref="LogPosition"/>, capable of reading log entries
		 /// from that position and onwards, with the given <seealso cref="LogVersionBridge"/>.
		 /// </summary>
		 /// <param name="position"> <seealso cref="LogPosition"/> to position the returned reader at. </param>
		 /// <param name="logVersionBridge"> <seealso cref="LogVersionBridge"/> how to bridge log versions. </param>
		 /// <returns> <seealso cref="ReadableClosableChannel"/> capable of reading log data, starting from <seealso cref="LogPosition position"/>. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.log.ReadableLogChannel getReader(org.Neo4Net.kernel.impl.transaction.log.LogPosition position, org.Neo4Net.kernel.impl.transaction.log.LogVersionBridge logVersionBridge) throws java.io.IOException;
		 ReadableLogChannel GetReader( LogPosition position, LogVersionBridge logVersionBridge );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(LogFile_LogFileVisitor visitor, org.Neo4Net.kernel.impl.transaction.log.LogPosition startingFromPosition) throws java.io.IOException;
		 void Accept( LogFile_LogFileVisitor visitor, LogPosition startingFromPosition );

		 /// <returns> {@code true} if a rotation is needed. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
		 bool RotationNeeded();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void rotate() throws java.io.IOException;
		 void Rotate();
	}

	 public interface LogFile_LogFileVisitor
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visit(org.Neo4Net.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel channel) throws java.io.IOException;
		  bool Visit( ReadableClosablePositionAwareChannel channel );
	 }

}