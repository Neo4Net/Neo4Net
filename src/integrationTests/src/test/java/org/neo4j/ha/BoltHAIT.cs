using System;

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
namespace Neo4Net.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using AuthTokens = Neo4Net.driver.v1.AuthTokens;
	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using SessionExpiredException = Neo4Net.driver.v1.exceptions.SessionExpiredException;
	using TransientException = Neo4Net.driver.v1.exceptions.TransientException;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.driver.v1.Values.parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.entireClusterSeesMemberAsNotAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterSeesMembers;

	public class BoltHAIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withBoltEnabled().withCluster(clusterOfSize(3));
		 public readonly ClusterRule ClusterRule = new ClusterRule().withBoltEnabled().withCluster(clusterOfSize(3));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueServingBoltRequestsBetweenInternalRestarts() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueServingBoltRequestsBetweenInternalRestarts()
		 {
			  // given
			  /*
			   * Interestingly, it is enough to simply start a slave and then direct sessions to it. The problem seems
			   * to arise immediately, since simply from startup to being into SLAVE at least one internal restart happens
			   * and that seems sufficient to break the bolt server.
			   * However, that would make the test really weird, so we'll start the cluster, make sure we can connect and
			   * then isolate the slave, make it shutdown internally, then have it rejoin and it will switch to slave.
			   * At the end of this process, it must still be possible to open and execute transactions against the instance.
			   */
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;

			  Driver driver = GraphDatabase.driver( cluster.GetBoltAddress( slave1 ), AuthTokens.basic( "Neo4Net", "Neo4Net" ) );

			  /*
			   * We'll use a bookmark to enforce use of kernel internals by the bolt server, to make sure that parts that are
			   * switched during an internal restart are actually refreshed. Technically, this is not necessary, since the
			   * bolt server makes such use for every request. But this puts a nice bow on top of it.
			   */
			  string lastBookmark = InExpirableSession(driver, Driver.session, s =>
			  {
				using ( Transaction tx = s.beginTransaction() )
				{
					 tx.run( "CREATE (person:Person {name: {name}, title: {title}})", parameters( "name", "Webber", "title", "Mr" ) );
					 tx.success();
				}
				return s.lastBookmark();
			  });

			  // when
			  ClusterManager.RepairKit slaveFailRK = cluster.Fail( slave1 );

			  cluster.Await( entireClusterSeesMemberAsNotAvailable( slave1 ) );
			  slaveFailRK.Repair();
			  cluster.Await( masterSeesMembers( 3 ) );

			  // then
			  int? count = InExpirableSession(driver, Driver.session, s =>
			  {
				Record record;
				using ( Transaction tx = s.beginTransaction( lastBookmark ) )
				{
					 record = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
					 tx.success();
				}
				return record.get( "count" ).asInt();
			  });
			  assertEquals( 1, count.Value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T> T inExpirableSession(org.Neo4Net.driver.v1.Driver driver, System.Func<org.Neo4Net.driver.v1.Driver,org.Neo4Net.driver.v1.Session> acquirer, System.Func<org.Neo4Net.driver.v1.Session,T> op) throws java.util.concurrent.TimeoutException
		 private static T InExpirableSession<T>( Driver driver, System.Func<Driver, Session> acquirer, System.Func<Session, T> op )
		 {
			  long endTime = DateTimeHelper.CurrentUnixTimeMillis() + 15_000;

			  do
			  {
					try
					{
							using ( Session session = acquirer( driver ) )
							{
							 return op( session );
							}
					}
					catch ( Exception e ) when ( e is TransientException || e is SessionExpiredException )
					{
						 // role might have changed; try again;
					}
			  } while ( DateTimeHelper.CurrentUnixTimeMillis() < endTime );

			  throw new TimeoutException( "Transaction did not succeed in time" );
		 }
	}

}