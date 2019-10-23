using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;


	using PrimitiveLongCollections = Neo4Net.Collections.primitive.PrimitiveLongCollections;
	using PrimitiveLongSet = Neo4Net.Collections.primitive.PrimitiveLongSet;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.config;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using KernelImpl = Neo4Net.Kernel.Impl.Api.KernelImpl;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using TopLevelTransaction = Neo4Net.Kernel.impl.coreapi.TopLevelTransaction;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class LuceneFulltextTestSupport
	{
		private bool InstanceFieldsInitialized = false;

		public LuceneFulltextTestSupport()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_repeatRule = CreateRepeatRule();
			Rules = RuleChain.outerRule( _repeatRule ).around( Db );
		}

		 internal static readonly Label Label = Label.label( "LABEL" );
		 internal static readonly RelationshipType Reltype = RelationshipType.withName( "type" );
		 internal const string PROP = "prop";

		 internal DatabaseRule Db = new EmbeddedDatabaseRule();
		 private RepeatRule _repeatRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(repeatRule).around(db);
		 public RuleChain Rules;

		 internal Properties Settings;
		 internal FulltextAdapter FulltextAdapter;

		 protected internal virtual RepeatRule CreateRepeatRule()
		 {
			  return new RepeatRule( false, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  Settings = new Properties();
			  FulltextAdapter = Accessor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void applySetting(org.Neo4Net.graphdb.config.Setting<String> setting, String value) throws java.io.IOException
		 internal virtual void ApplySetting( Setting<string> setting, string value )
		 {
			  Db.restartDatabase( setting.Name(), value );
			  Db.ensureStarted();
			  FulltextAdapter = Accessor;
		 }

		 internal virtual KernelTransactionImplementation KernelTransaction
		 {
			 get
			 {
				  try
				  {
						return ( KernelTransactionImplementation ) Db.resolveDependency( typeof( KernelImpl ) ).BeginTransaction( Neo4Net.Kernel.Api.Internal.Transaction_Type.Explicit, LoginContext.AUTH_DISABLED );
				  }
				  catch ( TransactionFailureException )
				  {
						throw new Exception( "oops" );
				  }
			 }
		 }

		 private FulltextAdapter Accessor
		 {
			 get
			 {
				  return ( FulltextAdapter ) Db.resolveDependency( typeof( IndexProviderMap ) ).lookup( FulltextIndexProviderFactory.Descriptor );
			 }
		 }

		 internal virtual long CreateNodeIndexableByPropertyValue( Label label, object propertyValue )
		 {
			  return CreateNodeWithProperty( label, PROP, propertyValue );
		 }

		 internal virtual long CreateNodeWithProperty( Label label, string propertyKey, object propertyValue )
		 {
			  Node node = Db.createNode( label );
			  node.SetProperty( propertyKey, propertyValue );
			  return node.Id;
		 }

		 internal virtual long CreateRelationshipIndexableByPropertyValue( long firstNodeId, long secondNodeId, object propertyValue )
		 {
			  return CreateRelationshipWithProperty( firstNodeId, secondNodeId, PROP, propertyValue );
		 }

		 internal virtual long CreateRelationshipWithProperty( long firstNodeId, long secondNodeId, string propertyKey, object propertyValue )
		 {
			  Node first = Db.getNodeById( firstNodeId );
			  Node second = Db.getNodeById( secondNodeId );
			  Relationship relationship = first.CreateRelationshipTo( second, Reltype );
			  relationship.SetProperty( propertyKey, propertyValue );
			  return relationship.Id;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.Neo4Net.kernel.api.KernelTransaction kernelTransaction(org.Neo4Net.graphdb.Transaction tx) throws Exception
		 public static KernelTransaction KernelTransaction( Transaction tx )
		 {
			  assertThat( tx, instanceOf( typeof( TopLevelTransaction ) ) );
			  System.Reflection.FieldInfo transactionField = typeof( TopLevelTransaction ).getDeclaredField( "transaction" );
			  transactionField.Accessible = true;
			  return ( KernelTransaction ) transactionField.get( tx );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertQueryFindsNothing(org.Neo4Net.kernel.api.KernelTransaction ktx, String indexName, String query) throws Exception
		 internal virtual void AssertQueryFindsNothing( KernelTransaction ktx, string indexName, string query )
		 {
			  AssertQueryFindsIds( ktx, indexName, query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertQueryFindsIds(org.Neo4Net.kernel.api.KernelTransaction ktx, String indexName, String query, long... ids) throws Exception
		 internal virtual void AssertQueryFindsIds( KernelTransaction ktx, string indexName, string query, params long[] ids )
		 {
			  ScoreEntityIterator result = FulltextAdapter.query( ktx, indexName, query );
			  AssertQueryResultsMatch( result, ids );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertQueryFindsIdsInOrder(org.Neo4Net.kernel.api.KernelTransaction ktx, String indexName, String query, long... ids) throws java.io.IOException, org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, org.apache.lucene.queryparser.classic.ParseException
		 internal virtual void AssertQueryFindsIdsInOrder( KernelTransaction ktx, string indexName, string query, params long[] ids )
		 {
			  ScoreEntityIterator result = FulltextAdapter.query( ktx, indexName, query );
			  AssertQueryResultsMatchInOrder( result, ids );
		 }

		 private static void AssertQueryResultsMatch( ScoreEntityIterator result, long[] ids )
		 {
			  PrimitiveLongSet set = PrimitiveLongCollections.setOf( ids );
			  while ( result.MoveNext() )
			  {
					long next = result.Current.entityId();
					assertTrue( format( "Result returned node id %d, expected one of %s", next, Arrays.ToString( ids ) ), set.Remove( next ) );
			  }
			  if ( !set.Empty )
			  {
					IList<long> list = new List<long>();
					set.VisitKeys( k => !list.Add( k ) );
					fail( "Number of results differ from expected. " + set.Size() + " IDs were not found in the result: " + list );
			  }
		 }

		 private static void AssertQueryResultsMatchInOrder( ScoreEntityIterator result, long[] ids )
		 {
			  int num = 0;
			  float score = float.MaxValue;
			  while ( result.MoveNext() )
			  {
					ScoreEntityIterator.ScoreEntry scoredResult = result.Current;
					long nextId = scoredResult.EntityId();
					float nextScore = scoredResult.Score();
					assertThat( nextScore, lessThanOrEqualTo( score ) );
					score = nextScore;
					assertEquals( format( "Result returned node id %d, expected %d", nextId, ids[num] ), ids[num], nextId );
					num++;
			  }
			  assertEquals( "Number of results differ from expected", ids.Length, num );
		 }

		 internal virtual void SetNodeProp( long nodeId, string value )
		 {
			  SetNodeProp( nodeId, PROP, value );
		 }

		 internal virtual void SetNodeProp( long nodeId, string propertyKey, string value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( nodeId );
					node.SetProperty( propertyKey, value );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void await(org.Neo4Net.Kernel.Api.Internal.IndexReference descriptor) throws Exception
		 internal virtual void Await( IndexReference descriptor )
		 {
			  try
			  {
					  using ( KernelTransactionImplementation tx = KernelTransaction )
					  {
						while ( tx.SchemaRead().index(descriptor.Schema()) == IndexReference.NO_INDEX )
						{
							 Thread.Sleep( 100 );
						}
						while ( tx.SchemaRead().indexGetState(descriptor) != InternalIndexState.ONLINE )
						{
							 Thread.Sleep( 100 );
						}
					  }
			  }
			  catch ( InterruptedException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
		 }
	}

}