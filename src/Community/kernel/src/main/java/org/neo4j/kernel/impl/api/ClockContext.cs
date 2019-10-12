using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using Clocks = Neo4Net.Time.Clocks;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	public sealed class ClockContext
	{
		 private readonly SystemNanoClock _system;
		 private Clock _statement;
		 private Clock _transaction;

		 public ClockContext() : this(Clocks.nanoClock())
		 {
		 }

		 public ClockContext( SystemNanoClock clock )
		 {
			  this._system = Objects.requireNonNull( clock, "system clock" );
		 }

		 internal void InitializeTransaction()
		 {
			  this._transaction = Clock.@fixed( _system.instant(), Timezone() );
			  this._statement = null;
		 }

		 internal void InitializeStatement()
		 {
			  if ( this._statement == null ) // this is the first statement in the transaction, use the transaction time
			  {
					this._statement = this._transaction;
			  }
			  else // this is not the first statement in the transaction, initialize with a new time
			  {
					this._statement = Clock.@fixed( _system.instant(), Timezone() );
			  }
		 }

		 public ZoneId Timezone()
		 {
			  return _system.Zone;
		 }

		 public SystemNanoClock SystemClock()
		 {
			  return _system;
		 }

		 public Clock StatementClock()
		 {
			  Debug.Assert( _statement != null, "statement clock not initialized" );
			  return _statement;
		 }

		 public Clock TransactionClock()
		 {
			  Debug.Assert( _transaction != null, "transaction clock not initialized" );
			  return _transaction;
		 }
	}

}