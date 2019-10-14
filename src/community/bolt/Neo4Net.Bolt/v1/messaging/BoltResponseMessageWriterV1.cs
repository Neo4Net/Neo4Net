using System;
using System.Collections.Generic;

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

	using BoltIOException = Neo4Net.Bolt.messaging.BoltIOException;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using PackProvider = Neo4Net.Bolt.messaging.PackProvider;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using Neo4Net.Bolt.messaging;
	using FailureMessageEncoder = Neo4Net.Bolt.v1.messaging.encoder.FailureMessageEncoder;
	using IgnoredMessageEncoder = Neo4Net.Bolt.v1.messaging.encoder.IgnoredMessageEncoder;
	using RecordMessageEncoder = Neo4Net.Bolt.v1.messaging.encoder.RecordMessageEncoder;
	using SuccessMessageEncoder = Neo4Net.Bolt.v1.messaging.encoder.SuccessMessageEncoder;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using FatalFailureMessage = Neo4Net.Bolt.v1.messaging.response.FatalFailureMessage;
	using IgnoredMessage = Neo4Net.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

	/// <summary>
	/// Writer for Bolt request messages to be sent to a <seealso cref="Neo4jPack.Packer"/>.
	/// </summary>
	public class BoltResponseMessageWriterV1 : BoltResponseMessageWriter
	{
		 private readonly PackOutput _output;
		 private readonly Neo4Net.Bolt.messaging.Neo4jPack_Packer _packer;
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
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, format( "Message %s is not supported in this protocol version.", message ) );
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