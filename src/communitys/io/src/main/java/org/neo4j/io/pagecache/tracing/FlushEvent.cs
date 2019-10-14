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
namespace Neo4Net.Io.pagecache.tracing
{

	/// <summary>
	/// Begin flushing modifications from an in-memory page to the backing file.
	/// </summary>
	public interface FlushEvent
	{
		 /// <summary>
		 /// A FlushEvent implementation that does nothing.
		 /// </summary>
	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 FlushEvent NULL = new FlushEvent()
	//	 {
	//		  @@Override public void addBytesWritten(long bytes)
	//		  {
	//		  }
	//
	//		  @@Override public void done()
	//		  {
	//		  }
	//
	//		  @@Override public void done(IOException exception)
	//		  {
	//		  }
	//
	//		  @@Override public void addPagesFlushed(int pageCount)
	//		  {
	//		  }
	//	 };

		 /// <summary>
		 /// Add up a number of bytes that has been written to the file.
		 /// </summary>
		 void AddBytesWritten( long bytes );

		 /// <summary>
		 /// The page flush has completed successfully.
		 /// </summary>
		 void Done();

		 /// <summary>
		 /// The page flush did not complete successfully, but threw the given exception.
		 /// </summary>
		 void Done( IOException exception );

		 void AddPagesFlushed( int pageCount );
	}

}