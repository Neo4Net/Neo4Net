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
namespace Neo4Net.Server.database
{
	using Test = org.junit.Test;

	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LifecycleManagingDatabaseTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustIgnoreExceptionsFromPreLoadingCypherQuery()
		 public virtual void MustIgnoreExceptionsFromPreLoadingCypherQuery()
		 {
			  // Given a lifecycled database that'll try to warm up Cypher when it starts
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.factory.GraphDatabaseFacade mockDb = mock(org.neo4j.kernel.impl.factory.GraphDatabaseFacade.class);
			  GraphDatabaseFacade mockDb = mock( typeof( GraphDatabaseFacade ) );
			  Config config = Config.defaults();
			  GraphDatabaseFacadeFactory.Dependencies deps = GraphDatabaseDependencies.newDependencies().userLogProvider(NullLogProvider.Instance);
			  GraphFactory factory = new SimpleGraphFactory( mockDb );
			  LifecycleManagingDatabase db = new LifecycleManagingDatabaseAnonymousInnerClass( this, config, factory, deps );

			  // When the execution of the query fails (for instance when this is a slave that just joined a cluster and is
			  // working on catching up to the master)
			  when( mockDb.Execute( LifecycleManagingDatabase.CYPHER_WARMUP_QUERY ) ).thenThrow( new TransactionFailureException( "Boo" ) );

			  // Then the database should still start up as normal, without bubbling the exception up
			  Db.init();
			  Db.start();
			  assertTrue( "the database should be running", Db.Running );
			  Db.stop();
			  Db.shutdown();
		 }

		 private class LifecycleManagingDatabaseAnonymousInnerClass : LifecycleManagingDatabase
		 {
			 private readonly LifecycleManagingDatabaseTest _outerInstance;

			 public LifecycleManagingDatabaseAnonymousInnerClass( LifecycleManagingDatabaseTest outerInstance, Config config, Neo4Net.Server.database.GraphFactory factory, GraphDatabaseFacadeFactory.Dependencies deps ) : base( config, factory, deps )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override bool InTestMode
			 {
				 get
				 {
					  return false;
				 }
			 }
		 }
	}

}