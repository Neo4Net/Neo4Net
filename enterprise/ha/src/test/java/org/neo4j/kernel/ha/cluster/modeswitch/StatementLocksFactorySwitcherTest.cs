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
namespace Org.Neo4j.Kernel.ha.cluster.modeswitch
{
	using Test = org.junit.Test;

	using Org.Neo4j.Kernel.ha;
	using SlaveStatementLocksFactory = Org.Neo4j.Kernel.ha.@lock.SlaveStatementLocksFactory;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class StatementLocksFactorySwitcherTest
	{

		 private StatementLocksFactory _configuredLockFactory = mock( typeof( StatementLocksFactory ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterStatementLocks()
		 public virtual void MasterStatementLocks()
		 {
			  StatementLocksFactorySwitcher switcher = LocksSwitcher;
			  StatementLocksFactory masterLocks = switcher.MasterImpl;
			  assertSame( masterLocks, _configuredLockFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveStatementLocks()
		 public virtual void SlaveStatementLocks()
		 {
			  StatementLocksFactorySwitcher switcher = LocksSwitcher;
			  StatementLocksFactory slaveLocks = switcher.SlaveImpl;
			  assertThat( slaveLocks, instanceOf( typeof( SlaveStatementLocksFactory ) ) );
		 }

		 private StatementLocksFactorySwitcher LocksSwitcher
		 {
			 get
			 {
				  DelegateInvocationHandler invocationHandler = mock( typeof( DelegateInvocationHandler ) );
				  return new StatementLocksFactorySwitcher( invocationHandler, _configuredLockFactory );
			 }
		 }
	}

}