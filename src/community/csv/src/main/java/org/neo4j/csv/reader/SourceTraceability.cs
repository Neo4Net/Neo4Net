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
namespace Neo4Net.Csv.Reader
{
	/// <summary>
	/// Provides information about a source of data.
	/// 
	/// An example usage would be reading a text file where <seealso cref="sourceDescription()"/> would say the name of the file,
	/// and <seealso cref="position()"/> the byte offset the reader is currently at.
	/// 
	/// Another example could be reading from a relationship db table where <seealso cref="sourceDescription()"/> would
	/// say the name of the database and table and <seealso cref="position()"/> some sort of absolute position saying
	/// the byte offset to the field.
	/// </summary>
	public interface ISourceTraceability
	{
		 /// <returns> description of the source being read from. </returns>
		 string SourceDescription();

		 /// <returns> a low-level byte-like position e.g. byte offset. </returns>
		 long Position();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 SourceTraceability EMPTY = new Adapter()
	//	 {
	//		  @@Override public String sourceDescription()
	//		  {
	//				return "EMPTY";
	//		  }
	//	 };
	}

	 public abstract class SourceTraceability_Adapter : SourceTraceability
	 {
		 public abstract string SourceDescription();
		  public override long Position()
		  {
				return 0;
		  }
	 }

}