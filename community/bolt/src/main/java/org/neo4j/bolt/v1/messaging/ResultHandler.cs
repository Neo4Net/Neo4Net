﻿/*
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
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using RecordMessage = Org.Neo4j.Bolt.v1.messaging.response.RecordMessage;
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using Log = Org.Neo4j.Logging.Log;
	using AnyValue = Org.Neo4j.Values.AnyValue;

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

		 private class BoltResult_VisitorAnonymousInnerClass : Org.Neo4j.Bolt.runtime.BoltResult_Visitor
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
			 public void visit( Org.Neo4j.Cypher.result.QueryResult_Record record )
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