using System;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Helpers.Collections;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class DataAndSchemaTransactionSeparationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static System.Func<org.neo4j.graphdb.GraphDatabaseService, Void> expectFailureAfterSchemaOperation(final System.Func<org.neo4j.graphdb.GraphDatabaseService, ?> function)
		 private static System.Func<GraphDatabaseService, Void> ExpectFailureAfterSchemaOperation<T1>( System.Func<T1> function )
		 {
			  return graphDb =>
			  {
				// given
				graphDb.schema().indexFor(label("Label1")).on("key1").create();

				// when
				try
				{
					 function( graphDb );

					 fail( "expected exception" );
				}
				// then
				catch ( Exception e )
				{
					 assertEquals( "Cannot perform data updates in a transaction that has performed schema updates.", e.Message );
				}
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static System.Func<org.neo4j.graphdb.GraphDatabaseService, Void> succeedAfterSchemaOperation(final System.Func<org.neo4j.graphdb.GraphDatabaseService, ?> function)
		 private static System.Func<GraphDatabaseService, Void> SucceedAfterSchemaOperation<T1>( System.Func<T1> function )
		 {
			  return graphDb =>
			  {
				// given
				graphDb.schema().indexFor(label("Label1")).on("key1").create();

				// when/then
				function( graphDb );
				return null;
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNodeCreationInSchemaTransaction()
		 public virtual void ShouldNotAllowNodeCreationInSchemaTransaction()
		 {
			  Db.executeAndRollback( ExpectFailureAfterSchemaOperation( CreateNode() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowRelationshipCreationInSchemaTransaction()
		 public virtual void ShouldNotAllowRelationshipCreationInSchemaTransaction()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.Pair<org.neo4j.graphdb.Node, org.neo4j.graphdb.Node> nodes = db.executeAndCommit(aPairOfNodes());
			  Pair<Node, Node> nodes = Db.executeAndCommit( APairOfNodes() );
			  // then
			  Db.executeAndRollback( ExpectFailureAfterSchemaOperation( Relate( nodes ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldNotAllowPropertyWritesInSchemaTransaction()
		 public virtual void ShouldNotAllowPropertyWritesInSchemaTransaction()
		 {
			  // given
			  Pair<Node, Node> nodes = Db.executeAndCommit( APairOfNodes() );
			  Relationship relationship = Db.executeAndCommit( Relate( nodes ) );
			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (System.Func<org.neo4j.graphdb.GraphDatabaseService, ?> operation : new System.Func[]{ propertyWrite(org.neo4j.graphdb.Node.class, nodes.first(), "key1", "value1"), propertyWrite(org.neo4j.graphdb.Relationship.class, relationship, "key1", "value1")})
			  foreach ( System.Func<GraphDatabaseService, ?> operation in new System.Func[]{ PropertyWrite( typeof( Node ), nodes.First(), "key1", "value1" ), PropertyWrite(typeof(Relationship), relationship, "key1", "value1") } )
			  {
					// then
					Db.executeAndRollback( ExpectFailureAfterSchemaOperation( operation ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldAllowPropertyReadsInSchemaTransaction()
		 public virtual void ShouldAllowPropertyReadsInSchemaTransaction()
		 {
			  // given
			  Pair<Node, Node> nodes = Db.executeAndCommit( APairOfNodes() );
			  Relationship relationship = Db.executeAndCommit( Relate( nodes ) );
			  Db.executeAndCommit( PropertyWrite( typeof( Node ), nodes.First(), "key1", "value1" ) );
			  Db.executeAndCommit( PropertyWrite( typeof( Relationship ), relationship, "key1", "value1" ) );

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (System.Func<org.neo4j.graphdb.GraphDatabaseService, ?> operation : new System.Func[]{ propertyRead(org.neo4j.graphdb.Node.class, nodes.first(), "key1"), propertyRead(org.neo4j.graphdb.Relationship.class, relationship, "key1")})
			  foreach ( System.Func<GraphDatabaseService, ?> operation in new System.Func[]{ PropertyRead( typeof( Node ), nodes.First(), "key1" ), PropertyRead(typeof(Relationship), relationship, "key1") } )
			  {
					// then
					Db.executeAndRollback( SucceedAfterSchemaOperation( operation ) );
			  }
		 }

		 private static System.Func<GraphDatabaseService, Node> CreateNode()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return GraphDatabaseService::createNode;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T extends org.neo4j.graphdb.PropertyContainer> System.Func<org.neo4j.graphdb.GraphDatabaseService, Object> propertyRead(Class<T> type, final T entity, final String key)
		 private static System.Func<GraphDatabaseService, object> PropertyRead<T>( Type type, T entity, string key ) where T : Neo4Net.Graphdb.PropertyContainer
		 {
				 type = typeof( T );
			  return new FailureRewriteAnonymousInnerClass( entity, key );
		 }

		 private class FailureRewriteAnonymousInnerClass : FailureRewrite<object>
		 {
			 private PropertyContainer _entity;
			 private string _key;

			 public FailureRewriteAnonymousInnerClass( PropertyContainer entity, string key ) : base( type.SimpleName + ".getProperty()" )
			 {
				 this._entity = entity;
				 this._key = key;
			 }

			 internal override object perform( GraphDatabaseService graphDb )
			 {
				  return _entity.getProperty( _key );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T extends org.neo4j.graphdb.PropertyContainer> System.Func<org.neo4j.graphdb.GraphDatabaseService, Void> propertyWrite(Class<T> type, final T entity, final String key, final Object value)
		 private static System.Func<GraphDatabaseService, Void> PropertyWrite<T>( Type type, T entity, string key, object value ) where T : Neo4Net.Graphdb.PropertyContainer
		 {
				 type = typeof( T );
			  return new FailureRewriteAnonymousInnerClass2( entity, key, value );
		 }

		 private class FailureRewriteAnonymousInnerClass2 : FailureRewrite<Void>
		 {
			 private PropertyContainer _entity;
			 private string _key;
			 private object _value;

			 public FailureRewriteAnonymousInnerClass2( PropertyContainer entity, string key, object value ) : base( type.SimpleName + ".setProperty()" )
			 {
				 this._entity = entity;
				 this._key = key;
				 this._value = value;
			 }

			 internal override Void perform( GraphDatabaseService graphDb )
			 {
				  _entity.setProperty( _key, _value );
				  return null;
			 }
		 }

		 private static System.Func<GraphDatabaseService, Pair<Node, Node>> APairOfNodes()
		 {
			  return graphDb => Pair.of( graphDb.createNode(), graphDb.createNode() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static System.Func<org.neo4j.graphdb.GraphDatabaseService, org.neo4j.graphdb.Relationship> relate(final org.neo4j.helpers.collection.Pair<org.neo4j.graphdb.Node, org.neo4j.graphdb.Node> nodes)
		 private static System.Func<GraphDatabaseService, Relationship> Relate( Pair<Node, Node> nodes )
		 {
			  return graphDb => nodes.First().createRelationshipTo(nodes.Other(), withName("RELATED"));
		 }

		 private abstract class FailureRewrite<T> : System.Func<GraphDatabaseService, T>
		 {
			  internal readonly string Message;

			  internal FailureRewrite( string message )
			  {
					this.Message = message;
			  }

			  public override T Apply( GraphDatabaseService graphDb )
			  {
					try
					{
						 return Perform( graphDb );
					}
					catch ( AssertionError e )
					{
						 AssertionError error = new AssertionError( Message + ": " + e.Message );
						 error.StackTrace = e.StackTrace;
						 throw error;
					}
			  }

			  internal abstract T Perform( GraphDatabaseService graphDb );
		 }
	}

}