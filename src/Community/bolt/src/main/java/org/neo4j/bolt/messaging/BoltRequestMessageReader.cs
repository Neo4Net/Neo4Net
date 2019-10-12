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
namespace Neo4Net.Bolt.messaging
{

	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using Neo4jError = Neo4Net.Bolt.runtime.Neo4jError;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;


	/// <summary>
	/// Reader for Bolt request messages made available via a <seealso cref="Neo4jPack.Unpacker"/>.
	/// </summary>
	public abstract class BoltRequestMessageReader
	{
		 private readonly BoltConnection _connection;
		 private readonly BoltResponseHandler _externalErrorResponseHandler;
		 private readonly IDictionary<int, RequestMessageDecoder> _decoders;

		 protected internal BoltRequestMessageReader( BoltConnection connection, BoltResponseHandler externalErrorResponseHandler, IList<RequestMessageDecoder> decoders )
		 {
			  this._connection = connection;
			  this._externalErrorResponseHandler = externalErrorResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  this._decoders = decoders.ToDictionary( RequestMessageDecoder::signature, identity() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 public virtual void Read( Neo4jPack_Unpacker unpacker )
		 {
			  try
			  {
					DoRead( unpacker );
			  }
			  catch ( BoltIOException e )
			  {
					if ( e.CausesFailureMessage() )
					{
						 Neo4jError error = Neo4jError.from( e );
						 _connection.enqueue( stateMachine => stateMachine.handleExternalFailure( error, _externalErrorResponseHandler ) );
					}
					else
					{
						 throw e;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doRead(Neo4jPack_Unpacker unpacker) throws java.io.IOException
		 private void DoRead( Neo4jPack_Unpacker unpacker )
		 {
			  try
			  {
					unpacker.UnpackStructHeader();
					int signature = unpacker.UnpackStructSignature();

					RequestMessageDecoder decoder = _decoders[signature];
					if ( decoder == null )
					{
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Message 0x{0} is not a valid message signature.", signature.ToString( "x" ) ) );
					}

					RequestMessage message = decoder.Decode( unpacker );
					BoltResponseHandler responseHandler = decoder.ResponseHandler();

					_connection.enqueue( stateMachine => stateMachine.process( message, responseHandler ) );
			  }
			  catch ( PackStream.PackStreamException e )
			  {
					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Unable to read message type. Error was: {0}.", e.Message ), e );
			  }
		 }
	}

}