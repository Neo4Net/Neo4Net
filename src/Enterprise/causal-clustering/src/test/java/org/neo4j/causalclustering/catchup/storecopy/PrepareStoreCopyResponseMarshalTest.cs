/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.catchup.storecopy
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PrepareStoreCopyResponseMarshalTest
	{
		 private EmbeddedChannel _embeddedChannel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _embeddedChannel = new EmbeddedChannel( new PrepareStoreCopyResponse.Encoder(), new PrepareStoreCopyResponse.Decoder() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionIdGetsTransmitted()
		 public virtual void TransactionIdGetsTransmitted()
		 {
			  // given
			  long transactionId = long.MaxValue;

			  // when a transaction id is serialised
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( new File[0], LongSets.immutable.empty(), transactionId );
			  SendToChannel( prepareStoreCopyResponse, _embeddedChannel );

			  // then it can be deserialised
			  PrepareStoreCopyResponse readPrepareStoreCopyResponse = _embeddedChannel.readInbound();
			  assertEquals( prepareStoreCopyResponse.LastTransactionId(), readPrepareStoreCopyResponse.LastTransactionId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fileListGetsTransmitted()
		 public virtual void FileListGetsTransmitted()
		 {
			  // given
			  File[] files = new File[]
			  {
				  new File( "File a.txt" ),
				  new File( "file-b" ),
				  new File( "aoifnoasndfosidfoisndfoisnodainfsonidfaosiidfna" ),
				  new File( "" )
			  };

			  // when
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( files, LongSets.immutable.empty(), 0L );
			  SendToChannel( prepareStoreCopyResponse, _embeddedChannel );

			  // then it can be deserialised
			  PrepareStoreCopyResponse readPrepareStoreCopyResponse = _embeddedChannel.readInbound();
			  assertEquals( prepareStoreCopyResponse.Files.Length, readPrepareStoreCopyResponse.Files.Length );
			  foreach ( File file in files )
			  {
					assertEquals( 1, Stream.of( readPrepareStoreCopyResponse.Files ).map( File.getName ).filter( f => f.Equals( file.Name ) ).count() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void descriptorsGetTransmitted()
		 public virtual void DescriptorsGetTransmitted()
		 {
			  // given
			  File[] files = new File[]
			  {
				  new File( "File a.txt" ),
				  new File( "file-b" ),
				  new File( "aoifnoasndfosidfoisndfoisnodainfsonidfaosiidfna" ),
				  new File( "" )
			  };
			  LongSet indexIds = LongSets.immutable.of( 13 );

			  // when
			  PrepareStoreCopyResponse prepareStoreCopyResponse = PrepareStoreCopyResponse.Success( files, indexIds, 1L );
			  SendToChannel( prepareStoreCopyResponse, _embeddedChannel );

			  // then it can be deserialised
			  PrepareStoreCopyResponse readPrepareStoreCopyResponse = _embeddedChannel.readInbound();
			  assertEquals( prepareStoreCopyResponse.Files.Length, readPrepareStoreCopyResponse.Files.Length );
			  foreach ( File file in files )
			  {
					assertEquals( 1, Stream.of( readPrepareStoreCopyResponse.Files ).map( File.getName ).filter( f => f.Equals( file.Name ) ).count() );
			  }
			  assertEquals( prepareStoreCopyResponse.IndexIds, readPrepareStoreCopyResponse.IndexIds );
		 }

		 private static void SendToChannel( PrepareStoreCopyResponse prepareStoreCopyResponse, EmbeddedChannel embeddedChannel )
		 {
			  embeddedChannel.writeOutbound( prepareStoreCopyResponse );

			  ByteBuf @object = embeddedChannel.readOutbound();
			  embeddedChannel.writeInbound( @object );
		 }
	}

}