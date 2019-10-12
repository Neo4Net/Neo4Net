using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Storageengine.Api
{

	public sealed class StoreId : Externalizable
	{
		 public static readonly StoreId Default = new StoreId( -1, -1, -1, -1, -1 );

		 private static readonly Random _r = new SecureRandom();

		 private long _creationTime;
		 private long _randomId;
		 private long _storeVersion;
		 private long _upgradeTime;
		 private long _upgradeId;

		 public StoreId()
		 {
			  //For the readExternal method.
		 }

		 public StoreId( long storeVersion )
		 {
			  // If creationTime == upgradeTime && randomNumber == upgradeId then store has never been upgraded
			  long currentTimeMillis = DateTimeHelper.CurrentUnixTimeMillis();
			  long randomLong = _r.nextLong();
			  this._storeVersion = storeVersion;
			  this._creationTime = currentTimeMillis;
			  this._randomId = randomLong;
			  this._upgradeTime = currentTimeMillis;
			  this._upgradeId = randomLong;
		 }

		 public StoreId( long creationTime, long randomId, long storeVersion, long upgradeTime, long upgradeId )
		 {
			  this._creationTime = creationTime;
			  this._randomId = randomId;
			  this._storeVersion = storeVersion;
			  this._upgradeTime = upgradeTime;
			  this._upgradeId = upgradeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static StoreId from(java.io.ObjectInput in) throws java.io.IOException
		 public static StoreId From( ObjectInput @in )
		 {
			  StoreId storeId = new StoreId();
			  storeId.ReadExternal( @in );
			  return storeId;
		 }

		 public long CreationTime
		 {
			 get
			 {
				  return _creationTime;
			 }
		 }

		 public long RandomId
		 {
			 get
			 {
				  return _randomId;
			 }
		 }

		 public long UpgradeTime
		 {
			 get
			 {
				  return _upgradeTime;
			 }
		 }

		 public long UpgradeId
		 {
			 get
			 {
				  return _upgradeId;
			 }
		 }

		 public long StoreVersion
		 {
			 get
			 {
				  return _storeVersion;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  @out.writeLong( _creationTime );
			  @out.writeLong( _randomId );
			  @out.writeLong( _storeVersion );
			  @out.writeLong( _upgradeTime );
			  @out.writeLong( _upgradeId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  _creationTime = @in.readLong();
			  _randomId = @in.readLong();
			  _storeVersion = @in.readLong();
			  _upgradeTime = @in.readLong();
			  _upgradeId = @in.readLong();
		 }

		 public bool EqualsByUpgradeId( StoreId other )
		 {
			  return InternalEqual( _upgradeTime, other._upgradeTime ) && InternalEqual( _upgradeId, other._upgradeId );
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
			  StoreId other = ( StoreId ) o;
			  return InternalEqual( _creationTime, other._creationTime ) && InternalEqual( _randomId, other._randomId );
		 }

		 public override int GetHashCode()
		 {
			  return 31 * ( int )( _creationTime ^ ( ( long )( ( ulong )_creationTime >> 32 ) ) ) + ( int )( _randomId ^ ( ( long )( ( ulong )_randomId >> 32 ) ) );
		 }

		 public override string ToString()
		 {
			  return "StoreId{" +
						 "creationTime=" + _creationTime +
						 ", randomId=" + _randomId +
						 ", storeVersion=" + _storeVersion +
						 ", upgradeTime=" + _upgradeTime +
						 ", upgradeId=" + _upgradeId +
						 '}';
		 }

		 private static bool InternalEqual( long first, long second )
		 {
			  return first == second || first == -1 || second == -1;
		 }
	}

}