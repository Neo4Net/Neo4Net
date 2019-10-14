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
namespace Neo4Net.cluster
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Uris.parameter;

	/// <summary>
	/// Represents the concept of the cluster wide unique id of an instance. The
	/// main requirement is total order over the instances of the class, currently
	/// implemented by an encapsulated integer.
	/// It is also expected to be serializable, as it's transmitted over the wire
	/// as part of messages between instances.
	/// </summary>
	public class InstanceId : Externalizable, IComparable<InstanceId>
	{
		 public static readonly InstanceId None = new InstanceId( int.MinValue );

		 private int _serverId;

		 public InstanceId()
		 {
		 }

		 public InstanceId( int serverId )
		 {
			  this._serverId = serverId;
		 }

		 public override int CompareTo( InstanceId o )
		 {
			  return Integer.compare( _serverId, o._serverId );
		 }

		 public override int GetHashCode()
		 {
			  return _serverId;
		 }

		 public override string ToString()
		 {
			  return Convert.ToString( _serverId );
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

			  InstanceId instanceId1 = ( InstanceId ) o;

			  return _serverId == instanceId1._serverId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  @out.writeInt( _serverId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  _serverId = @in.readInt();
		 }

		 public virtual int ToIntegerIndex()
		 {
			  return _serverId;
		 }

		 public virtual string InstanceNameFromURI( URI member )
		 {
			  string name = member == null ? null : parameter( "memberName" ).apply( member );
			  return string.ReferenceEquals( name, null ) ? ToString() : name;
		 }
	}

}