using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.machines.token
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;


	using BoundedNetworkWritableChannel = Neo4Net.causalclustering.messaging.BoundedNetworkWritableChannel;
	using NetworkReadableClosableChannelNetty4 = Neo4Net.causalclustering.messaging.NetworkReadableClosableChannelNetty4;
	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;
	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using InvalidLogEntryHandler = Neo4Net.Kernel.impl.transaction.log.entry.InvalidLogEntryHandler;
	using LogEntryCommand = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class ReplicatedTokenRequestSerializer
	{
		 private ReplicatedTokenRequestSerializer()
		 {
			  throw new AssertionError( "Should not be instantiated" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(ReplicatedTokenRequest content, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
		 public static void Marshal( ReplicatedTokenRequest content, WritableChannel channel )
		 {
			  channel.PutInt( content.Type().ordinal() );
			  StringMarshal.marshal( channel, content.TokenName() );

			  channel.PutInt( content.CommandBytes().Length );
			  channel.Put( content.CommandBytes(), content.CommandBytes().Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ReplicatedTokenRequest unmarshal(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public static ReplicatedTokenRequest Unmarshal( ReadableChannel channel )
		 {
			  TokenType type = Enum.GetValues( typeof( TokenType ) )[channel.Int];
			  string tokenName = StringMarshal.unmarshal( channel );

			  int commandBytesLength = channel.Int;
			  sbyte[] commandBytes = new sbyte[commandBytesLength];
			  channel.Get( commandBytes, commandBytesLength );

			  return new ReplicatedTokenRequest( type, tokenName, commandBytes );
		 }

		 public static void Marshal( ReplicatedTokenRequest content, ByteBuf buffer )
		 {
			  buffer.writeInt( content.Type().ordinal() );
			  StringMarshal.marshal( buffer, content.TokenName() );

			  buffer.writeInt( content.CommandBytes().Length );
			  buffer.writeBytes( content.CommandBytes() );
		 }

		 public static ReplicatedTokenRequest Unmarshal( ByteBuf buffer )
		 {
			  TokenType type = Enum.GetValues( typeof( TokenType ) )[buffer.readInt()];
			  string tokenName = StringMarshal.unmarshal( buffer );

			  int commandBytesLength = buffer.readInt();
			  sbyte[] commandBytes = new sbyte[commandBytesLength];
			  buffer.readBytes( commandBytes );

			  return new ReplicatedTokenRequest( type, tokenName, commandBytes );
		 }

		 public static sbyte[] CommandBytes( ICollection<StorageCommand> commands )
		 {
			  ByteBuf commandBuffer = Unpooled.buffer();
			  BoundedNetworkWritableChannel channel = new BoundedNetworkWritableChannel( commandBuffer );

			  try
			  {
					( new LogEntryWriter( channel ) ).serialize( commands );
			  }
			  catch ( IOException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace ); // TODO: Handle or throw.
			  }

			  /*
			   * This trims down the array to send up to the actual index it was written. Not doing this would send additional
			   * zeroes which not only wasteful, but also not handled by the LogEntryReader receiving this.
			   */
			  sbyte[] commandsBytes = Arrays.copyOf( commandBuffer.array(), commandBuffer.writerIndex() );
			  commandBuffer.release();

			  return commandsBytes;
		 }

		 internal static ICollection<StorageCommand> ExtractCommands( sbyte[] commandBytes )
		 {
			  ByteBuf txBuffer = Unpooled.wrappedBuffer( commandBytes );
			  NetworkReadableClosableChannelNetty4 channel = new NetworkReadableClosableChannelNetty4( txBuffer );

			  LogEntryReader<ReadableClosablePositionAwareChannel> reader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>( new RecordStorageCommandReaderFactory(), InvalidLogEntryHandler.STRICT );

			  LogEntryCommand entryRead;
			  IList<StorageCommand> commands = new LinkedList<StorageCommand>();

			  try
			  {
					while ( ( entryRead = ( LogEntryCommand ) reader.ReadLogEntry( channel ) ) != null )
					{
						 commands.Add( entryRead.Command );
					}
			  }
			  catch ( IOException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace ); // TODO: Handle or throw.
			  }

			  return commands;
		 }
	}

}