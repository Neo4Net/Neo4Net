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
namespace Neo4Net.cluster.protocol.atomicbroadcast
{

	/// <summary>
	/// Serializes and deserializes value to/from Payloads.
	/// </summary>
	public class AtomicBroadcastSerializer
	{
		 private ObjectInputStreamFactory _objectInputStreamFactory;
		 private ObjectOutputStreamFactory _objectOutputStreamFactory;

		 public AtomicBroadcastSerializer() : this(new ObjectStreamFactory(), new ObjectStreamFactory())
		 {
		 }

		 public AtomicBroadcastSerializer( ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory )
		 {
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Payload broadcast(Object value) throws java.io.IOException
		 public virtual Payload Broadcast( object value )
		 {
			  MemoryStream bout = new MemoryStream();
			  sbyte[] bytes;
			  using ( ObjectOutputStream oout = _objectOutputStreamFactory.create( bout ) )
			  {
					oout.writeObject( value );
			  }
			  bytes = bout.toByteArray();
			  return new Payload( bytes, bytes.Length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object receive(Payload payload) throws java.io.IOException, ClassNotFoundException
		 public virtual object Receive( Payload payload )
		 {
			  MemoryStream @in = new MemoryStream( payload.Buf, 0, payload.Len );
			  using ( ObjectInputStream oin = _objectInputStreamFactory.create( @in ) )
			  {
					return oin.readObject();
			  }
		 }
	}

}