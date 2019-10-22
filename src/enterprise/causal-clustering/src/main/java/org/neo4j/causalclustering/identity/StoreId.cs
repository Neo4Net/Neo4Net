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
namespace Neo4Net.causalclustering.identity
{

	public sealed class StoreId
	{
		 public static readonly StoreId Default = new StoreId( Neo4Net.Storageengine.Api.StoreId.Default.CreationTime, Neo4Net.Storageengine.Api.StoreId.Default.RandomId, Neo4Net.Storageengine.Api.StoreId.Default.UpgradeTime, Neo4Net.Storageengine.Api.StoreId.Default.UpgradeId );

		 public static bool IsDefault( StoreId storeId )
		 {
			  return storeId.CreationTime == Default.CreationTime && storeId.RandomId == Default.RandomId && storeId.UpgradeTime == Default.UpgradeTime && storeId.UpgradeId == Default.UpgradeId;
		 }

		 private long _creationTime;
		 private long _randomId;
		 private long _upgradeTime;
		 private long _upgradeId;

		 public StoreId( long creationTime, long randomId, long upgradeTime, long upgradeId )
		 {
			  this._creationTime = creationTime;
			  this._randomId = randomId;
			  this._upgradeTime = upgradeTime;
			  this._upgradeId = upgradeId;
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

		 public bool EqualToKernelStoreId( Neo4Net.Storageengine.Api.StoreId kernelStoreId )
		 {
			  return _creationTime == kernelStoreId.CreationTime && _randomId == kernelStoreId.RandomId && _upgradeTime == kernelStoreId.UpgradeTime && _upgradeId == kernelStoreId.UpgradeId;
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
			  if ( IsDefault( this ) )
			  {
					return false;
			  }
			  StoreId storeId = ( StoreId ) o;
			  if ( IsDefault( storeId ) )
			  {
					return false;
			  }
			  return _creationTime == storeId._creationTime && _randomId == storeId._randomId && _upgradeTime == storeId._upgradeTime && _upgradeId == storeId._upgradeId;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _creationTime, _randomId, _upgradeTime, _upgradeId );
		 }

		 public override string ToString()
		 {
			  return format( "Store{creationTime:%d, randomId:%s, upgradeTime:%d, upgradeId:%d}", _creationTime, _randomId, _upgradeTime, _upgradeId );
		 }
	}

}