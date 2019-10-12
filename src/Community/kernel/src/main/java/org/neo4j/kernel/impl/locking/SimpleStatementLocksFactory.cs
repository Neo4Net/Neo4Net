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
namespace Neo4Net.Kernel.impl.locking
{
	using Config = Neo4Net.Kernel.configuration.Config;

	/// <summary>
	/// A <seealso cref="StatementLocksFactory"/> that creates <seealso cref="SimpleStatementLocks"/>.
	/// </summary>
	public class SimpleStatementLocksFactory : StatementLocksFactory
	{
		 private Locks _locks;

		 public SimpleStatementLocksFactory()
		 {
		 }

		 /// <summary>
		 /// Creates a new factory initialized with given {@code locks}.
		 /// </summary>
		 /// <param name="locks"> the locks to use. </param>
		 public SimpleStatementLocksFactory( Locks locks )
		 {
			  Initialize( locks, null );
		 }

		 public override void Initialize( Locks locks, Config config )
		 {
			  this._locks = requireNonNull( locks );
		 }

		 public override StatementLocks NewInstance()
		 {
			  if ( _locks == null )
			  {
					throw new System.InvalidOperationException( "Factory has not been initialized" );
			  }

			  return new SimpleStatementLocks( _locks.newClient() );
		 }
	}

}