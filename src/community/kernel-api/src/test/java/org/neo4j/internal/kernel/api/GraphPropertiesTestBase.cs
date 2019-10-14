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
namespace Neo4Net.Internal.Kernel.Api
{
	using Test = org.junit.Test;

	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class GraphPropertiesTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class GraphPropertiesTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteNewGraphProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteNewGraphProperty()
		 {
			  int prop;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( testSupport.graphProperties().getProperty("prop"), equalTo("hello") );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReplaceExistingGraphProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReplaceExistingGraphProperty()
		 {
			  int prop;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("good bye")), equalTo(stringValue("hello")) );
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( testSupport.graphProperties().getProperty("prop"), equalTo("good bye") );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveExistingGraphProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemoveExistingGraphProperty()
		 {
			  int prop;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertThat( tx.DataWrite().graphRemoveProperty(prop), equalTo(stringValue("hello")) );
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertFalse( testSupport.graphProperties().hasProperty("prop") );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReadExistingGraphProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReadExistingGraphProperties()
		 {
			  int prop1, prop2, prop3;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop1 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop1");
					prop2 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop2");
					prop3 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop3");
					tx.DataWrite().graphSetProperty(prop1, stringValue("hello"));
					tx.DataWrite().graphSetProperty(prop2, stringValue("world"));
					tx.DataWrite().graphSetProperty(prop3, stringValue("etc"));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction(), PropertyCursor cursor = tx.Cursors().allocatePropertyCursor() )
			  {
					tx.DataRead().graphProperties(cursor);

					assertTrue( cursor.Next() );
					assertThat( cursor.PropertyKey(), equalTo(prop1) );
					assertThat( cursor.PropertyValue(), equalTo(stringValue("hello")) );

					assertTrue( cursor.Next() );
					assertThat( cursor.PropertyKey(), equalTo(prop2) );
					assertThat( cursor.PropertyValue(), equalTo(stringValue("world")) );

					assertTrue( cursor.Next() );
					assertThat( cursor.PropertyKey(), equalTo(prop3) );
					assertThat( cursor.PropertyValue(), equalTo(stringValue("etc")) );

					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNewGraphPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNewGraphPropertyInTransaction()
		 {
			  using ( Transaction tx = beginTransaction(), PropertyCursor cursor = tx.Cursors().allocatePropertyCursor() )
			  {
					int prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );

					tx.DataRead().graphProperties(cursor);
					assertTrue( cursor.Next() );
					assertThat( cursor.PropertyKey(), equalTo(prop) );
					assertThat( cursor.PropertyValue(), equalTo(stringValue("hello")) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeUpdatedGraphPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeUpdatedGraphPropertyInTransaction()
		 {
			  int prop;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction(), PropertyCursor cursor = tx.Cursors().allocatePropertyCursor() )
			  {
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("good bye")), equalTo(stringValue("hello")) );

					tx.DataRead().graphProperties(cursor);
					assertTrue( cursor.Next() );
					assertThat( cursor.PropertyKey(), equalTo(prop) );
					assertThat( cursor.PropertyValue(), equalTo(stringValue("good bye")) );
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeRemovedGraphPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeRemovedGraphPropertyInTransaction()
		 {
			  int prop;
			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					assertThat( tx.DataWrite().graphSetProperty(prop, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction(), PropertyCursor cursor = tx.Cursors().allocatePropertyCursor() )
			  {
					assertThat( tx.DataWrite().graphRemoveProperty(prop), equalTo(stringValue("hello")) );

					tx.DataRead().graphProperties(cursor);
					assertFalse( cursor.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWriteWhenSettingPropertyToSameValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWriteWhenSettingPropertyToSameValue()
		 {
			  // Given
			  int prop;
			  Value theValue = stringValue( "The Value" );

			  using ( Transaction tx = beginTransaction() )
			  {
					prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					tx.DataWrite().graphSetProperty(prop, theValue);
					tx.Success();
			  }

			  // When
			  Transaction tx = beginTransaction();
			  assertThat( tx.DataWrite().graphSetProperty(prop, theValue), equalTo(theValue) );
			  tx.Success();

			  assertThat( tx.CloseTransaction(), equalTo(Transaction_Fields.READ_ONLY) );
		 }
	}

}