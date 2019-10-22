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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Decorator = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntity.NO_LABELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntity.NO_PROPERTIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.additiveLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.decorators;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.defaultRelationshipType;

	public class InputEntityDecoratorsTest
	{
		 private readonly InputEntity _entity = new InputEntity();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideDefaultRelationshipType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideDefaultRelationshipType()
		 {
			  // GIVEN
			  string defaultType = "TYPE";
			  InputEntityVisitor relationship = defaultRelationshipType( defaultType ).apply( _entity );

			  // WHEN
			  relationship( relationship, "source", 1, 0, NO_PROPERTIES, null, "start", "end", null, null );

			  // THEN
			  assertEquals( defaultType, _entity.stringType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverrideAlreadySetRelationshipType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOverrideAlreadySetRelationshipType()
		 {
			  // GIVEN
			  string defaultType = "TYPE";
			  InputEntityVisitor relationship = defaultRelationshipType( defaultType ).apply( _entity );

			  // WHEN
			  string customType = "CUSTOM_TYPE";
			  relationship( relationship, "source", 1, 0, NO_PROPERTIES, null, "start", "end", customType, null );

			  // THEN
			  assertEquals( customType, _entity.stringType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverrideAlreadySetRelationshipTypeId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOverrideAlreadySetRelationshipTypeId()
		 {
			  // GIVEN
			  string defaultType = "TYPE";
			  Decorator decorator = defaultRelationshipType( defaultType );
			  InputEntityVisitor relationship = decorator.apply( _entity );

			  // WHEN
			  int typeId = 5;
			  relationship( relationship, "source", 1, 0, NO_PROPERTIES, null, "start", "end", null, typeId );

			  // THEN
			  assertTrue( _entity.hasIntType );
			  assertEquals( typeId, _entity.intType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddLabelsToNodeWithoutLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddLabelsToNodeWithoutLabels()
		 {
			  // GIVEN
			  string[] toAdd = new string[] { "Add1", "Add2" };
			  InputEntityVisitor node = additiveLabels( toAdd ).apply( _entity );

			  // WHEN
			  node( node, "source", 1, 0, "id", NO_PROPERTIES, null, NO_LABELS, null );

			  // THEN
			  assertArrayEquals( toAdd, _entity.labels() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddMissingLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddMissingLabels()
		 {
			  // GIVEN
			  string[] toAdd = new string[] { "Add1", "Add2" };
			  InputEntityVisitor node = additiveLabels( toAdd ).apply( _entity );

			  // WHEN
			  string[] nodeLabels = new string[] { "SomeOther" };
			  node( node, "source", 1, 0, "id", NO_PROPERTIES, null, nodeLabels, null );

			  // THEN
			  assertEquals( asSet( ArrayUtil.union( toAdd, nodeLabels ) ), asSet( _entity.labels() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTouchLabelsIfNodeHasLabelFieldSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTouchLabelsIfNodeHasLabelFieldSet()
		 {
			  // GIVEN
			  string[] toAdd = new string[] { "Add1", "Add2" };
			  InputEntityVisitor node = additiveLabels( toAdd ).apply( _entity );

			  // WHEN
			  long labelField = 123L;
			  node( node, "source", 1, 0, "id", NO_PROPERTIES, null, null, labelField );

			  // THEN
			  assertEquals( 0, _entity.labels().Length );
			  assertEquals( labelField, _entity.labelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCramMultipleDecoratorsIntoOne()
		 public virtual void ShouldCramMultipleDecoratorsIntoOne()
		 {
			  // GIVEN
			  Decorator decorator1 = spy( new IdentityDecorator() );
			  Decorator decorator2 = spy( new IdentityDecorator() );
			  Decorator multi = decorators( decorator1, decorator2 );

			  // WHEN
			  InputEntityVisitor node = mock( typeof( InputEntityVisitor ) );
			  multi.apply( node );

			  // THEN
			  InOrder order = inOrder( decorator1, decorator2 );
			  order.verify( decorator1, times( 1 ) ).apply( node );
			  order.verify( decorator2, times( 1 ) ).apply( node );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void node(InputEntityVisitor IEntity, String sourceDescription, long lineNumber, long position, Object id, Object[] properties, System.Nullable<long> propertyId, String[] labels, System.Nullable<long> labelField) throws java.io.IOException
		 private static void Node( InputEntityVisitor IEntity, string sourceDescription, long lineNumber, long position, object id, object[] properties, long? propertyId, string[] labels, long? labelField )
		 {
			  ApplyProperties( IEntity, properties, propertyId );
			  IEntity.Id( id, Group_Fields.Global );
			  if ( labelField != null )
			  {
					entity.LabelField( labelField.Value );
			  }
			  else
			  {
					entity.Labels( labels );
			  }
			  IEntity.EndOfEntity();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void relationship(InputEntityVisitor IEntity, String sourceDescription, long lineNumber, long position, Object[] properties, System.Nullable<long> propertyId, Object startNode, Object endNode, String type, System.Nullable<int> typeId) throws java.io.IOException
		 private static void Relationship( InputEntityVisitor IEntity, string sourceDescription, long lineNumber, long position, object[] properties, long? propertyId, object startNode, object endNode, string type, int? typeId )
		 {
			  ApplyProperties( IEntity, properties, propertyId );
			  IEntity.StartId( startNode, Group_Fields.Global );
			  IEntity.EndId( endNode, Group_Fields.Global );
			  if ( typeId != null )
			  {
					entity.type( typeId );
			  }
			  else if ( !string.ReferenceEquals( type, null ) )
			  {
					entity.Type( type );
			  }
			  IEntity.EndOfEntity();
		 }

		 private static void ApplyProperties( InputEntityVisitor IEntity, object[] properties, long? propertyId )
		 {
			  if ( propertyId != null )
			  {
					entity.PropertyId( propertyId.Value );
			  }
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					entity.Property( ( string ) properties[i++], properties[i] );
			  }
		 }

		 private class IdentityDecorator : Decorator
		 {
			  public override InputEntityVisitor Apply( InputEntityVisitor IEntity )
			  {
					return IEntity;
			  }
		 }
	}

}