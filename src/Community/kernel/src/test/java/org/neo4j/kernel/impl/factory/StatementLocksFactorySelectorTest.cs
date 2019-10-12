using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.factory
{
	using Test = org.junit.Test;


	using Config = Neo4Net.Kernel.configuration.Config;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using Locks_Client = Neo4Net.Kernel.impl.locking.Locks_Client;
	using SimpleStatementLocks = Neo4Net.Kernel.impl.locking.SimpleStatementLocks;
	using SimpleStatementLocksFactory = Neo4Net.Kernel.impl.locking.SimpleStatementLocksFactory;
	using StatementLocks = Neo4Net.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.same;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class StatementLocksFactorySelectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loadSimpleStatementLocksFactoryWhenNoServices()
		 public virtual void LoadSimpleStatementLocksFactoryWhenNoServices()
		 {
			  Locks locks = mock( typeof( Locks ) );
			  Locks_Client locksClient = mock( typeof( Locks_Client ) );
			  when( locks.NewClient() ).thenReturn(locksClient);

			  StatementLocksFactorySelector loader = NewLoader( locks );

			  StatementLocksFactory factory = loader.Select();
			  StatementLocks statementLocks = factory.NewInstance();

			  assertThat( factory, instanceOf( typeof( SimpleStatementLocksFactory ) ) );
			  assertThat( statementLocks, instanceOf( typeof( SimpleStatementLocks ) ) );

			  assertSame( locksClient, statementLocks.Optimistic() );
			  assertSame( locksClient, statementLocks.Pessimistic() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loadSingleAvailableFactory()
		 public virtual void LoadSingleAvailableFactory()
		 {
			  Locks locks = mock( typeof( Locks ) );
			  StatementLocksFactory factory = mock( typeof( StatementLocksFactory ) );

			  StatementLocksFactorySelector loader = NewLoader( locks, factory );

			  StatementLocksFactory loadedFactory = loader.Select();

			  assertSame( factory, loadedFactory );
			  verify( factory ).initialize( same( locks ), any( typeof( Config ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwWhenMultipleFactoriesLoaded()
		 public virtual void ThrowWhenMultipleFactoriesLoaded()
		 {
			  TestStatementLocksFactorySelector loader = NewLoader( mock( typeof( Locks ) ), mock( typeof( StatementLocksFactory ) ), mock( typeof( StatementLocksFactory ) ), mock( typeof( StatementLocksFactory ) ) );

			  try
			  {
					loader.Select();
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }
		 }

		 private static TestStatementLocksFactorySelector NewLoader( Locks locks, params StatementLocksFactory[] factories )
		 {
			  return new TestStatementLocksFactorySelector( locks, Config.defaults(), NullLogService.Instance, factories );
		 }

		 private class TestStatementLocksFactorySelector : StatementLocksFactorySelector
		 {
			  internal readonly IList<StatementLocksFactory> Factories;

			  internal TestStatementLocksFactorySelector( Locks locks, Config config, LogService logService, params StatementLocksFactory[] factories ) : base( locks, config, logService )
			  {
					this.Factories = Arrays.asList( factories );
			  }

			  internal override IList<StatementLocksFactory> ServiceLoadFactories()
			  {
					return Factories;
			  }
		 }
	}

}