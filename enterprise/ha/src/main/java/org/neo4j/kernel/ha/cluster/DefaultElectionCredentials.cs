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
namespace Org.Neo4j.Kernel.ha.cluster
{

	using ElectionCredentials = Org.Neo4j.cluster.protocol.election.ElectionCredentials;

	public sealed class DefaultElectionCredentials : ElectionCredentials, Externalizable
	{
		 private int _serverId;
		 private long _latestTxId;
		 private bool _currentWinner;

		 // For Externalizable
		 public DefaultElectionCredentials()
		 {
		 }

		 public DefaultElectionCredentials( int serverId, long latestTxId, bool currentWinner )
		 {
			  this._serverId = serverId;
			  this._latestTxId = latestTxId;
			  this._currentWinner = currentWinner;
		 }

		 public override int CompareTo( ElectionCredentials o )
		 {
			  DefaultElectionCredentials other = ( DefaultElectionCredentials ) o;
			  if ( this._latestTxId == other._latestTxId )
			  {
					// Smaller id means higher priority
					if ( this._currentWinner == other._currentWinner )
					{
						 return Integer.compare( other._serverId, this._serverId );
					}
					else
					{
						 return other._currentWinner ? -1 : 1;
					}
			  }
			  else
			  {
					return Long.compare( this._latestTxId, other._latestTxId );
			  }
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj == null )
			  {
					return false;
			  }
			  if ( !( obj is DefaultElectionCredentials ) )
			  {
					return false;
			  }
			  DefaultElectionCredentials other = ( DefaultElectionCredentials ) obj;
			  return other._serverId == this._serverId && other._latestTxId == this._latestTxId && other._currentWinner == this._currentWinner;
		 }

		 public override int GetHashCode()
		 {
			  return ( int )( 17 * _serverId + _latestTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  @out.writeInt( _serverId );
			  @out.writeLong( _latestTxId );
			  @out.writeBoolean( _currentWinner );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  _serverId = @in.readInt();
			  _latestTxId = @in.readLong();
			  _currentWinner = @in.readBoolean();
		 }

		 public override string ToString()
		 {
			  return "DefaultElectionCredentials[serverId=" + _serverId +
						 ", latestTxId=" + _latestTxId +
						 ", currentWinner=" + _currentWinner + "]";
		 }
	}

}