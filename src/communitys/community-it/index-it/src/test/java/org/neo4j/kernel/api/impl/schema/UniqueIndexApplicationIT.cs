using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.loop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.addLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.awaitIndexesOnline;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.createNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.index;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.setProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.DatabaseFunctions.uniquenessConstraint;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UniqueIndexApplicationIT
	public class UniqueIndexApplicationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final System.Func<org.neo4j.graphdb.GraphDatabaseService, ?> createIndex;
		 private readonly System.Func<GraphDatabaseService, ?> _createIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> indexTypes()
		 public static IList<object[]> IndexTypes()
		 {
			  return new IList<object[]> { CreateIndex( index( label( "Label1" ), "key1" ) ), CreateIndex( uniquenessConstraint( label( "Label1" ), "key1" ) ) };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void then()
		 public virtual void Then()
		 {
			  assertThat( "Matching nodes from index lookup", Db.when( Db.tx( ListNodeIdsFromIndexLookup( label( "Label1" ), "key1", "value1" ) ) ), HasSize( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void given()
		 public virtual void Given()
		 {
			  Db.executeAndCommit( _createIndex );
			  Db.executeAndCommit( awaitIndexesOnline( 5, SECONDS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_addLabel_setProperty()
		 public virtual void TxCreateNodeAddLabelSetProperty()
		 {
			  Db.when( Db.tx( createNode().andThen(addLabel(label("Label1")).andThen(setProperty("key1", "value1"))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_tx_addLabel_setProperty()
		 public virtual void TxCreateNodeTxAddLabelSetProperty()
		 {
			  Db.when( Db.tx( createNode() ).andThen(Db.tx(addLabel(label("Label1")).andThen(setProperty("key1", "value1")))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_addLabel_tx_setProperty()
		 public virtual void TxCreateNodeAddLabelTxSetProperty()
		 {
			  Db.when( Db.tx( createNode().andThen(addLabel(label("Label1"))) ).andThen(Db.tx(setProperty("key1", "value1"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_setProperty_tx_addLabel()
		 public virtual void TxCreateNodeSetPropertyTxAddLabel()
		 {
			  Db.when( Db.tx( createNode().andThen(setProperty("key1", "value1")) ).andThen(Db.tx(addLabel(label("Label1")))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_tx_addLabel_tx_setProperty()
		 public virtual void TxCreateNodeTxAddLabelTxSetProperty()
		 {
			  Db.when( Db.tx( createNode() ).andThen(Db.tx(addLabel(label("Label1"))).andThen(Db.tx(setProperty("key1", "value1")))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tx_createNode_tx_setProperty_tx_addLabel()
		 public virtual void TxCreateNodeTxSetPropertyTxAddLabel()
		 {
			  Db.when( Db.tx( createNode() ).andThen(Db.tx(setProperty("key1", "value1")).andThen(Db.tx(addLabel(label("Label1"))))) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.hamcrest.Matcher<java.util.List<?>> hasSize(final int size)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static Matcher<IList<object>> HasSize( int size )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new org.hamcrest.TypeSafeMatcher<java.util.List<?>>()
			  return new TypeSafeMatcherAnonymousInnerClass( size );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IList<JavaToDotNetGenericWildcard>>
		 {
			 private int _size;

			 public TypeSafeMatcherAnonymousInnerClass( int size )
			 {
				 this._size = size;
			 }

			 protected internal override bool matchesSafely<T1>( IList<T1> item )
			 {
				  return item.Count == _size;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "List with size=" ).appendValue( _size );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Func<org.neo4j.graphdb.GraphDatabaseService, java.util.List<long>> listNodeIdsFromIndexLookup(final org.neo4j.graphdb.Label label, final String propertyKey, final Object value)
		 private System.Func<GraphDatabaseService, IList<long>> ListNodeIdsFromIndexLookup( Label label, string propertyKey, object value )
		 {
			  return graphDb =>
			  {
				List<long> ids = new List<long>();
				foreach ( Node node in loop( graphDb.findNodes( label, propertyKey, value ) ) )
				{
					 ids.add( node.Id );
				}
				return ids;
			  };
		 }

		 public UniqueIndexApplicationIT<T1>( System.Func<T1> createIndex )
		 {
			  this._createIndex = createIndex;
		 }

		 private static object[] CreateIndex( System.Func<GraphDatabaseService, Void> createIndex )
		 {
			  return new object[]{ createIndex };
		 }
	}

}