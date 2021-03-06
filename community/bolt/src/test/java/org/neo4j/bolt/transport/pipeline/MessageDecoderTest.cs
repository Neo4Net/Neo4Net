﻿using System;
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
namespace Org.Neo4j.Bolt.transport.pipeline
{
	using Unpooled = io.netty.buffer.Unpooled;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using BoltIOException = Org.Neo4j.Bolt.messaging.BoltIOException;
	using BoltRequestMessageReader = Org.Neo4j.Bolt.messaging.BoltRequestMessageReader;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using SynchronousBoltConnection = Org.Neo4j.Bolt.runtime.SynchronousBoltConnection;
	using BoltRequestMessageReaderV1 = Org.Neo4j.Bolt.v1.messaging.BoltRequestMessageReaderV1;
	using Neo4jPackV1 = Org.Neo4j.Bolt.v1.messaging.Neo4jPackV1;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using PackedOutputArray = Org.Neo4j.Bolt.v1.packstream.PackedOutputArray;
	using Neo4jPackV2 = Org.Neo4j.Bolt.v2.messaging.Neo4jPackV2;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.ByteBufUtil.hexDump;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.refEq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Edges.ALICE_KNOWS_BOB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Nodes.ALICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.example.Paths.ALL_PATHS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.durationValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class MessageDecoderTest
	public class MessageDecoderTest
	{
		 private EmbeddedChannel _channel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public org.neo4j.bolt.messaging.Neo4jPack packerUnderTest;
		 public Neo4jPack PackerUnderTest;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{1}") public static Object[][] testParameters()
		 public static object[][] TestParameters()
		 {
			  return new object[][]
			  {
				  new object[]
				  {
					  new Neo4jPackV1(),
					  "V1"
				  },
				  new object[]
				  {
					  new Neo4jPackV2(),
					  "V2"
				  }
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _channel != null )
			  {
					_channel.finishAndReleaseAll();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchInit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchInit()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  string userAgent = "Test/User Agent 1.0";
			  IDictionary<string, object> authToken = MapUtil.map( "scheme", "basic", "principal", "user", "credentials", "password" );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, new InitMessage( userAgent, authToken ) ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( refEq( new InitMessage( userAgent, authToken ), "authToken" ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchAckFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchAckFailure()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, AckFailureMessage.INSTANCE ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( eq( AckFailureMessage.INSTANCE ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchReset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchReset()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, ResetMessage.INSTANCE ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( eq( ResetMessage.INSTANCE ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchRun() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchRun()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  string statement = "RETURN 1";
			  MapValue parameters = ValueUtils.asMapValue( MapUtil.map( "param1", 1, "param2", "2", "param3", true, "param4", 5.0 ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, new RunMessage( statement, parameters ) ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( eq( new RunMessage( statement, parameters ) ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchDiscardAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchDiscardAll()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, DiscardAllMessage.INSTANCE ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( eq( DiscardAllMessage.INSTANCE ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDispatchPullAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDispatchPullAll()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, PullAllMessage.INSTANCE ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).process( eq( PullAllMessage.INSTANCE ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnInitWithNullKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnInitWithNullKeys()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  string userAgent = "Test/User Agent 1.0";
			  IDictionary<string, object> authToken = MapUtil.map( "scheme", "basic", null, "user", "credentials", "password" );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( PackerUnderTest, new InitMessage( userAgent, authToken ) ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).handleExternalFailure( eq( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." ) ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnInitWithDuplicateKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnInitWithDuplicateKeys()
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  // Generate INIT message with duplicate keys
			  PackedOutputArray @out = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = PackerUnderTest.newPacker( @out );
			  packer.PackStructHeader( 2, InitMessage.SIGNATURE );
			  packer.Pack( "Test/User Agent 1.0" );
			  packer.PackMapHeader( 3 );
			  packer.Pack( "scheme" );
			  packer.Pack( "basic" );
			  packer.Pack( "principal" );
			  packer.Pack( "user" );
			  packer.Pack( "scheme" );
			  packer.Pack( "password" );

			  _channel.writeInbound( Unpooled.wrappedBuffer( @out.Bytes() ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).handleExternalFailure( eq( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid, "Duplicate map key `scheme`." ) ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnNodeParameter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnNodeParameter()
		 {
			  TestUnpackableStructParametersWithKnownType( ALICE, "Node values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnRelationshipParameter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnRelationshipParameter()
		 {
			  TestUnpackableStructParametersWithKnownType( ALICE_KNOWS_BOB, "Relationship values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnPathParameter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnPathParameter()
		 {
			  foreach ( PathValue path in ALL_PATHS )
			  {
					TestUnpackableStructParametersWithKnownType( path, "Path values cannot be unpacked with this version of bolt." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnDuration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnDuration()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), durationValue(Duration.ofDays(10)), "Duration values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnDate()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(LocalDate.now()), "LocalDate values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnLocalTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnLocalTime()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(LocalTime.now()), "LocalTime values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnTime()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(OffsetTime.now()), "OffsetTime values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnLocalDateTime() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnLocalDateTime()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(DateTime.Now), "LocalDateTime values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnDateTimeWithOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnDateTimeWithOffset()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(OffsetDateTime.now()), "OffsetDateTime values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallExternalErrorOnDateTimeWithZoneName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallExternalErrorOnDateTimeWithZoneName()
		 {
			  assumeThat( PackerUnderTest.version(), equalTo(1L) );

			  TestUnpackableStructParametersWithKnownType( new Neo4jPackV2(), ValueUtils.of(ZonedDateTime.now()), "ZonedDateTime values cannot be unpacked with this version of bolt." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnUnknownStructType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnUnknownStructType()
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = PackerUnderTest.newPacker( @out );
			  packer.PackStructHeader( 2, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN $x" );
			  packer.PackMapHeader( 1 );
			  packer.Pack( "x" );
			  packer.PackStructHeader( 0, ( sbyte ) 'A' );

			  try
			  {
					Unpack( @out.Bytes() );
			  }
			  catch ( BoltIOException ex )
			  {
					assertThat( ex.Message, equalTo( "Struct types of 0x41 are not recognized." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogContentOfTheMessageOnIOError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogContentOfTheMessageOnIOError()
		 {
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  BoltResponseMessageWriter responseMessageHandler = mock( typeof( BoltResponseMessageWriter ) );

			  BoltRequestMessageReader requestMessageReader = new BoltRequestMessageReaderV1( connection, responseMessageHandler, NullLogService.Instance );

			  LogService logService = mock( typeof( LogService ) );
			  Log log = mock( typeof( Log ) );
			  when( logService.GetInternalLog( typeof( MessageDecoder ) ) ).thenReturn( log );

			  _channel = new EmbeddedChannel( new MessageDecoder( PackerUnderTest.newUnpacker, requestMessageReader, logService ) );

			  sbyte invalidMessageSignature = sbyte.MaxValue;
			  sbyte[] messageBytes = PackMessageWithSignature( invalidMessageSignature );

			  try
			  {
					_channel.writeInbound( Unpooled.wrappedBuffer( messageBytes ) );
					fail( "Exception expected" );
			  }
			  catch ( Exception )
			  {
			  }

			  AssertMessageHexDumpLogged( log, messageBytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogContentOfTheMessageOnError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogContentOfTheMessageOnError()
		 {
			  BoltRequestMessageReader requestMessageReader = mock( typeof( BoltRequestMessageReader ) );
			  Exception error = new Exception( "Hello!" );
			  doThrow( error ).when( requestMessageReader ).read( any() );

			  LogService logService = mock( typeof( LogService ) );
			  Log log = mock( typeof( Log ) );
			  when( logService.GetInternalLog( typeof( MessageDecoder ) ) ).thenReturn( log );

			  _channel = new EmbeddedChannel( new MessageDecoder( PackerUnderTest.newUnpacker, requestMessageReader, logService ) );

			  sbyte[] messageBytes = PackMessageWithSignature( RunMessage.SIGNATURE );

			  try
			  {
					_channel.writeInbound( Unpooled.wrappedBuffer( messageBytes ) );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertEquals( error, e );
			  }

			  AssertMessageHexDumpLogged( log, messageBytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testUnpackableStructParametersWithKnownType(org.neo4j.values.AnyValue parameterValue, String expectedMessage) throws Exception
		 private void TestUnpackableStructParametersWithKnownType( AnyValue parameterValue, string expectedMessage )
		 {
			  TestUnpackableStructParametersWithKnownType( PackerUnderTest, parameterValue, expectedMessage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testUnpackableStructParametersWithKnownType(org.neo4j.bolt.messaging.Neo4jPack packerForSerialization, org.neo4j.values.AnyValue parameterValue, String expectedMessage) throws Exception
		 private void TestUnpackableStructParametersWithKnownType( Neo4jPack packerForSerialization, AnyValue parameterValue, string expectedMessage )
		 {
			  string statement = "RETURN $x";
			  MapValue parameters = VirtualValues.map( new string[]{ "x" }, new AnyValue[]{ parameterValue } );

			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( serialize( packerForSerialization, new RunMessage( statement, parameters ) ) ) );
			  _channel.finishAndReleaseAll();

			  verify( stateMachine ).handleExternalFailure( eq( Neo4jError.from( Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.TypeError, expectedMessage ) ), any() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unpack(byte[] input) throws java.io.IOException
		 private void Unpack( sbyte[] input )
		 {
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  SynchronousBoltConnection connection = new SynchronousBoltConnection( stateMachine );
			  _channel = new EmbeddedChannel( NewDecoder( connection ) );

			  _channel.writeInbound( Unpooled.wrappedBuffer( input ) );
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] packMessageWithSignature(byte signature) throws java.io.IOException
		 private sbyte[] PackMessageWithSignature( sbyte signature )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer = PackerUnderTest.newPacker( @out );
			  packer.PackStructHeader( 2, signature );
			  packer.Pack( "RETURN 'Hello World!'" );
			  packer.pack( EMPTY_MAP );
			  return @out.Bytes();
		 }

		 private MessageDecoder NewDecoder( BoltConnection connection )
		 {
			  BoltRequestMessageReader reader = new BoltRequestMessageReaderV1( connection, mock( typeof( BoltResponseMessageWriter ) ), NullLogService.Instance );
			  return new MessageDecoder( PackerUnderTest.newUnpacker, reader, NullLogService.Instance );
		 }

		 private static void AssertMessageHexDumpLogged( Log logMock, sbyte[] messageBytes )
		 {
			  ArgumentCaptor<string> captor = ArgumentCaptor.forClass( typeof( string ) );
			  verify( logMock ).error( captor.capture() );
			  assertThat( captor.Value, containsString( hexDump( messageBytes ) ) );
		 }
	}

}