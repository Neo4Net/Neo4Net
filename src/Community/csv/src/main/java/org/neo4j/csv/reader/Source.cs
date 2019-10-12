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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// Source of data chunks to read.
	/// </summary>
	public interface Source : System.IDisposable
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Source_Chunk nextChunk(int seekStartPos) throws java.io.IOException;
		 Source_Chunk NextChunk( int seekStartPos );

		 /// <summary>
		 /// One chunk of data to read.
		 /// </summary>

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Chunk EMPTY_CHUNK = new Chunk()
	//	 {
	//		  @@Override public int startPosition()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public String sourceDescription()
	//		  {
	//				return "EMPTY";
	//		  }
	//
	//		  @@Override public int maxFieldSize()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public int length()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public char[] data()
	//		  {
	//				return null;
	//		  }
	//
	//		  @@Override public int backPosition()
	//		  {
	//				return 0;
	//		  }
	//	 };

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static Source singleChunk(Source_Chunk chunk)
	//	 {
	//		  return new Source()
	//		  {
	//				private boolean returned;
	//
	//				@@Override public void close()
	//				{ // Nothing to close
	//				}
	//
	//				@@Override public Chunk nextChunk(int seekStartPos)
	//				{
	//					 if (!returned)
	//					 {
	//						  returned = true;
	//						  return chunk;
	//					 }
	//					 return EMPTY_CHUNK;
	//				}
	//		  };
	//	 }
	}

	 public interface Source_Chunk
	 {
		  /// <returns> character data to read </returns>
		  char[] Data();

		  /// <returns> number of effective characters in the <seealso cref="data()"/> </returns>
		  int Length();

		  /// <returns> effective capacity of the <seealso cref="data()"/> array </returns>
		  int MaxFieldSize();

		  /// <returns> source description of the source this chunk was read from </returns>
		  string SourceDescription();

		  /// <returns> position in the <seealso cref="data()"/> array to start reading from </returns>
		  int StartPosition();

		  /// <returns> position in the <seealso cref="data()"/> array where the current field which is being
		  /// read starts. Some characters of the current field may have started in the previous chunk
		  /// and so those characters are transfered over to this data array before <seealso cref="startPosition()"/> </returns>
		  int BackPosition();
	 }

}