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
namespace Neo4Net.com
{

	public class ClientCrashingWriter : MadeUpWriter
	{
		 private readonly MadeUpClient _client;
		 private readonly int _crashAtSize;
		 private int _totalSize;

		 public ClientCrashingWriter( MadeUpClient client, int crashAtSize )
		 {
			  this._client = client;
			  this._crashAtSize = crashAtSize;
		 }

		 public override void Write( ReadableByteChannel data )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( 1000 );
			  while ( true )
			  {
					buffer.clear();
					try
					{
						 int size = data.read( buffer );
						 if ( size == -1 )
						 {
							  break;
						 }
						 if ( ( _totalSize += size ) >= _crashAtSize )
						 {
							  _client.stop();
							  throw new IOException( "Fake read error" );
						 }
					}
					catch ( IOException e )
					{
						 throw new ComException( e );
					}
			  }
		 }

		 public virtual int SizeRead
		 {
			 get
			 {
				  return _totalSize;
			 }
		 }
	}

}