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
namespace Neo4Net.Bolt.testing
{

	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.BoltResponseMessage.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.BoltResponseMessage.IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringOrNoValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

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
//ORIGINAL LINE: public void onRecords(org.Neo4Net.bolt.runtime.BoltResult result, boolean pull) throws Exception
		 public override void OnRecords( BoltResult result, bool pull )
		 {
			  result.Accept( new BoltResult_VisitorAnonymousInnerClass( this ) );
		 }

		 private class BoltResult_VisitorAnonymousInnerClass : Neo4Net.Bolt.runtime.BoltResult_Visitor
		 {
			 private readonly BoltResponseRecorder _outerInstance;

			 public BoltResult_VisitorAnonymousInnerClass( BoltResponseRecorder outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void visit( Neo4Net.Cypher.result.QueryResult_Record record )
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

		 public override void MarkFailed( Neo4NetError error )
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