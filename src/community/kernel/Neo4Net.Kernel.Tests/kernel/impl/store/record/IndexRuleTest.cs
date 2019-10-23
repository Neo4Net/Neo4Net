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
namespace Neo4Net.Kernel.Impl.Store.Records
{
	using Test = org.junit.Test;

	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using Type = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor.Type;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.uniqueForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertException;

	public class IndexRuleTest : SchemaRuleTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateGeneralIndex()
		 public virtual void ShouldCreateGeneralIndex()
		 {
			  // GIVEN
			  IndexDescriptor descriptor = ForLabel( LABEL_ID, PROPERTY_ID_1 );
			  StoreIndexDescriptor indexRule = descriptor.WithId( RULE_ID );

			  // THEN
			  assertThat( indexRule.Id, equalTo( RULE_ID ) );
			  assertFalse( indexRule.CanSupportUniqueConstraint() );
			  assertThat( indexRule.Schema(), equalTo(descriptor.Schema()) );
			  assertThat( indexRule, equalTo( descriptor ) );
			  assertThat( indexRule.ProviderDescriptor(), equalTo(ProviderDescriptor) );
			  assertException( indexRule.getOwningConstraint, typeof( System.InvalidOperationException ) );
			  assertException( () => indexRule.WithOwningConstraint(RULE_ID_2), typeof(System.InvalidOperationException) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniqueIndex()
		 public virtual void ShouldCreateUniqueIndex()
		 {
			  // GIVEN
			  IndexDescriptor descriptor = UniqueForLabel( LABEL_ID, PROPERTY_ID_1 );
			  StoreIndexDescriptor indexRule = descriptor.WithId( RULE_ID );

			  // THEN
			  assertThat( indexRule.Id, equalTo( RULE_ID ) );
			  assertTrue( indexRule.CanSupportUniqueConstraint() );
			  assertThat( indexRule.Schema(), equalTo(descriptor.Schema()) );
			  assertThat( indexRule, equalTo( descriptor ) );
			  assertThat( indexRule.ProviderDescriptor(), equalTo(ProviderDescriptor) );
			  assertThat( indexRule.OwningConstraint, equalTo( null ) );

			  StoreIndexDescriptor withConstraint = indexRule.WithOwningConstraint( RULE_ID_2 );
			  assertThat( withConstraint.OwningConstraint, equalTo( RULE_ID_2 ) );
			  assertThat( indexRule.OwningConstraint, equalTo( null ) ); // this is unchanged
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexRulesAreEqualBasedOnIndexDescriptor()
		 public virtual void IndexRulesAreEqualBasedOnIndexDescriptor()
		 {
			  AssertEqualityByDescriptor( ForLabel( LABEL_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( UniqueForLabel( LABEL_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( ForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );
			  AssertEqualityByDescriptor( UniqueForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detectUniqueIndexWithoutOwningConstraint()
		 public virtual void DetectUniqueIndexWithoutOwningConstraint()
		 {
			  IndexDescriptor descriptor = UniqueForLabel( LABEL_ID, PROPERTY_ID_1 );
			  StoreIndexDescriptor indexRule = descriptor.WithId( RULE_ID );

			  assertTrue( indexRule.IndexWithoutOwningConstraint );
		 }

		 private void AssertEqualityByDescriptor( IndexDescriptor descriptor )
		 {
			  StoreIndexDescriptor rule1 = descriptor.WithId( RULE_ID );
			  StoreIndexDescriptor rule2 = descriptor.WithId( RULE_ID_2 );
			  StoreIndexDescriptor rule3 = ( descriptor.Type() == IndexDescriptor.Type.GENERAL ? forSchema(descriptor.Schema()) : uniqueForSchema(descriptor.Schema()) ).withId(RULE_ID);

			  AssertEquality( rule1, rule2 );
			  AssertEquality( rule1, rule3 );
		 }
	}

}