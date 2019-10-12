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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

	/// <summary>
	/// This test ensures that lazy properties
	/// </summary>
	public class TestReferenceDangling
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPropertyStoreReferencesOnRead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPropertyStoreReferencesOnRead()
		 {
			  // Given
			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;

			  // and Given the cache contains a LazyProperty
			  long nId = EnsurePropertyIsCachedLazyProperty( db, "some" );

			  // When
			  RestartNeoDataSource( db );

			  // Then reading the property is still possible
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( nId ).getProperty( "some" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPropertyStoreReferencesOnWrite() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPropertyStoreReferencesOnWrite()
		 {
			  // Given
			  GraphDatabaseAPI db = DbRule.GraphDatabaseAPI;

			  // and Given the cache contains a LazyProperty
			  long nId = EnsurePropertyIsCachedLazyProperty( db, "some" );

			  // When
			  RestartNeoDataSource( db );

			  // Then it should still be possible to manipulate properties on this node
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( nId ).setProperty( "some", new long[]{ -1, 2, 2, 3, 4, 5, 5 } );
					tx.Success();
			  }
		 }

		 private long EnsurePropertyIsCachedLazyProperty( GraphDatabaseAPI slave, string key )
		 {
			  long nId;
			  using ( Transaction tx = slave.BeginTx() )
			  {
					Node n = slave.CreateNode();
					nId = n.Id;
					n.SetProperty( key, new long[]{ -1, 2, 2, 3, 4, 5, 5 } );
					tx.Success();
			  }

			  using ( Transaction tx = slave.BeginTx() )
			  {
					slave.GetNodeById( nId ).hasProperty( key );
					tx.Success();
			  }
			  return nId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void restartNeoDataSource(org.neo4j.kernel.internal.GraphDatabaseAPI slave) throws Throwable
		 private void RestartNeoDataSource( GraphDatabaseAPI slave )
		 {
			  slave.DependencyResolver.resolveDependency( typeof( DataSourceManager ) ).DataSource.stop();
			  slave.DependencyResolver.resolveDependency( typeof( DataSourceManager ) ).DataSource.start();
		 }
	}

}