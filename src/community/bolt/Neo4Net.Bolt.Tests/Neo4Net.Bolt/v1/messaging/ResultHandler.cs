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
namespace Neo4Net.Bolt.v1.messaging
{
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using Log = Neo4Net.Logging.Log;
	using AnyValue = Neo4Net.Values.AnyValue;

	public class ResultHandler : MessageProcessingHandler
	{
		 public ResultHandler( BoltResponseMessageWriter handler, BoltConnection connection, Log log ) : base( handler, connection, log )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void onRecords(final org.neo4j.bolt.runtime.BoltResult result, final boolean pull) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override void OnRecords( BoltResult result, bool pull )
		 {
			  result.Accept( new BoltResult_VisitorAnonymousInnerClass( this, pull ) );
		 }

		 private class BoltResult_VisitorAnonymousInnerClass : Neo4Net.Bolt.runtime.BoltResult_Visitor
		 {
			 private readonly ResultHandler _outerInstance;

			 private bool _pull;

			 public BoltResult_VisitorAnonymousInnerClass( ResultHandler outerInstance, bool pull )
			 {
				 this.outerInstance = outerInstance;
				 this._pull = pull;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visit(org.neo4j.cypher.result.QueryResult_Record record) throws Exception
			 public void visit( Neo4Net.Cypher.result.QueryResult_Record record )
			 {
				  if ( _pull )
				  {
						_outerInstance.messageWriter.write( new RecordMessage( record ) );
				  }
			 }

			 public void addMetadata( string key, AnyValue value )
			 {
				  outerInstance.OnMetadata( key, value );
			 }
		 }
	}

}