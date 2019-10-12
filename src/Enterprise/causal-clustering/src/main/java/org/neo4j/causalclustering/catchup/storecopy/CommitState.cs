using System.Diagnostics;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	internal class CommitState
	{
		 private readonly long _metaDataStoreIndex;
		 private readonly long? _transactionLogIndex;

		 internal CommitState( long metaDataStoreIndex )
		 {
			  this._metaDataStoreIndex = metaDataStoreIndex;
			  this._transactionLogIndex = null;
		 }

		 internal CommitState( long metaDataStoreIndex, long transactionLogIndex )
		 {
			  Debug.Assert( transactionLogIndex >= metaDataStoreIndex );

			  this._metaDataStoreIndex = metaDataStoreIndex;
			  this._transactionLogIndex = transactionLogIndex;
		 }

		 internal virtual long MetaDataStoreIndex()
		 {
			  return _metaDataStoreIndex;
		 }

		 internal virtual long? TransactionLogIndex()
		 {
			  return Optional.ofNullable( _transactionLogIndex );
		 }

		 public override string ToString()
		 {
			  return "CommitState{" + "metaDataStoreIndex=" + _metaDataStoreIndex + ", transactionLogIndex=" + _transactionLogIndex + '}';
		 }
	}

}