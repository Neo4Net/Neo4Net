using System.Collections.Generic;

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
namespace Org.Neo4j.management
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using JmxKernelExtension = Org.Neo4j.Jmx.impl.JmxKernelExtension;
	using LockInfo = Org.Neo4j.Kernel.info.LockInfo;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;

	public class TestLockManagerBean
	{
		 private LockManager _lockManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
		 private GraphDatabaseAPI _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _graphDb = DbRule.GraphDatabaseAPI;
			  _lockManager = _graphDb.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( LockManager ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void restingGraphHoldsNoLocks()
		 public virtual void RestingGraphHoldsNoLocks()
		 {
			  assertEquals( "unexpected lock count", 0, _lockManager.Locks.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void modifiedNodeImpliesLock()
		 public virtual void ModifiedNodeImpliesLock()
		 {
			  Node node = CreateNode();

			  using ( Transaction ignore = _graphDb.beginTx() )
			  {
					node.SetProperty( "key", "value" );

					IList<LockInfo> locks = _lockManager.Locks;
					assertEquals( "unexpected lock count", 1, locks.Count );
					LockInfo @lock = locks[0];
					assertNotNull( "null lock", @lock );

			  }
			  IList<LockInfo> locks = _lockManager.Locks;
			  assertEquals( "unexpected lock count", 0, locks.Count );
		 }

		 private Node CreateNode()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					Node node = _graphDb.createNode();
					tx.Success();
					return node;
			  }
		 }

	}

}