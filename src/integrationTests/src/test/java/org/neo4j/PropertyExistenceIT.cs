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
namespace Neo4Net
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	public class PropertyExistenceIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletedNodesNotCheckedByExistenceConstraints()
		 public virtual void DeletedNodesNotCheckedByExistenceConstraints()
		 {
			  IGraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.directory());
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