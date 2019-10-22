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
namespace Neo4Net.Kernel.ha.@lock
{
	using Config = Neo4Net.Kernel.configuration.Config;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;

	/// <summary>
	/// Statement locks factory that will produce slave specific statement locks
	/// that are aware how to grab shared locks for labels and relationship types
	/// during slave commit
	/// </summary>
	public class SlaveStatementLocksFactory : StatementLocksFactory
	{
		 private readonly StatementLocksFactory @delegate;

		 public SlaveStatementLocksFactory( StatementLocksFactory @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override void Initialize( Locks locks, Config config )
		 {
			  @delegate.Initialize( locks, config );
		 }

		 public override StatementLocks NewInstance()
		 {
			  StatementLocks statementLocks = @delegate.NewInstance();
			  return new SlaveStatementLocks( statementLocks );
		 }
	}

}