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
	/// AtomicBroadcast payload. Wraps a byte buffer and its length.
	/// </summary>
	public class Payload : Externalizable
	{
		 private sbyte[] _buf;
		 private int _len;

		 /// <summary>
		 /// Externalizable constructor
		 /// </summary>
		 public Payload()
		 {
		 }

		 public Payload( sbyte[] buf, int len )
		 {
			  this._buf = buf;
			  this._len = len;
		 }

		 public virtual sbyte[] Buf
		 {
			 get
			 {
				  return _buf;
			 }
		 }

		 public virtual int Len
		 {
			 get
			 {
				  return _len;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  // NOTE: This was changed from writing only a byte in 2.2, which doesn't work
			  @out.writeInt( _len );
			  @out.write( _buf, 0, _len );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  // NOTE: This was changed from reading only a byte in 2.2, which doesn't work
			  _len = @in.readInt();
			  _buf = new sbyte[_len];
			  @in.read( _buf, 0, _len );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  Payload payload = ( Payload ) o;

			  if ( _len != payload._len )
			  {
					return false;
			  }
			  return Arrays.Equals( _buf, payload._buf );
		 }

		 public override int GetHashCode()
		 {
			  int result = _buf != null ? Arrays.GetHashCode( _buf ) : 0;
			  result = 31 * result + _len;
			  return result;
		 }
	}

}