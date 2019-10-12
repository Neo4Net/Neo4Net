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
namespace Org.Neo4j.cluster.member.paxos
{

	/// <summary>
	/// This message is broadcast when a member of the cluster declares that
	/// it is not ready to serve a particular role for the cluster.
	/// </summary>
	public class MemberIsUnavailable : Externalizable
	{
		 private string _role;
		 private InstanceId _instanceId;
		 private URI _clusterUri;

		 public MemberIsUnavailable()
		 {
		 }

		 public MemberIsUnavailable( string role, InstanceId instanceId, URI clusterUri )
		 {
			  this._role = role;
			  this._instanceId = instanceId;
			  this._clusterUri = clusterUri;
		 }

		 public virtual string Role
		 {
			 get
			 {
				  return _role;
			 }
		 }

		 public virtual InstanceId InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual URI ClusterUri
		 {
			 get
			 {
				  return _clusterUri;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  @out.writeUTF( _role );
			  @out.writeObject( _instanceId );
			  if ( _clusterUri != null )
			  {
					@out.writeUTF( _clusterUri.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException, ClassNotFoundException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  _role = @in.readUTF();
			  _instanceId = ( InstanceId ) @in.readObject();
			  if ( @in.available() != 0 )
			  {
					_clusterUri = URI.create( @in.readUTF() );
			  }
		 }

		 public override string ToString()
		 {
			  return string.Format( "MemberIsUnavailable[ Role: {0}, InstanceId: {1}, ClusterURI: {2} ]", _role, _instanceId.ToString(), (_clusterUri == null) ? null : _clusterUri.ToString() );
		 }
	}

}