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
namespace Org.Neo4j.Bolt.v1.messaging
{
	using Test = org.junit.jupiter.api.Test;


	using ResponseMessage = Org.Neo4j.Bolt.messaging.ResponseMessage;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using RecordMessage = Org.Neo4j.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Org.Neo4j.Bolt.v1.messaging.response.SuccessMessage;
	using ImmutableRecord = Org.Neo4j.Bolt.v1.runtime.spi.ImmutableRecord;
	using QueryResult_Record = Org.Neo4j.Cypher.result.QueryResult_Record;
	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.values;

	internal class ResultHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPullTheResult() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPullTheResult()
		 {
			  BoltResponseMessageRecorder messageWriter = new BoltResponseMessageRecorder();
			  ResultHandler handler = new ResultHandler( messageWriter, mock( typeof( BoltConnection ) ), NullLog.Instance );

			  ImmutableRecord record1 = new ImmutableRecord( values( "a", "b", "c" ) );
			  ImmutableRecord record2 = new ImmutableRecord( values( "1", "2", "3" ) );
			  BoltResult result = new TestBoltResult( record1, record2 );

			  handler.OnRecords( result, true );
			  handler.OnFinish();

			  IList<ResponseMessage> messages = messageWriter.AsList();
			  assertThat( messages.Count, equalTo( 3 ) );
			  assertThat( messages[0], equalTo( new RecordMessage( record1 ) ) );
			  assertThat( messages[1], equalTo( new RecordMessage( record2 ) ) );
			  assertThat( messages[2], instanceOf( typeof( SuccessMessage ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDiscardTheResult() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDiscardTheResult()
		 {
			  BoltResponseMessageRecorder messageWriter = new BoltResponseMessageRecorder();
			  ResultHandler handler = new ResultHandler( messageWriter, mock( typeof( BoltConnection ) ), NullLog.Instance );

			  ImmutableRecord record1 = new ImmutableRecord( values( "a", "b", "c" ) );
			  ImmutableRecord record2 = new ImmutableRecord( values( "1", "2", "3" ) );
			  BoltResult result = new TestBoltResult( record1, record2 );

			  handler.OnRecords( result, false );
			  handler.OnFinish();

			  IList<ResponseMessage> messages = messageWriter.AsList();
			  assertThat( messages.Count, equalTo( 1 ) );
			  assertThat( messages[0], instanceOf( typeof( SuccessMessage ) ) );
		 }

		 private class TestBoltResult : BoltResult
		 {
			  internal readonly QueryResult_Record[] Records;

			  internal TestBoltResult( params QueryResult_Record[] records )
			  {
					this.Records = records;
			  }

			  public override string[] FieldNames()
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(org.neo4j.bolt.runtime.BoltResult_Visitor visitor) throws Exception
			  public override void Accept( Org.Neo4j.Bolt.runtime.BoltResult_Visitor visitor )
			  {
					foreach ( QueryResult_Record record in Records )
					{
						 visitor.Visit( record );
					}
			  }

			  public override void Close()
			  {
			  }

			  public override string ToString()
			  {
					return "TestBoltResult{" + "records=" + Arrays.ToString( Records ) + '}';
			  }
		 }
	}

}