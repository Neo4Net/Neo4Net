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
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using FailureMessage = Org.Neo4j.Bolt.v1.messaging.response.FailureMessage;
	using IgnoredMessage = Org.Neo4j.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Org.Neo4j.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Org.Neo4j.Bolt.v1.messaging.response.SuccessMessage;
	using PackStream = Org.Neo4j.Bolt.v1.packstream.PackStream;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using StringValue = Org.Neo4j.Values.Storable.StringValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.Records.record;

	public class BoltResponseMessageReader
	{
		 private readonly Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker _unpacker;

		 public BoltResponseMessageReader( Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker unpacker )
		 {
			  this._unpacker = unpacker;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(org.neo4j.bolt.messaging.BoltResponseMessageWriter messageWriter) throws java.io.IOException
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
						 case Org.Neo4j.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.SUCCESS:
							  MapValue successMetadata = _unpacker.unpackMap();
							  messageWriter.Write( new SuccessMessage( successMetadata ) );
							  break;
						 case Org.Neo4j.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.RECORD:
							  long length = _unpacker.unpackListHeader();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.AnyValue[] fields = new org.neo4j.values.AnyValue[(int) length];
							  AnyValue[] fields = new AnyValue[( int ) length];
							  for ( int i = 0; i < length; i++ )
							  {
									fields[i] = _unpacker.unpack();
							  }
							  messageWriter.Write( new RecordMessage( record( fields ) ) );
							  break;
						 case Org.Neo4j.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.IGNORED:
							  messageWriter.Write( IgnoredMessage.IGNORED_MESSAGE );
							  break;
						 case Org.Neo4j.Bolt.v1.messaging.BoltResponseMessage.InnerEnum.FAILURE:
							  MapValue failureMetadata = _unpacker.unpackMap();
							  string code = failureMetadata.ContainsKey( "code" ) ? ( ( StringValue ) failureMetadata.Get( "code" ) ).stringValue() : Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError.name();
							  string msg = failureMetadata.ContainsKey( "message" ) ? ( ( StringValue ) failureMetadata.Get( "message" ) ).stringValue() : "<No message supplied>";
							  messageWriter.Write( new FailureMessage( Neo4jError.codeFromString( code ), msg ) );
							  break;
						 default:
							  throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Message 0x{0} is not supported.", signature.ToString( "x" ) ) );
						 }
					}
					catch ( System.ArgumentException )
					{
						 throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Message 0x{0} is not a valid message signature.", signature.ToString( "x" ) ) );
					}
			  }
			  catch ( PackStream.PackStreamException e )
			  {
					throw new BoltIOException( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Unable to read message type. Error was: {0}.", e.Message ), e );
			  }
		 }

	}

}