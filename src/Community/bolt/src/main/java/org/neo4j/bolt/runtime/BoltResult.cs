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
namespace Neo4Net.Bolt.runtime
{
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;

	/// <summary>
	/// Represents a data stream of records, this is the output cypher produces.
	/// 
	/// Streams contains nominally uniform records meaning each record has the same set of named fields.
	/// However, the contents of these fields may vary by both type and value and may be null.
	/// </summary>
	public interface BoltResult : AutoCloseable
	{
		 /// <summary>
		 /// Positional names for all fields in every record of this stream. </summary>
		 string[] FieldNames();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(BoltResult_Visitor visitor) throws Exception;
		 void Accept( BoltResult_Visitor visitor );

		 void Close();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 BoltResult EMPTY = new BoltResult()
	//	 {
	//		  private final String[] nothing = new String[0];
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//
	//		  @@Override public String[] fieldNames()
	//		  {
	//				return nothing;
	//		  }
	//
	//		  @@Override public void accept(Visitor visitor)
	//		  {
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "EmptyBoltResult{}";
	//		  }
	//	 };
	}

	 public interface BoltResult_Visitor
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visit(org.neo4j.cypher.result.QueryResult_Record record) throws Exception;
		  void Visit( Neo4Net.Cypher.result.QueryResult_Record record );

		  /// <summary>
		  /// Associate arbitrary metadata with the result stream. This will get transferred at the end of the stream.
		  /// Please stick to Neo4j type system types (Map, List, Integer, Float, Boolean, String etc) 
		  /// </summary>
		  void AddMetadata( string key, AnyValue value );
	 }

}