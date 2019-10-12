using System.Collections.Generic;

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
namespace Neo4Net.Bolt.testing
{

	using BoltResponseMessage = Neo4Net.Bolt.v1.messaging.BoltResponseMessage;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class RecordedBoltResponse
	{
		 private IList<Neo4Net.Cypher.result.QueryResult_Record> _records;
		 private BoltResponseMessage _response;
		 private IDictionary<string, AnyValue> _metadata;

		 public RecordedBoltResponse()
		 {
			  _records = new List<Neo4Net.Cypher.result.QueryResult_Record>();
			  _response = null;
			  _metadata = new Dictionary<string, AnyValue>();
		 }

		 public virtual void AddRecord( Neo4Net.Cypher.result.QueryResult_Record record )
		 {
			  _records.Add( record );
		 }

		 public virtual void AddMetadata( string key, AnyValue value )
		 {
			  _metadata[key] = value;
		 }

		 public virtual BoltResponseMessage Message()
		 {
			  return _response;
		 }

		 public virtual BoltResponseMessage Response
		 {
			 set
			 {
				  this._response = value;
			 }
		 }

		 public virtual bool HasMetadata( string key )
		 {
			  return _metadata.ContainsKey( key );
		 }

		 public virtual AnyValue Metadata( string key )
		 {
			  return _metadata[key];
		 }

		 public virtual void AssertRecord( int index, params object[] values )
		 {
			  assertThat( index, lessThan( _records.Count ) );
			  assertArrayEquals( _records[index].fields(), values );
		 }

		 public virtual Neo4Net.Cypher.result.QueryResult_Record[] Records()
		 {
			  Neo4Net.Cypher.result.QueryResult_Record[] recordArray = new Neo4Net.Cypher.result.QueryResult_Record[_records.Count];
			  return _records.toArray( recordArray );
		 }

		 public override string ToString()
		 {
			  return "RecordedBoltResponse{" + "records=" + _records + ", response=" + _response + ", metadata=" + _metadata + '}';
		 }
	}

}