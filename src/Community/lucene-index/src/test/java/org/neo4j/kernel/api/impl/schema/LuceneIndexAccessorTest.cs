/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using ReporterFactories = Neo4Net.Kernel.Impl.Annotations.ReporterFactories;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class LuceneIndexAccessorTest
	public class LuceneIndexAccessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private SchemaIndex schemaIndex;
		 private SchemaIndex _schemaIndex;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.storageengine.api.schema.IndexDescriptor schemaIndexDescriptor;
		 private IndexDescriptor _schemaIndexDescriptor;
		 private LuceneIndexAccessor _accessor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _accessor = new LuceneIndexAccessor( _schemaIndex, _schemaIndexDescriptor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexIsDirtyWhenLuceneIndexIsNotValid()
		 public virtual void IndexIsDirtyWhenLuceneIndexIsNotValid()
		 {
			  when( _schemaIndex.Valid ).thenReturn( false );
			  assertTrue( _accessor.Dirty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexIsCleanWhenLuceneIndexIsValid()
		 public virtual void IndexIsCleanWhenLuceneIndexIsValid()
		 {
			  when( _schemaIndex.Valid ).thenReturn( true );
			  assertFalse( _accessor.Dirty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexIsNotConsistentWhenIndexIsNotValid()
		 public virtual void IndexIsNotConsistentWhenIndexIsNotValid()
		 {
			  when( _schemaIndex.Valid ).thenReturn( false );
			  assertFalse( _accessor.consistencyCheck( ReporterFactories.noopReporterFactory() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexIsConsistentWhenIndexIsValid()
		 public virtual void IndexIsConsistentWhenIndexIsValid()
		 {
			  when( _schemaIndex.Valid ).thenReturn( true );
			  assertTrue( _accessor.consistencyCheck( ReporterFactories.noopReporterFactory() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexReportInconsistencyToVisitor()
		 public virtual void IndexReportInconsistencyToVisitor()
		 {
			  when( _schemaIndex.Valid ).thenReturn( false );
			  MutableBoolean called = new MutableBoolean();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final InvocationHandler handler = (proxy, method, args) ->
			  InvocationHandler handler = ( proxy, method, args ) =>
			  {
				called.setTrue();
				return null;
			  };
			  assertFalse( "Expected index to be inconsistent", _accessor.consistencyCheck( new ReporterFactory( handler ) ) );
			  assertTrue( "Expected visitor to be called", called.booleanValue() );
		 }
	}

}