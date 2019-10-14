using System;

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
namespace Neo4Net.com
{

	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class ToChannelBufferWriter : MadeUpWriter
	{
		 private readonly ChannelBuffer _target;

		 public ToChannelBufferWriter( ChannelBuffer target )
		 {
			  this._target = target;
		 }

		 public override void Write( ReadableByteChannel data )
		 {
			  try
			  {
					  using ( BlockLogBuffer blockBuffer = new BlockLogBuffer( _target, ( new Monitors() ).newMonitor(typeof(ByteCounterMonitor)) ) )
					  {
						blockBuffer.Write( data );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}