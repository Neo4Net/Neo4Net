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
namespace Neo4Net.Bolt.v1.messaging.util
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using IgnoredMessage = Neo4Net.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using BufferedChannelInput = Neo4Net.Bolt.v1.packstream.BufferedChannelInput;
	using BufferedChannelOutput = Neo4Net.Bolt.v1.packstream.BufferedChannelOutput;
	using TestNotification = Neo4Net.Bolt.v1.transport.integration.TestNotification;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using Node = Neo4Net.Graphdb.Node;
	using Notification = Neo4Net.Graphdb.Notification;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4Net.Kernel.impl.util;
	using HexPrinter = Neo4Net.Kernel.impl.util.HexPrinter;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class MessageMatchers
	public class MessageMatchers
	{
		 private MessageMatchers()
		 {
		 }

		 private static IDictionary<string, object> ToRawMap( MapValue mapValue )
		 {
			  Deserializer deserializer = new Deserializer();
			  Dictionary<string, object> map = new Dictionary<string, object>( mapValue.Size() );
			  mapValue.Foreach((key, value) =>
			  {
			  value.writeTo( deserializer );
			  map[key] = deserializer.Value();
			  });

			  return map;
		 }

		 private class Deserializer : BaseToObjectValueWriter<Exception>
		 {

			  protected internal override Node NewNodeProxyById( long id )
			  {
					return null;
			  }

			  protected internal override Relationship NewRelationshipProxyById( long id )
			  {
					return null;
			  }

			  protected internal override Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate )
			  {
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<java.util.List<org.neo4j.bolt.messaging.ResponseMessage>> equalsMessages(final org.hamcrest.Matcher<org.neo4j.bolt.messaging.ResponseMessage>... messageMatchers)
		 public static Matcher<IList<ResponseMessage>> EqualsMessages( params Matcher<ResponseMessage>[] messageMatchers )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( messageMatchers );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IList<ResponseMessage>>
		 {
			 private Matcher<ResponseMessage>[] _messageMatchers;

			 public TypeSafeMatcherAnonymousInnerClass( Matcher<ResponseMessage>[] messageMatchers )
			 {
				 this._messageMatchers = messageMatchers;
			 }

			 protected internal override bool matchesSafely( IList<ResponseMessage> messages )
			 {
				  if ( _messageMatchers.Length != messages.Count )
				  {
						return false;
				  }
				  for ( int i = 0; i < _messageMatchers.Length; i++ )
				  {
						if ( !_messageMatchers[i].matches( messages[i] ) )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendList( "MessageList[", ", ", "]", Arrays.asList( _messageMatchers ) );
			 }
		 }

		 public static Matcher<ResponseMessage> HasNotification( Notification notification )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( notification );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<ResponseMessage>
		 {
			 private Notification _notification;

			 public TypeSafeMatcherAnonymousInnerClass2( Notification notification )
			 {
				 this._notification = notification;
			 }

			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( SuccessMessage ) ) );
				  IDictionary<string, object> meta = ToRawMap( ( ( SuccessMessage ) t ).meta() );

				  assertThat( meta.ContainsKey( "notifications" ), @is( true ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				  ISet<Notification> notifications = ( ( IList<IDictionary<string, object>> ) meta["notifications"] ).Select( TestNotification.fromMap ).collect( Collectors.toSet() );

				  assertThat( notifications, Matchers.contains( _notification ) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "SUCCESS" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<org.neo4j.bolt.messaging.ResponseMessage> msgSuccess(final java.util.Map<String,Object> metadata)
		 public static Matcher<ResponseMessage> MsgSuccess( IDictionary<string, object> metadata )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass3( metadata );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass3 : TypeSafeMatcher<ResponseMessage>
		 {
			 private IDictionary<string, object> _metadata;

			 public TypeSafeMatcherAnonymousInnerClass3( IDictionary<string, object> metadata )
			 {
				 this._metadata = metadata;
			 }

			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( SuccessMessage ) ) );
				  assertThat( ToRawMap( ( ( SuccessMessage ) t ).meta() ), equalTo(_metadata) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "SUCCESS" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<org.neo4j.bolt.messaging.ResponseMessage> msgSuccess(final org.hamcrest.Matcher<java.util.Map<String,?>> matcher)
		 public static Matcher<ResponseMessage> MsgSuccess<T1>( Matcher<T1> matcher )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass4( matcher );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass4 : TypeSafeMatcher<ResponseMessage>
		 {
			 private Matcher<T1> _matcher;

			 public TypeSafeMatcherAnonymousInnerClass4( Matcher<T1> matcher )
			 {
				 this._matcher = matcher;
			 }

			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( SuccessMessage ) ) );
				  IDictionary<string, object> actual = ToRawMap( ( ( SuccessMessage ) t ).meta() );
				  assertThat( actual, _matcher );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "SUCCESS" );
			 }
		 }

		 public static Matcher<ResponseMessage> MsgSuccess()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass5();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass5 : TypeSafeMatcher<ResponseMessage>
		 {
			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( SuccessMessage ) ) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "SUCCESS" );
			 }
		 }

		 public static Matcher<ResponseMessage> MsgIgnored()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass6();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass6 : TypeSafeMatcher<ResponseMessage>
		 {
			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( IgnoredMessage ) ) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "IGNORED" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<org.neo4j.bolt.messaging.ResponseMessage> msgFailure(final org.neo4j.kernel.api.exceptions.Status status, final String message)
		 public static Matcher<ResponseMessage> MsgFailure( Status status, string message )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass7( status, message );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass7 : TypeSafeMatcher<ResponseMessage>
		 {
			 private Status _status;
			 private string _message;

			 public TypeSafeMatcherAnonymousInnerClass7( Status status, string message )
			 {
				 this._status = status;
				 this._message = message;
			 }

			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( FailureMessage ) ) );
				  FailureMessage msg = ( FailureMessage ) t;
				  assertThat( msg.Status(), equalTo(_status) );
				  assertThat( msg.Message(), containsString(_message) );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "FAILURE" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<org.neo4j.bolt.messaging.ResponseMessage> msgRecord(final org.hamcrest.Matcher<org.neo4j.cypher.result.QueryResult_Record> matcher)
		 public static Matcher<ResponseMessage> MsgRecord( Matcher<Neo4Net.Cypher.result.QueryResult_Record> matcher )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass8( matcher );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass8 : TypeSafeMatcher<ResponseMessage>
		 {
			 private Matcher<Neo4Net.Cypher.result.QueryResult_Record> _matcher;

			 public TypeSafeMatcherAnonymousInnerClass8( Matcher<Neo4Net.Cypher.result.QueryResult_Record> matcher )
			 {
				 this._matcher = matcher;
			 }

			 protected internal override bool matchesSafely( ResponseMessage t )
			 {
				  assertThat( t, instanceOf( typeof( RecordMessage ) ) );

				  RecordMessage msg = ( RecordMessage ) t;
				  assertThat( msg.Record(), _matcher );
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "RECORD " );
				  description.appendDescriptionOf( _matcher );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] serialize(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, org.neo4j.bolt.messaging.RequestMessage... messages) throws java.io.IOException
		 public static sbyte[] Serialize( Neo4jPack neo4jPack, params RequestMessage[] messages )
		 {
			  RecordingByteChannel rawData = new RecordingByteChannel();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = neo4jPack.NewPacker( new BufferedChannelOutput( rawData ) );
			  BoltRequestMessageWriter writer = new BoltRequestMessageWriter( packer );

			  foreach ( RequestMessage message in messages )
			  {
					writer.Write( message );
			  }
			  writer.Flush();

			  return rawData.Bytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] serialize(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, org.neo4j.bolt.messaging.ResponseMessage... messages) throws java.io.IOException
		 public static sbyte[] Serialize( Neo4jPack neo4jPack, params ResponseMessage[] messages )
		 {
			  RecordingByteChannel rawData = new RecordingByteChannel();
			  BufferedChannelOutput output = new BufferedChannelOutput( rawData );
			  BoltResponseMessageWriterV1 writer = new BoltResponseMessageWriterV1( neo4jPack.newPacker, output, NullLogService.Instance );

			  foreach ( ResponseMessage message in messages )
			  {
					writer.Write( message );
			  }
			  writer.Flush();

			  return rawData.Bytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.bolt.messaging.ResponseMessage responseMessage(org.neo4j.bolt.messaging.Neo4jPack neo4jPack, byte[] bytes) throws java.io.IOException
		 public static ResponseMessage ResponseMessage( Neo4jPack neo4jPack, sbyte[] bytes )
		 {
			  BoltResponseMessageReader unpacker = ResponseReader( neo4jPack, bytes );
			  BoltResponseMessageRecorder consumer = new BoltResponseMessageRecorder();

			  try
			  {
					unpacker.Read( consumer );
					return consumer.AsList()[0];
			  }
			  catch ( Exception e )
			  {
					throw new IOException( "Failed to deserialize response, '" + e.Message + "'.\n" + "Raw data: \n" + HexPrinter.hex( bytes ), e );
			  }
		 }

		 private static BoltResponseMessageReader ResponseReader( Neo4jPack neo4jPack, sbyte[] bytes )
		 {
			  BufferedChannelInput input = new BufferedChannelInput( 128 );
			  input.Reset( new ArrayByteChannel( bytes ) );
			  return new BoltResponseMessageReader( neo4jPack.NewUnpacker( input ) );
		 }

	}

}