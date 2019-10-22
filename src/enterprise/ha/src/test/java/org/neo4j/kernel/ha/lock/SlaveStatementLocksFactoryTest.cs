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
	using Test = org.junit.Test;

	using Config = Neo4Net.Kernel.configuration.Config;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;

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