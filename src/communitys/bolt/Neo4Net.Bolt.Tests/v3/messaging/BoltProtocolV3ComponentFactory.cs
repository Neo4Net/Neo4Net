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
	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using SynchronousBoltConnection = Neo4Net.Bolt.runtime.SynchronousBoltConnection;
	using BoltRequestMessageWriter = Neo4Net.Bolt.v1.messaging.BoltRequestMessageWriter;
	using RecordingByteChannel = Neo4Net.Bolt.v1.messaging.RecordingByteChannel;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;

	/// <summary>
	/// A helper factory to generate boltV3 component in tests
	/// </summary>
	public class BoltProtocolV3ComponentFactory
	{
		 public static Neo4jPack NewNeo4jPack()
		 {
			  return new Neo4jPackV2();
		 }

		 public static BoltRequestMessageWriter RequestMessageWriter( Neo4Net.Bolt.messaging.Neo4jPack_Packer packer )
		 {
			  return new BoltRequestMessageWriterV3( packer );
		 }

		 public static BoltRequestMessageReader RequestMessageReader( BoltStateMachine stateMachine )
		 {
			  return new BoltRequestMessageReaderV3( new SynchronousBoltConnection( stateMachine ), mock( typeof( BoltResponseMessageWriter ) ), NullLogService.Instance );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] encode(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, org.neo4j.bolt.messaging.RequestMessage... messages) throws java.io.IOException
		 public static sbyte[] Encode( Neo4jPack neo4jPack, params RequestMessage[] messages )
		 {
			  RecordingByteChannel rawData = new RecordingByteChannel();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = neo4jPack.NewPacker( new BufferedChannelOutput( rawData ) );
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
//ORIGINAL LINE: public byte[] encode(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, org.neo4j.bolt.messaging.RequestMessage... messages) throws java.io.IOException
			 public sbyte[] encode( Neo4jPack neo4jPack, params RequestMessage[] messages )
			 {
				  return BoltProtocolV3ComponentFactory.Encode( neo4jPack, messages );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte[] encode(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, org.neo4j.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
			 public sbyte[] encode( Neo4jPack neo4jPack, params ResponseMessage[] messages )
			 {
				  return serialize( neo4jPack, messages );
			 }
		 }
	}

}