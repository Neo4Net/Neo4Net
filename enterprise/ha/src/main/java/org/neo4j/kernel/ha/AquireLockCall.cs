﻿/*
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
namespace Org.Neo4j.Kernel.ha
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using LockResult = Org.Neo4j.Kernel.ha.@lock.LockResult;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	internal abstract class AcquireLockCall : TargetCaller<Master, LockResult>
	{
		public abstract Response<R> Call( T requestTarget, RequestContext context, ChannelBuffer input, ChannelBuffer target );
		 public override Response<LockResult> Call( Master master, RequestContext context, ChannelBuffer input, ChannelBuffer target )
		 {
			  ResourceType type = ResourceTypes.fromId( input.readInt() );
			  long[] ids = new long[input.readInt()];
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					ids[i] = input.readLong();
			  }
			  return Lock( master, context, type, ids );
		 }

		 protected internal abstract Response<LockResult> Lock( Master master, RequestContext context, ResourceType type, params long[] ids );
	}

}