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
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using IgnoredMessage = Neo4Net.Bolt.v1.messaging.response.IgnoredMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.BoltResponseMessage.IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.map;

	public class BoltResponseMessageWriterV1Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteRecordMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteRecordMessage()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  writer.Write( new RecordMessage( () => new AnyValue[]{ longValue(42), stringValue("42") } ) );

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).pack( longValue( 42 ) );
			  inOrder.verify( packer ).pack( stringValue( "42" ) );
			  inOrder.verify( output ).messageSucceeded();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteSuccessMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteSuccessMessage()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  MapValue metadata = map( new string[]{ "a", "b", "c" }, new AnyValue[]{ intValue( 1 ), stringValue( "2" ), date( 2010, 0x2, 0x2 ) } );
			  writer.Write( new SuccessMessage( metadata ) );

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).pack( metadata );
			  inOrder.verify( output ).messageSucceeded();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteFailureMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteFailureMessage()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  Neo4Net.Kernel.Api.Exceptions.Status_Transaction errorStatus = Neo4Net.Kernel.Api.Exceptions.Status_Transaction.DeadlockDetected;
			  string errorMessage = "Hi Deadlock!";
			  writer.Write( new FailureMessage( errorStatus, errorMessage ) );

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).pack( errorStatus.code().serialize() );
			  inOrder.verify( packer ).pack( errorMessage );
			  inOrder.verify( output ).messageSucceeded();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteIgnoredMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteIgnoredMessage()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  writer.Write( IgnoredMessage.IGNORED_MESSAGE );

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).packStructHeader( 0, IGNORED.signature() );
			  inOrder.verify( output ).messageSucceeded();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlush() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlush()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  writer.Flush();

			  verify( packer ).flush();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyOutputAboutFailedRecordMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotifyOutputAboutFailedRecordMessage()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );
			  IOException error = new IOException( "Unable to pack 42" );
			  doThrow( error ).when( packer ).pack( longValue( 42 ) );

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  try
			  {
					writer.Write( new RecordMessage( () => new AnyValue[]{ stringValue("42"), longValue(42) } ) );
					fail( "Exception expected" );
			  }
			  catch ( IOException e )
			  {
					assertEquals( error, e );
			  }

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).pack( stringValue( "42" ) );
			  inOrder.verify( packer ).pack( longValue( 42 ) );
			  inOrder.verify( output ).messageFailed();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotNotifyOutputWhenOutputItselfFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotNotifyOutputWhenOutputItselfFails()
		 {
			  PackOutput output = mock( typeof( PackOutput ) );
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = mock( typeof( Neo4Net.Bolt.messaging.Neo4NetPack_Packer ) );
			  IOException error = new IOException( "Unable to flush" );
			  doThrow( error ).when( output ).messageSucceeded();

			  BoltResponseMessageWriterV1 writer = NewWriter( output, packer );

			  try
			  {
					writer.Write( new RecordMessage( () => new AnyValue[]{ longValue(1), longValue(2) } ) );
					fail( "Exception expected" );
			  }
			  catch ( IOException e )
			  {
					assertEquals( error, e );
			  }

			  InOrder inOrder = inOrder( output, packer );
			  inOrder.verify( output ).beginMessage();
			  inOrder.verify( packer ).pack( longValue( 1 ) );
			  inOrder.verify( packer ).pack( longValue( 2 ) );
			  inOrder.verify( output ).messageSucceeded();

			  verify( output, never() ).messageFailed();
		 }

		 private static BoltResponseMessageWriterV1 NewWriter( PackOutput output, Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer )
		 {
			  return new BoltResponseMessageWriterV1( @out => packer, output, NullLogService.Instance );
		 }
	}

}