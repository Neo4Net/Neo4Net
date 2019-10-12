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
namespace Org.Neo4j.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Label = Org.Neo4j.Graphdb.Label;
	using ConstraintCreator = Org.Neo4j.Graphdb.schema.ConstraintCreator;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ConstraintsInHAIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingConstraintOnSlaveIsNotAllowed()
		 public virtual void CreatingConstraintOnSlaveIsNotAllowed()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  slave.BeginTx();
			  try
			  {
					ConstraintCreator constraintCreator = slave.Schema().constraintFor(Label.label("LabelName")).assertPropertyIsUnique("PropertyName");

					// when
					constraintCreator.Create();
					fail( "should have thrown exception" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertThat( e.Message, equalTo( "Modifying the database schema can only be done on the master server, " + "this server is a slave. Please issue schema modification commands directly to the master." ) );
			  }
		 }
	}

}