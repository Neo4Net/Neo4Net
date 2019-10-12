using System;
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

	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using PackProvider = Org.Neo4j.Bolt.messaging.PackProvider;
	using ResponseMessage = Org.Neo4j.Bolt.messaging.ResponseMessage;
	using Org.Neo4j.Bolt.messaging;
	using FailureMessageEncoder = Org.Neo4j.Bolt.v1.messaging.encoder.FailureMessageEncoder;
	using IgnoredMessageEncoder = Org.Neo4j.Bolt.v1.messaging.encoder.IgnoredMessageEncoder;
	using RecordMessageEncoder = Org.Neo4j.Bolt.v1.messaging.encoder.RecordMessageEncoder;
	using SuccessMessageEncoder = Org.Neo4j.Bolt.v1.messaging.encoder.SuccessMessageEncoder;
	using FailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FailureMessage;
	using FatalFailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FatalFailureMessage;
	using IgnoredMessage = Org.Neo4j.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Org.Neo4j.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Org.Neo4j.Bolt.v1.messaging.response.SuccessMessage;
	using PackOutput = Org.Neo4j.Bolt.v1.packstream.PackOutput;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	/// <summary>
	/// Writer for Bolt request messages to be sent to a <seealso cref="Neo4jPack.Packer"/>.
	/// </summary>
	public class BoltResponseMessageWriterV1 : BoltResponseMessageWriter
	{
		 private readonly PackOutput _output;
		 private readonly Org.Neo4j.Bolt.messaging.Neo4jPack_Packer _packer;
		 private readonly Log _log;
		 private readonly IDictionary<sbyte, ResponseMessageEncoder<ResponseMessage>> _encoders;

		 public BoltResponseMessageWriterV1( PackProvider packerProvider, PackOutput output, LogService logService )
		 {
			  this._output = output;
			  this._packer = packerProvider.NewPacker( output );
			  this._log = logService.GetInternalLog( this.GetType() );
			  this._encoders = RegisterEncoders();
		 }

		 private IDictionary<sbyte, ResponseMessageEncoder<ResponseMessage>> RegisterEncoders()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<sbyte,org.neo4j.bolt.messaging.ResponseMessageEncoder<?>> encoders = new java.util.HashMap<>();
			  IDictionary<sbyte, ResponseMessageEncoder<object>> encoders = new Dictionary<sbyte, ResponseMessageEncoder<object>>();
			  encoders[SuccessMessage.SIGNATURE] = new SuccessMessageEncoder();
			  encoders[RecordMessage.SIGNATURE] = new RecordMessageEncoder();
			  encoders[IgnoredMessage.SIGNATURE] = new IgnoredMessageEncoder();
			  encoders[FailureMessage.SIGNATURE] = new FailureMessageEncoder( _log );
			  return ( System.Collections.IDictionary )encoders;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.neo4j.bolt.messaging.ResponseMessage message) throws java.io.IOException
		 public override void Write( ResponseMessage message )
		 {
			  PackCompleteMessageOrFail( message );
			  if ( message is FatalFailureMessage )
			  {
					Flush();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public virtual void Flush()
		 {
			  _packer.flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void packCompleteMessageOrFail(org.neo4j.bolt.messaging.ResponseMessage message) throws java.io.IOException
		 private void PackCompleteMessageOrFail( ResponseMessage message )
		 {
			  bool packingFailed = true;
			  _output.beginMessage();
			  try
			  {
					ResponseMessageEncoder<ResponseMessage> encoder = _encoders[message.Signature()];
					if ( encoder == null )
					{
						 throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, format( "Message %s is not supported in this protocol version.", message ) );
					}
					encoder.Encode( _packer, message );
					packingFailed = false;
					_output.messageSucceeded();
			  }
			  catch ( Exception error )
			  {
					if ( packingFailed )
					{
						 // packing failed, there might be some half-written data in the output buffer right now
						 // notify output about the failure so that it cleans up the buffer
						 _output.messageFailed();
						 _log.error( "Failed to write full %s message because: %s", message, error.Message );
					}
					throw error;
			  }
		 }
	}

}