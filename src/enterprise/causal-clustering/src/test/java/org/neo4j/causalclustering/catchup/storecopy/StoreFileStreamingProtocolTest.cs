using System.Collections.Generic;

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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.Cursors;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse.Status.E_STORE_ID_MISMATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse.Status.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Cursors.rawCursorOf;

	public class StoreFileStreamingProtocolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStreamResources() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStreamResources()
		 {
			  // given
			  StoreFileStreamingProtocol protocol = new StoreFileStreamingProtocol();
			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );

			  Fs.mkdir( new File( "dirA" ) );
			  Fs.mkdir( new File( "dirB" ) );

			  string[] files = new string[]{ "dirA/one", "dirA/two", "dirB/one", "dirB/two", "one", "two", "three" };

			  IList<StoreResource> resourceList = new List<StoreResource>();
			  foreach ( string file in files )
			  {
					resourceList.Add( CreateResource( new File( file ), ThreadLocalRandom.current().Next(1, 4096) ) );
			  }
			  RawCursor<StoreResource, IOException> resources = rawCursorOf( resourceList );

			  // when
			  while ( resources.Next() )
			  {
					protocol.Stream( ctx, resources.get() );
			  }

			  // then
			  InOrder inOrder = Mockito.inOrder( ctx );

			  foreach ( StoreResource resource in resourceList )
			  {
					inOrder.verify( ctx ).write( ResponseMessageType.FILE );
					inOrder.verify( ctx ).write( new FileHeader( resource.Path(), resource.RecordSize() ) );
					inOrder.verify( ctx ).write( new FileSender( resource ) );
			  }
			  verifyNoMoreInteractions( ctx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEndWithFailure()
		 public virtual void ShouldBeAbleToEndWithFailure()
		 {
			  // given
			  StoreFileStreamingProtocol protocol = new StoreFileStreamingProtocol();
			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );

			  // when
			  protocol.End( ctx, E_STORE_ID_MISMATCH );

			  // then
			  InOrder inOrder = Mockito.inOrder( ctx );
			  inOrder.verify( ctx ).write( ResponseMessageType.STORE_COPY_FINISHED );
			  inOrder.verify( ctx ).writeAndFlush( new StoreCopyFinishedResponse( E_STORE_ID_MISMATCH ) );
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEndWithSuccess()
		 public virtual void ShouldBeAbleToEndWithSuccess()
		 {
			  // given
			  StoreFileStreamingProtocol protocol = new StoreFileStreamingProtocol();
			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );

			  // when
			  protocol.End( ctx, StoreCopyFinishedResponse.Status.Success );

			  // then
			  InOrder inOrder = Mockito.inOrder( ctx );
			  inOrder.verify( ctx ).write( ResponseMessageType.STORE_COPY_FINISHED );
			  inOrder.verify( ctx ).writeAndFlush( new StoreCopyFinishedResponse( SUCCESS ) );
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private StoreResource createResource(java.io.File file, int recordSize) throws java.io.IOException
		 private StoreResource CreateResource( File file, int recordSize )
		 {
			  Fs.create( file );
			  return new StoreResource( file, file.Path, recordSize, Fs );
		 }
	}

}