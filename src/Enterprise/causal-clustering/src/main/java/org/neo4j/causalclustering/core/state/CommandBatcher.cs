using System;
using System.Collections.Generic;
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
namespace Neo4Net.causalclustering.core.state
{

	using DistributedOperation = Neo4Net.causalclustering.core.replication.DistributedOperation;
	using Neo4Net.Function;

	internal class CommandBatcher
	{
		 private IList<DistributedOperation> _batch;
		 private int _maxBatchSize;
		 private readonly ThrowingBiConsumer<long, IList<DistributedOperation>, Exception> _applier;
		 private long _lastIndex;

		 internal CommandBatcher( int maxBatchSize, ThrowingBiConsumer<long, IList<DistributedOperation>, Exception> applier )
		 {
			  this._batch = new List<DistributedOperation>( maxBatchSize );
			  this._maxBatchSize = maxBatchSize;
			  this._applier = applier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(long index, org.neo4j.causalclustering.core.replication.DistributedOperation operation) throws Exception
		 internal virtual void Add( long index, DistributedOperation operation )
		 {
			  Debug.Assert( _batch.Count <= 0 || index == ( _lastIndex + 1 ) );

			  _batch.Add( operation );
			  _lastIndex = index;

			  if ( _batch.Count == _maxBatchSize )
			  {
					Flush();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flush() throws Exception
		 internal virtual void Flush()
		 {
			  _applier.accept( _lastIndex, _batch );
			  _batch.Clear();
		 }
	}

}