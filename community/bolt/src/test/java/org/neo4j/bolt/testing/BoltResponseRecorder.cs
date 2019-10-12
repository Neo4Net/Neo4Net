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
namespace Org.Neo4j.Bolt.testing
{

	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using AnyValue = Org.Neo4j.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringOrNoValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class BoltResponseRecorder : BoltResponseHandler
	{
		 private BlockingQueue<RecordedBoltResponse> _responses;
		 private RecordedBoltResponse _currentResponse;

		 public BoltResponseRecorder()
		 {
			  Reset();
		 }

		 public virtual void Reset()
		 {
			  _responses = new LinkedBlockingQueue<RecordedBoltResponse>();
			  _currentResponse = new RecordedBoltResponse();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void onRecords(org.neo4j.bolt.runtime.BoltResult result, boolean pull) throws Exception
		 public override void OnRecords( BoltResult result, bool pull )
		 {
			  result.Accept( new BoltResult_VisitorAnonymousInnerClass( this ) );
		 }

		 private class BoltResult_VisitorAnonymousInnerClass : Org.Neo4j.Bolt.runtime.BoltResult_Visitor
		 {
			 private readonly BoltResponseRecorder _outerInstance;

			 public BoltResult_VisitorAnonymousInnerClass( BoltResponseRecorder outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void visit( Org.Neo4j.Cypher.result.QueryResult_Record record )
			 {
				  _outerInstance.currentResponse.addRecord( record );
			 }

			 public void addMetadata( string key, AnyValue value )
			 {
				  _outerInstance.currentResponse.addMetadata( key, value );
			 }
		 }

		 public override void OnMetadata( string key, AnyValue value )
		 {
			  _currentResponse.addMetadata( key, value );
		 }

		 public override void MarkIgnored()
		 {
			  _currentResponse.Response = IGNORED;
		 }

		 public override void MarkFailed( Neo4jError error )
		 {
			  _currentResponse.Response = FAILURE;
			  OnMetadata( "code", stringValue( error.Status().code().serialize() ) );
			  OnMetadata( "message", stringOrNoValue( error.Message() ) );
		 }

		 public override void OnFinish()
		 {
			  if ( _currentResponse.message() == null )
			  {
					_currentResponse.Response = SUCCESS;
			  }
			  _responses.add( _currentResponse );
			  _currentResponse = new RecordedBoltResponse();
		 }

		 public virtual int ResponseCount()
		 {
			  return _responses.size();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RecordedBoltResponse nextResponse() throws InterruptedException
		 public virtual RecordedBoltResponse NextResponse()
		 {
			  RecordedBoltResponse response = _responses.poll( 3, SECONDS );
			  assertNotNull( "No message arrived after 3s", response );
			  return response;
		 }

	}

}