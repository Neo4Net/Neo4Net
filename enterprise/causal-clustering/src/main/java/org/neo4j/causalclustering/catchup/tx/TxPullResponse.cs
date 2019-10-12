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
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;

	public class TxPullResponse
	{
		 private readonly StoreId _storeId;
		 private readonly CommittedTransactionRepresentation _tx;

		 public TxPullResponse( StoreId storeId, CommittedTransactionRepresentation tx )
		 {
			  this._storeId = storeId;
			  this._tx = tx;
		 }

		 public virtual StoreId StoreId()
		 {
			  return _storeId;
		 }

		 public virtual CommittedTransactionRepresentation Tx()
		 {
			  return _tx;
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

			  TxPullResponse that = ( TxPullResponse ) o;

			  return ( _storeId != null ? _storeId.Equals( that._storeId ) : that._storeId == null ) && ( _tx != null ? _tx.Equals( that._tx ) : that._tx == null );
		 }

		 public override int GetHashCode()
		 {
			  int result = _storeId != null ? _storeId.GetHashCode() : 0;
			  result = 31 * result + ( _tx != null ? _tx.GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return string.Format( "TxPullResponse{{storeId={0}, tx={1}}}", _storeId, _tx );
		 }
	}

}