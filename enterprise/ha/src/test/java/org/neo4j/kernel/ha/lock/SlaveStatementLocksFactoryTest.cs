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
namespace Org.Neo4j.Kernel.ha.@lock
{
	using Test = org.junit.Test;

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocks = Org.Neo4j.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class SlaveStatementLocksFactoryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createSlaveStatementLocks()
		 public virtual void CreateSlaveStatementLocks()
		 {
			  StatementLocksFactory @delegate = mock( typeof( StatementLocksFactory ) );
			  Locks locks = mock( typeof( Locks ) );
			  Config config = Config.defaults();

			  SlaveStatementLocksFactory slaveStatementLocksFactory = new SlaveStatementLocksFactory( @delegate );
			  slaveStatementLocksFactory.Initialize( locks, config );
			  StatementLocks statementLocks = slaveStatementLocksFactory.NewInstance();

			  assertThat( statementLocks, instanceOf( typeof( SlaveStatementLocks ) ) );
			  verify( @delegate ).initialize( locks, config );
			  verify( @delegate ).newInstance();
		 }
	}

}