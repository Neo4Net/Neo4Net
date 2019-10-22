/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com.storecopy
{
	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;


	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class ToNetworkStoreWriter : StoreWriter
	{
		 public const string STORE_COPIER_MONITOR_TAG = "storeCopier";

		 private readonly ChannelBuffer _targetBuffer;
		 private readonly ByteCounterMonitor _bufferMonitor;

		 public ToNetworkStoreWriter( ChannelBuffer targetBuffer, Monitors monitors )
		 {
			  this._targetBuffer = targetBuffer;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _bufferMonitor = monitors.NewMonitor( typeof( ByteCounterMonitor ), this.GetType().FullName, STORE_COPIER_MONITOR_TAG );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(String path, java.nio.channels.ReadableByteChannel data, ByteBuffer temporaryBuffer, boolean hasData, int requiredElementAlignment) throws java.io.IOException
		 public override long Write( string path, ReadableByteChannel data, ByteBuffer temporaryBuffer, bool hasData, int requiredElementAlignment )
		 {
			  char[] chars = path.ToCharArray();
			  _targetBuffer.writeShort( chars.Length );
			  Protocol.writeChars( _targetBuffer, chars );
			  _targetBuffer.writeByte( hasData ? 1 : 0 );
			  // TODO Make use of temporaryBuffer?
			  BlockLogBuffer buffer = new BlockLogBuffer( _targetBuffer, _bufferMonitor );
			  long totalWritten = Short.BYTES + chars.Length * Character.BYTES + Byte.BYTES;
			  if ( hasData )
			  {
					_targetBuffer.writeInt( requiredElementAlignment );
					totalWritten += Integer.BYTES;
					totalWritten += buffer.Write( data );
					buffer.Dispose();
			  }
			  return totalWritten;
		 }

		 public override void Close()
		 {
			  _targetBuffer.writeShort( 0 );
		 }
	}

}