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
namespace Neo4Net.Bolt.v3.messaging
{

	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using SynchronousBoltConnection = Neo4Net.Bolt.runtime.SynchronousBoltConnection;
	using BoltRequestMessageWriter = Neo4Net.Bolt.v1.messaging.BoltRequestMessageWriter;
	using RecordingByteChannel = Neo4Net.Bolt.v1.messaging.RecordingByteChannel;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using Neo4NetPackV2 = Neo4Net.Bolt.v2.messaging.Neo4NetPackV2;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.util.MessageMatchers.serialize;

	/// <summary>
	/// A helper factory to generate boltV3 component in tests
	/// </summary>
	public class BoltProtocolV3ComponentFactory
	{
		 public static Neo4NetPack NewNeo4NetPack()
		 {
			  return new Neo4NetPackV2();
		 }

		 public static BoltRequestMessageWriter RequestMessageWriter( Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer )
		 {
			  return new BoltRequestMessageWriterV3( packer );
		 }

		 public static BoltRequestMessageReader RequestMessageReader( BoltStateMachine stateMachine )
		 {
			  return new BoltRequestMessageReaderV3( new SynchronousBoltConnection( stateMachine ), mock( typeof( BoltResponseMessageWriter ) ), NullLogService.Instance );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException
		 public static sbyte[] Encode( Neo4NetPack Neo4NetPack, params RequestMessage[] messages )
		 {
			  RecordingByteChannel rawData = new RecordingByteChannel();
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = Neo4NetPack.NewPacker( new BufferedChannelOutput( rawData ) );
			  BoltRequestMessageWriter writer = RequestMessageWriter( packer );

			  foreach ( RequestMessage message in messages )
			  {
					writer.Write( message );
			  }
			  writer.Flush();

			  return rawData.Bytes;
		 }

		 public static TransportTestUtil.MessageEncoder NewMessageEncoder()
		 {
			  return new MessageEncoderAnonymousInnerClass();
		 }

		 private class MessageEncoderAnonymousInnerClass : TransportTestUtil.MessageEncoder
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.RequestMessage... messages) throws java.io.IOException
			 public sbyte[] encode( Neo4NetPack Neo4NetPack, params RequestMessage[] messages )
			 {
				  return BoltProtocolV3ComponentFactory.Encode( Neo4NetPack, messages );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] encode(org.Neo4Net.bolt.messaging.Neo4NetPack Neo4NetPack, org.Neo4Net.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
			 public sbyte[] encode( Neo4NetPack Neo4NetPack, params ResponseMessage[] messages )
			 {
				  return serialize( Neo4NetPack, messages );
			 }
		 }
	}

}