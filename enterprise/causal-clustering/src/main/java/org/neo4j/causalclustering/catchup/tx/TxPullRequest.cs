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
namespace Org.Neo4j.causalclustering.catchup.tx
{

	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using CatchUpRequest = Org.Neo4j.causalclustering.messaging.CatchUpRequest;

	public class TxPullRequest : CatchUpRequest
	{
		 private long _previousTxId;
		 private readonly StoreId _expectedStoreId;

		 public TxPullRequest( long previousTxId, StoreId expectedStoreId )
		 {
			  this._previousTxId = previousTxId;
			  this._expectedStoreId = expectedStoreId;
		 }

		 /// <summary>
		 /// Request is for transactions after this id
		 /// </summary>
		 public virtual long PreviousTxId()
		 {
			  return _previousTxId;
		 }

		 public virtual StoreId ExpectedStoreId()
		 {
			  return _expectedStoreId;
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
			  TxPullRequest that = ( TxPullRequest ) o;
			  return _previousTxId == that._previousTxId && Objects.Equals( _expectedStoreId, that._expectedStoreId );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _previousTxId, _expectedStoreId );
		 }

		 public override string ToString()
		 {
			  return string.Format( "TxPullRequest{{txId={0:D}, storeId={1}}}", _previousTxId, _expectedStoreId );
		 }

		 public override RequestMessageType MessageType()
		 {
			  return RequestMessageType.TX_PULL_REQUEST;
		 }
	}

}