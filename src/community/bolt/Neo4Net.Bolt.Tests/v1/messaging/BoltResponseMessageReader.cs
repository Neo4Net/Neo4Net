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
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using IgnoredMessage = Neo4Net.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using PackStream = Neo4Net.Bolt.v1.packstream.PackStream;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnyValue = Neo4Net.Values.AnyValue;
	using StringValue = Neo4Net.Values.Storable.StringValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.spi.Records.record;

	public class BoltResponseMessageReader
	{
		 private readonly Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker _unpacker;

		 public BoltResponseMessageReader( Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker unpacker )
		 {
			  this._unpacker = unpacker;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(Neo4Net.bolt.messaging.BoltResponseMessageWriter messageWriter) throws java.io.IOException
		 public virtual void Read( BoltResponseMessageWriter messageWriter )
		 {
			  try
			  {
					_unpacker.unpackStructHeader();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int signature = (int) unpacker.unpackStructSignature();
					int signature = ( int ) _unpacker.unpackStructSignature();
					BoltResponseMessage message = BoltResponseMessage.withSignature( signature );
					try
					{
						 switch ( message.innerEnumValue )
						 {
						 case Neo4Net.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.SUCCESS:
							  MapValue successMetadata = _unpacker.unpackMap();
							  messageWriter.Write( new SuccessMessage( successMetadata ) );
							  break;
						 case Neo4Net.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.RECORD:
							  long length = _unpacker.unpackListHeader();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.AnyValue[] fields = new Neo4Net.values.AnyValue[(int) length];
							  AnyValue[] fields = new AnyValue[( int ) length];
							  for ( int i = 0; i < length; i++ )
							  {
									fields[i] = _unpacker.unpack();
							  }
							  messageWriter.Write( new RecordMessage( record( fields ) ) );
							  break;
						 case Neo4Net.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.IGNORED:
							  messageWriter.Write( IgnoredMessage.IGNORED_MESSAGE );
							  break;
						 case Neo4Net.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.FAILURE:
							  MapValue failureMetadata = _unpacker.unpackMap();
							  string code = failureMetadata.ContainsKey( "code" ) ? ( ( StringValue ) failureMetadata.Get( "code" ) ).stringValue() : Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError.name();
							  string msg = failureMetadata.ContainsKey( "message" ) ? ( ( StringValue ) failureMetadata.Get( "message" ) ).stringValue() : "<No message supplied>";
							  messageWriter.Write( new FailureMessage( Neo4NetError.codeFromString( code ), msg ) );
							  break;
						 default:
							  throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Message 0x{0} is not supported.", signature.ToString( "x" ) ) );
						 }
					}
					catch ( System.ArgumentException )
					{
						 throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Message 0x{0} is not a valid message signature.", signature.ToString( "x" ) ) );
					}
			  }
			  catch ( PackStream.PackStreamException e )
			  {
					throw new BoltIOException( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Unable to read message type. Error was: {0}.", e.Message ), e );
			  }
		 }

	}

}