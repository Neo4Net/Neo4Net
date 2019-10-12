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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using Future = io.netty.util.concurrent.Future;


	using Org.Neo4j.Cursor;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.storecopy.StoreCopyFinishedResponse.Status.SUCCESS;

	public class StoreStreamingProcess
	{
		 private readonly StoreFileStreamingProtocol _protocol;
		 private readonly System.Func<CheckPointer> _checkPointerSupplier;
		 private readonly StoreCopyCheckPointMutex _mutex;
		 private readonly StoreResourceStreamFactory _resourceStreamFactory;

		 public StoreStreamingProcess( StoreFileStreamingProtocol protocol, System.Func<CheckPointer> checkPointerSupplier, StoreCopyCheckPointMutex mutex, StoreResourceStreamFactory resourceStreamFactory )
		 {
			  this._protocol = protocol;
			  this._checkPointerSupplier = checkPointerSupplier;
			  this._mutex = mutex;
			  this._resourceStreamFactory = resourceStreamFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void perform(io.netty.channel.ChannelHandlerContext ctx) throws java.io.IOException
		 internal virtual void Perform( ChannelHandlerContext ctx )
		 {
			  CheckPointer checkPointer = _checkPointerSupplier.get();
			  Resource checkPointLock = _mutex.storeCopy( () => checkPointer.TryCheckPoint(new SimpleTriggerInfo("Store copy")) );

			  Future<Void> completion = null;
			  try
			  {
					  using ( RawCursor<StoreResource, IOException> resources = _resourceStreamFactory.create() )
					  {
						while ( resources.Next() )
						{
							 StoreResource resource = resources.get();
							 _protocol.stream( ctx, resource );
						}
						completion = _protocol.end( ctx, SUCCESS );
					  }
			  }
			  finally
			  {
					if ( completion != null )
					{
						 completion.addListener( f => checkPointLock.close() );
					}
					else
					{
						 checkPointLock.Close();
					}
			  }
		 }

		 public virtual void Fail( ChannelHandlerContext ctx, StoreCopyFinishedResponse.Status failureCode )
		 {
			  _protocol.end( ctx, failureCode );
		 }
	}

}