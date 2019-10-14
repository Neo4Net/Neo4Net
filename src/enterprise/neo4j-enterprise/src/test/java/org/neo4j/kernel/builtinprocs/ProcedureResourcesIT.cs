using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.builtinprocs
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ProcedureResourcesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule().withSetting(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE);
		 public DatabaseRule Db = new EnterpriseDatabaseRule().withSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

		 private readonly string _indexDefinition = ":Label(prop)";
		 private readonly string _explicitIndexName = "explicitIndex";
		 private readonly string _relExplicitIndexName = "relExplicitIndex";
		 private readonly string _ftsNodesIndex = "'ftsNodes'";
		 private readonly string _ftsRelsIndex = "'ftsRels'";
		 private readonly ExecutorService _executor = Executors.newSingleThreadExecutor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _executor.shutdown();
			  _executor.awaitTermination( 5, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allProcedures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllProcedures()
		 {
			  // when
			  CreateIndex();
			  CreateExplicitIndex();
			  CreateFulltextIndexes();
			  foreach ( ProcedureSignature procedure in Db.DependencyResolver.resolveDependency( typeof( Procedures ) ).AllProcedures )
			  {
					// then
					InitialData();
					ProcedureData procedureData = null;
					try
					{
						 procedureData = ProcedureDataFor( procedure );
						 VerifyProcedureCloseAllAcquiredKernelStatements( procedureData );
					}
					catch ( Exception e )
					{
						 throw new Exception( "Failed on procedure: \"" + procedureData + "\"", e );
					}
					ClearDb();
			  }
		 }

		 private void InitialData()
		 {
			  Label unusedLabel = Label.label( "unusedLabel" );
			  RelationshipType unusedRelType = RelationshipType.withName( "unusedRelType" );
			  string unusedPropKey = "unusedPropKey";
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node1 = Db.createNode( unusedLabel );
					node1.SetProperty( unusedPropKey, "value" );
					Node node2 = Db.createNode( unusedLabel );
					node2.SetProperty( unusedPropKey, 1 );
					node1.CreateRelationshipTo( node2, unusedRelType );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyProcedureCloseAllAcquiredKernelStatements(ProcedureData proc) throws java.util.concurrent.ExecutionException, InterruptedException
		 private void VerifyProcedureCloseAllAcquiredKernelStatements( ProcedureData proc )
		 {
			  if ( proc.Skip )
			  {
					return;
			  }
			  string failureMessage = "Failed on procedure " + proc.Name;
			  using ( Transaction outer = Db.beginTx() )
			  {
					string procedureQuery = proc.BuildProcedureQuery();
					Exhaust( Db.execute( procedureQuery ) ).close();
					Exhaust( Db.execute( "MATCH (mo:Label) WHERE mo.prop = 'n/a' RETURN mo" ) ).close();
					ExecuteInOtherThread( "CREATE(mo:Label) SET mo.prop = 'val' RETURN mo" );
					Result result = Db.execute( "MATCH (mo:Label) WHERE mo.prop = 'val' RETURN mo" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( failureMessage, result.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> next = result.Next();
					assertNotNull( failureMessage, next["mo"] );
					Exhaust( result );
					result.Close();
					outer.Success();
			  }
		 }

		 private Result Exhaust( Result execute )
		 {
			  while ( execute.MoveNext() )
			  {
					execute.Current;
			  }
			  return execute;
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( "CREATE INDEX ON " + _indexDefinition );
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(5, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

		 private void CreateFulltextIndexes()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( "call db.index.fulltext.createNodeIndex(" + _ftsNodesIndex + ", ['Label'], ['prop'])" ).close();
					Db.execute( "call db.index.fulltext.createRelationshipIndex(" + _ftsRelsIndex + ", ['Label'], ['prop'])" ).close();
					tx.Success();
			  }
		 }

		 private void CreateExplicitIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.index().forNodes(_explicitIndexName);
					Db.index().forRelationships(_relExplicitIndexName);
					tx.Success();
			  }
		 }

		 private void ClearDb()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( "MATCH (n) DETACH DELETE n" ).close();
					tx.Success();
			  }
		 }

		 private class ProcedureData
		 {
			  internal readonly string Name;
			  internal readonly IList<object> Params = new List<object>();
			  internal string SetupQuery;
			  internal string PostQuery;
			  internal bool Skip;

			  internal ProcedureData( ProcedureSignature procedure )
			  {
					this.Name = procedure.Name().ToString();
			  }

			  internal virtual void WithParam( object param )
			  {
					this.Params.Add( param );
			  }

			  internal virtual void WithSetup( string setupQuery, string postQuery )
			  {
					this.SetupQuery = setupQuery;
					this.PostQuery = postQuery;
			  }

			  internal virtual string BuildProcedureQuery()
			  {
					StringJoiner stringJoiner = new StringJoiner( ",", "CALL " + Name + "(", ")" );
					foreach ( object parameter in Params )
					{
						 stringJoiner.add( parameter.ToString() );
					}
					if ( !string.ReferenceEquals( SetupQuery, null ) && !string.ReferenceEquals( PostQuery, null ) )
					{
						 return SetupQuery + " " + stringJoiner.ToString() + " " + PostQuery;
					}
					else
					{
						 return stringJoiner.ToString();
					}
			  }

			  public override string ToString()
			  {
					return BuildProcedureQuery();
			  }
		 }

		 private ProcedureData ProcedureDataFor( ProcedureSignature procedure )
		 {
			  ProcedureData proc = new ProcedureData( procedure );
			  switch ( proc.Name )
			  {
			  case "db.createProperty":
					proc.WithParam( "'propKey'" );
					break;
			  case "db.resampleIndex":
					proc.WithParam( "'" + _indexDefinition + "'" );
					break;
			  case "db.createRelationshipType":
					proc.WithParam( "'RelType'" );
					break;
			  case "dbms.queryJmx":
					proc.WithParam( "'*:*'" );
					break;
			  case "db.awaitIndex":
					proc.WithParam( "'" + _indexDefinition + "'" );
					proc.WithParam( 100 );
					break;
			  case "db.createLabel":
					proc.WithParam( "'OtherLabel'" );
					break;
			  case "dbms.killQuery":
					proc.WithParam( "'query-1234'" );
					break;
			  case "dbms.killQueries":
					proc.WithParam( "['query-1234']" );
					break;
			  case "dbms.killConnection":
					proc.WithParam( "'bolt-1234'" );
					break;
			  case "dbms.killConnections":
					proc.WithParam( "['bolt-1234']" );
					break;
			  case "dbms.setTXMetaData":
					proc.WithParam( "{realUser:'MyMan'}" );
					break;
			  case "dbms.listActiveLocks":
					proc.WithParam( "'query-1234'" );
					break;
			  case "db.index.explicit.seekNodes":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "'noKey'" );
					proc.WithParam( "'noValue'" );
					break;
			  case "db.index.explicit.searchNodes":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "'noKey:foo*'" );
					break;
			  case "db.index.explicit.searchRelationships":
					proc.WithParam( "'" + _relExplicitIndexName + "'" );
					proc.WithParam( "'noKey:foo*'" );
					break;
			  case "db.index.explicit.searchRelationshipsIn":
					proc.WithParam( "'" + _relExplicitIndexName + "'" );

					proc.WithParam( "n" );
					proc.WithParam( "'noKey:foo*'" );
					proc.WithSetup( "OPTIONAL MATCH (n) WITH n LIMIT 1", "YIELD relationship AS r RETURN r" );
					break;
			  case "db.index.explicit.searchRelationshipsOut":
					proc.WithParam( "'" + _relExplicitIndexName + "'" );
					proc.WithParam( "n" );
					proc.WithParam( "'noKey:foo*'" );
					proc.WithSetup( "OPTIONAL MATCH (n) WITH n LIMIT 1", "YIELD relationship AS r RETURN r" );
					break;
			  case "db.index.explicit.searchRelationshipsBetween":
					proc.WithParam( "'" + _relExplicitIndexName + "'" );
					proc.WithParam( "n" );
					proc.WithParam( "n" );
					proc.WithParam( "'noKey:foo*'" );
					proc.WithSetup( "OPTIONAL MATCH (n) WITH n LIMIT 1", "YIELD relationship AS r RETURN r" );
					break;
			  case "db.index.explicit.seekRelationships":
					proc.WithParam( "'" + _relExplicitIndexName + "'" );
					proc.WithParam( "'noKey'" );
					proc.WithParam( "'noValue'" );
					break;
			  case "db.index.explicit.auto.seekNodes":
					proc.WithParam( "'noKey'" );
					proc.WithParam( "'noValue'" );
					break;
			  case "db.index.explicit.auto.searchNodes":
					proc.WithParam( "'noKey:foo*'" );
					break;
			  case "db.index.explicit.auto.searchRelationships":
					proc.WithParam( "'noKey:foo*'" );
					break;
			  case "db.index.explicit.auto.seekRelationships":
					proc.WithParam( "'noKey'" );
					proc.WithParam( "'noValue'" );
					break;
			  case "db.index.explicit.existsForNodes":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					break;
			  case "db.index.explicit.existsForRelationships":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					break;
			  case "db.index.explicit.forNodes":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					break;
			  case "db.index.explicit.forRelationships":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					break;
			  case "db.index.explicit.addNode":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "n" );
					proc.WithParam( "'prop'" );
					proc.WithParam( "'value'" );
					proc.WithSetup( "OPTIONAL MATCH (n) WITH n LIMIT 1", "YIELD success RETURN success" );
					break;
			  case "db.index.explicit.addRelationship":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "r" );
					proc.WithParam( "'prop'" );
					proc.WithParam( "'value'" );
					proc.WithSetup( "OPTIONAL MATCH ()-[r]->() WITH r LIMIT 1", "YIELD success RETURN success" );
					break;
			  case "db.index.explicit.removeNode":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "n" );
					proc.WithParam( "'prop'" );
					proc.WithSetup( "OPTIONAL MATCH (n) WITH n LIMIT 1", "YIELD success RETURN success" );
					break;
			  case "db.index.explicit.removeRelationship":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					proc.WithParam( "r" );
					proc.WithParam( "'prop'" );
					proc.WithSetup( "OPTIONAL MATCH ()-[r]->() WITH r LIMIT 1", "YIELD success RETURN success" );
					break;
			  case "db.index.explicit.drop":
					proc.WithParam( "'" + _explicitIndexName + "'" );
					break;
			  case "dbms.setConfigValue":
					proc.WithParam( "'dbms.logs.query.enabled'" );
					proc.WithParam( "'false'" );
					break;
			  case "db.createIndex":
					proc.WithParam( "':Person(name)'" );
					proc.WithParam( "'lucene+native-2.0'" );
					break;
			  case "db.createNodeKey":
					// Grabs schema lock an so can not execute concurrently with node creation
					proc.Skip = true;
					break;
			  case "db.createUniquePropertyConstraint":
					// Grabs schema lock an so can not execute concurrently with node creation
					proc.Skip = true;
					break;

			  case "db.index.fulltext.awaitIndex":
					proc.WithParam( _ftsNodesIndex );
					proc.WithParam( 100 );
					break;

			  case "db.index.fulltext.queryNodes":
					proc.WithParam( _ftsNodesIndex );
					proc.WithParam( "'value'" );
					break;
			  case "db.index.fulltext.queryRelationships":
					proc.WithParam( _ftsRelsIndex );
					proc.WithParam( "'value'" );
					break;
			  case "db.index.fulltext.drop":
					proc.WithParam( _ftsRelsIndex );
					break;
			  case "db.stats.retrieveAllAnonymized":
					proc.WithParam( "'myToken'" );
					break;
			  case "db.stats.retrieve":
					proc.WithParam( "'GRAPH COUNTS'" );
					break;

			  case "db.stats.stop":
					proc.WithParam( "'QUERIES'" );
					break;

			  case "db.stats.clear":
					proc.WithParam( "'QUERIES'" );
					break;

			  case "db.stats.collect":
					proc.WithParam( "'QUERIES'" );
					break;

			  case "db.index.fulltext.createRelationshipIndex":
					// Grabs schema lock an so can not execute concurrently with node creation
					proc.Skip = true;
					break;
			  case "db.index.fulltext.createNodeIndex":
					// Grabs schema lock an so can not execute concurrently with node creation
					proc.Skip = true;
					break;
			  default:
		  break;
			  }
			  return proc;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void executeInOtherThread(String query) throws java.util.concurrent.ExecutionException, InterruptedException
		 private void ExecuteInOtherThread( string query )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(() ->
			  Future<object> future = _executor.submit(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Exhaust( Db.execute( query ) );
					 tx.success();
				}
			  });
			  future.get();
		 }

	}

}