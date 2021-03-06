﻿using System;

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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Suite = org.junit.runners.Suite;


	using Org.Neo4j.Function;
	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Node = Org.Neo4j.Graphdb.Node;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using ConstraintViolationTransactionFailureException = Org.Neo4j.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Operations = Org.Neo4j.Kernel.Impl.Newapi.Operations;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Org.Neo4j.Test.rule.EnterpriseDatabaseRule;
	using ThreadingRule = Org.Neo4j.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Suite.SuiteClasses;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
	using static Org.Neo4j.Kernel.Impl.Api.integrationtest.PropertyExistenceConstraintVerificationIT.NodePropertyExistenceExistenceConstrainVerificationIT;
	using static Org.Neo4j.Kernel.Impl.Api.integrationtest.PropertyExistenceConstraintVerificationIT.RelationshipPropertyExistenceExistenceConstrainVerificationIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.ThreadingRule.waitingWhileIn;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @SuiteClasses({ NodePropertyExistenceExistenceConstrainVerificationIT.class, RelationshipPropertyExistenceExistenceConstrainVerificationIT.class }) public class PropertyExistenceConstraintVerificationIT
	public class PropertyExistenceConstraintVerificationIT
	{
		 private const int WAIT_TIMEOUT_SECONDS = 200;

		 public class NodePropertyExistenceExistenceConstrainVerificationIT : AbstractPropertyExistenceConstraintVerificationIT
		 {
			  internal override void CreateConstraint( DatabaseRule db, string label, string property )
			  {
					SchemaHelper.createNodePropertyExistenceConstraint( db, label, property );
			  }

			  internal override string ConstraintCreationMethodName()
			  {
					return "nodePropertyExistenceConstraintCreate";
			  }

			  internal override long CreateOffender( DatabaseRule db, string key )
			  {
					Node node = Db.createNode();
					node.AddLabel( label( key ) );
					return node.Id;
			  }

			  internal override string OffenderCreationMethodName()
			  {
					return "nodeAddLabel"; // takes schema read lock to enforce constraints
			  }

			  internal override Type Owner
			  {
				  get
				  {
						return typeof( Operations );
				  }
			  }

		 }

		 public class RelationshipPropertyExistenceExistenceConstrainVerificationIT : AbstractPropertyExistenceConstraintVerificationIT
		 {
			  public override void CreateConstraint( DatabaseRule db, string relType, string property )
			  {
					SchemaHelper.createRelPropertyExistenceConstraint( db, relType, property );
			  }

			  public override string ConstraintCreationMethodName()
			  {
					return "relationshipPropertyExistenceConstraintCreate";
			  }

			  public override long CreateOffender( DatabaseRule db, string key )
			  {
					Node start = Db.createNode();
					Node end = Db.createNode();
					Relationship relationship = start.CreateRelationshipTo( end, withName( key ) );
					return relationship.Id;
			  }

			  public override string OffenderCreationMethodName()
			  {
					return "relationshipCreate"; // takes schema read lock to enforce constraints
			  }

			  internal override Type Owner
			  {
				  get
				  {
						return typeof( Operations );
				  }
			  }

		 }

		 public abstract class AbstractPropertyExistenceConstraintVerificationIT
		 {
			  internal const string KEY = "Foo";
			  internal const string PROPERTY = "bar";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule();
			  public readonly DatabaseRule Db = new EnterpriseDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule thread = new org.neo4j.test.rule.concurrent.ThreadingRule();
			  public readonly ThreadingRule Thread = new ThreadingRule();

			  internal abstract void CreateConstraint( DatabaseRule db, string key, string property );

			  internal abstract string ConstraintCreationMethodName();

			  internal abstract long CreateOffender( DatabaseRule db, string key );

			  internal abstract string OffenderCreationMethodName();
			  internal abstract Type Owner { get; }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateConstraintIfSomeNodeLacksTheMandatoryProperty()
			  public virtual void ShouldFailToCreateConstraintIfSomeNodeLacksTheMandatoryProperty()
			  {
					// given
					using ( Transaction tx = Db.beginTx() )
					{
						 CreateOffender( Db, KEY );
						 tx.Success();
					}

					// when
					try
					{
						 using ( Transaction tx = Db.beginTx() )
						 {
							  CreateConstraint( Db, KEY, PROPERTY );
							  tx.Success();
						 }
						 fail( "expected exception" );
					}
					// then
					catch ( QueryExecutionException e )
					{
						 assertThat( e.InnerException.Message, startsWith( "Unable to create CONSTRAINT" ) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateConstraintIfConcurrentlyCreatedEntityLacksTheMandatoryProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldFailToCreateConstraintIfConcurrentlyCreatedEntityLacksTheMandatoryProperty()
			  {
					// when
					try
					{
						 Future<Void> nodeCreation;
						 using ( Transaction tx = Db.beginTx() )
						 {
							  CreateConstraint( Db, KEY, PROPERTY );

							  nodeCreation = Thread.executeAndAwait( CreateOffender(), null, waitingWhileIn(Owner, OffenderCreationMethodName()), WAIT_TIMEOUT_SECONDS, SECONDS );

							  tx.Success();
						 }
						 nodeCreation.get();
						 fail( "expected exception" );
					}
					// then, we either fail to create the constraint,
					catch ( ConstraintViolationException e )
					{
						 assertThat( e.InnerException, instanceOf( typeof( CreateConstraintFailureException ) ) );
					}
					// or we fail to create the offending node
					catch ( ExecutionException e )
					{
						 assertThat( e.InnerException, instanceOf( typeof( ConstraintViolationException ) ) );
						 assertThat( e.InnerException.InnerException, instanceOf( typeof( ConstraintViolationTransactionFailureException ) ) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateConstraintIfConcurrentlyCommittedEntityLacksTheMandatoryProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldFailToCreateConstraintIfConcurrentlyCommittedEntityLacksTheMandatoryProperty()
			  {
					// when
					try
					{
						 Future<Void> constraintCreation;
						 using ( Transaction tx = Db.beginTx() )
						 {
							  CreateOffender( Db, KEY );

							  constraintCreation = Thread.executeAndAwait( CreateConstraint(), null, waitingWhileIn(typeof(Operations), ConstraintCreationMethodName()), WAIT_TIMEOUT_SECONDS, SECONDS );

							  tx.Success();
						 }
						 constraintCreation.get();
						 fail( "expected exception" );
					}
					// then, we either fail to create the constraint,
					catch ( ExecutionException e )
					{
						 assertThat( e.InnerException, instanceOf( typeof( QueryExecutionException ) ) );
						 assertThat( e.InnerException.Message, startsWith( "Unable to create CONSTRAINT" ) );
					}
					// or we fail to create the offending node
					catch ( ConstraintViolationException e )
					{
						 assertThat( e.InnerException, instanceOf( typeof( ConstraintViolationTransactionFailureException ) ) );
					}
			  }

			  internal virtual ThrowingFunction<Void, Void, Exception> CreateOffender()
			  {
					return aVoid =>
					{
					 using ( Transaction tx = Db.beginTx() )
					 {
						  CreateOffender( Db, KEY );
						  tx.success();
					 }
					 return null;
					};
			  }

			  internal virtual ThrowingFunction<Void, Void, Exception> CreateConstraint()
			  {
					return aVoid =>
					{
					 using ( Transaction tx = Db.beginTx() )
					 {
						  CreateConstraint( Db, KEY, PROPERTY );
						  tx.success();
					 }
					 return null;
					};
			  }
		 }
	}

}