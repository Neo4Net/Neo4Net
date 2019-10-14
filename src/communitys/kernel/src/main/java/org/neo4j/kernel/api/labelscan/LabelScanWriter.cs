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
namespace Neo4Net.Kernel.api.labelscan
{

	public interface LabelScanWriter : System.IDisposable
	{
		 /// <summary>
		 /// Store a <seealso cref="NodeLabelUpdate"/>. Calls to this method MUST be ordered by ascending node id.
		 /// </summary>
		 /// <param name="update"> node label update to store </param>
		 /// <exception cref="IOException"> some kind of I/O exception has occurred </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void write(NodeLabelUpdate update) throws java.io.IOException;
		 void Write( NodeLabelUpdate update );

		 /// <summary>
		 /// Close this writer and flush pending changes to the store.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws java.io.IOException;
		 void Close();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 LabelScanWriter EMPTY = new LabelScanWriter()
	//	 {
	//		  @@Override public void write(NodeLabelUpdate update)
	//		  {
	//				// do nothing
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//				// nothing to close
	//		  }
	//	 };
	}

}