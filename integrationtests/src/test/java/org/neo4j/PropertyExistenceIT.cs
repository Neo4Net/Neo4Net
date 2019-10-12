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
namespace Org.Neo4j
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

	public class PropertyExistenceIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletedNodesNotCheckedByExistenceConstraints()
		 public virtual void DeletedNodesNotCheckedByExistenceConstraints()
		 {
			  GraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.directory());
			  try
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 database.Execute( "CREATE CONSTRAINT ON (book:Book) ASSERT exists(book.isbn)" );
						 transaction.Success();
					}

					using ( Transaction transaction = database.BeginTx() )
					{
						 database.Execute( "CREATE (:label1 {name: \"Pelle\"})<-[:T1]-(:label2 {name: \"Elin\"})-[:T2]->(:label3)" );
						 transaction.Success();
					}

					using ( Transaction transaction = database.BeginTx() )
					{
						 database.Execute( "MATCH (n:label1 {name: \"Pelle\"})<-[r:T1]-(:label2 {name: \"Elin\"})-[:T2]->(:label3) DELETE r,n" );
						 transaction.Success();
					}
			  }
			  finally
			  {
					database.Shutdown();
			  }

		 }
	}

}