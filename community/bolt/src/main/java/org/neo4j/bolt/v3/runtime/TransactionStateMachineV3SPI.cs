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
namespace Org.Neo4j.Bolt.v3.runtime
{

	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltResultHandle = Org.Neo4j.Bolt.runtime.BoltResultHandle;
	using TransactionStateMachineV1SPI = Org.Neo4j.Bolt.v1.runtime.TransactionStateMachineV1SPI;
	using QueryResultProvider = Org.Neo4j.Cypher.@internal.javacompat.QueryResultProvider;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	public class TransactionStateMachineV3SPI : TransactionStateMachineV1SPI
	{
		 public TransactionStateMachineV3SPI( GraphDatabaseAPI db, BoltChannel boltChannel, Duration txAwaitDuration, Clock clock ) : base( db, boltChannel, txAwaitDuration, clock )
		 {
		 }

		 protected internal override BoltResultHandle NewBoltResultHandle( string statement, MapValue @params, TransactionalContext transactionalContext )
		 {
			  return new BoltResultHandleV3( this, statement, @params, transactionalContext );
		 }

		 private class BoltResultHandleV3 : BoltResultHandleV1
		 {
			 private readonly TransactionStateMachineV3SPI _outerInstance;

			  internal BoltResultHandleV3( TransactionStateMachineV3SPI outerInstance, string statement, MapValue @params, TransactionalContext transactionalContext ) : base( outerInstance, statement, @params, transactionalContext )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override BoltResult NewBoltResult( QueryResultProvider result, Clock clock )
			  {
					return new CypherAdapterStreamV3( result.QueryResult(), clock );
			  }
		 }
	}

}