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

	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.constraints.ConstraintDescriptorFactory.existsForLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.constraints.ConstraintDescriptorFactory.nodeKeyForLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertException;

	public class ConstraintRuleTest : SchemaRuleTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquenessConstraint()
		 public virtual void ShouldCreateUniquenessConstraint()
		 {
			  // GIVEN
			  ConstraintDescriptor descriptor = ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 );
			  ConstraintRule constraintRule = ConstraintRule.ConstraintRuleConflict( RULE_ID, descriptor );

			  // THEN
			  assertThat( constraintRule.Id, equalTo( RULE_ID ) );
			  assertThat( constraintRule.Schema(), equalTo(descriptor.Schema()) );
			  assertThat( constraintRule.ConstraintDescriptor, equalTo( descriptor ) );
			  assertException( constraintRule.getOwnedIndex, typeof( System.InvalidOperationException ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquenessConstraintWithOwnedIndex()
		 public virtual void ShouldCreateUniquenessConstraintWithOwnedIndex()
		 {
			  // GIVEN
			  UniquenessConstraintDescriptor descriptor = ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 );
			  ConstraintRule constraintRule = ConstraintRule.constraintRule( RULE_ID, descriptor, RULE_ID_2 );

			  // THEN
			  assertThat( constraintRule.ConstraintDescriptor, equalTo( descriptor ) );
			  assertThat( constraintRule.OwnedIndex, equalTo( RULE_ID_2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeKeyConstraint()
		 public virtual void ShouldCreateNodeKeyConstraint()
		 {
			  // GIVEN
			  ConstraintDescriptor descriptor = nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 );
			  ConstraintRule constraintRule = ConstraintRule.ConstraintRuleConflict( RULE_ID, descriptor );

			  // THEN
			  assertThat( constraintRule.Id, equalTo( RULE_ID ) );
			  assertThat( constraintRule.Schema(), equalTo(descriptor.Schema()) );
			  assertThat( constraintRule.ConstraintDescriptor, equalTo( descriptor ) );
			  assertException( constraintRule.getOwnedIndex, typeof( System.InvalidOperationException ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeKeyConstraintWithOwnedIndex()
		 public virtual void ShouldCreateNodeKeyConstraintWithOwnedIndex()
		 {
			  // GIVEN
			  NodeKeyConstraintDescriptor descriptor = nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 );
			  ConstraintRule constraintRule = ConstraintRule.constraintRule( RULE_ID, descriptor, RULE_ID_2 );

			  // THEN
			  assertThat( constraintRule.ConstraintDescriptor, equalTo( descriptor ) );
			  assertThat( constraintRule.OwnedIndex, equalTo( RULE_ID_2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExistenceConstraint()
		 public virtual void ShouldCreateExistenceConstraint()
		 {
			  // GIVEN
			  ConstraintDescriptor descriptor = existsForLabel( LABEL_ID, PROPERTY_ID_1 );
			  ConstraintRule constraintRule = ConstraintRule.ConstraintRuleConflict( RULE_ID, descriptor );

			  // THEN
			  assertThat( constraintRule.Id, equalTo( RULE_ID ) );
			  assertThat( constraintRule.Schema(), equalTo(descriptor.Schema()) );
			  assertThat( constraintRule.ConstraintDescriptor, equalTo( descriptor ) );
			  assertException( constraintRule.getOwnedIndex, typeof( System.InvalidOperationException ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexRulesAreEqualBasedOnConstraintDescriptor()
		 public virtual void IndexRulesAreEqualBasedOnConstraintDescriptor()
		 {
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.existsForRelType( REL_TYPE_ID, PROPERTY_ID_1 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.existsForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );
			  AssertEqualityByDescriptor( ConstraintDescriptorFactory.nodeKeyForLabel( LABEL_ID, PROPERTY_ID_1, PROPERTY_ID_2 ) );
		 }

		 private void AssertEqualityByDescriptor( UniquenessConstraintDescriptor descriptor )
		 {
			  ConstraintRule rule1 = ConstraintRule.constraintRule( RULE_ID, descriptor, RULE_ID_2 );
			  ConstraintRule rule2 = ConstraintRule.ConstraintRuleConflict( RULE_ID_2, descriptor );

			  AssertEquality( rule1, rule2 );
		 }

		 private void AssertEqualityByDescriptor( ConstraintDescriptor descriptor )
		 {
			  ConstraintRule rule1 = ConstraintRule.ConstraintRuleConflict( RULE_ID, descriptor );
			  ConstraintRule rule2 = ConstraintRule.ConstraintRuleConflict( RULE_ID_2, descriptor );

			  AssertEquality( rule1, rule2 );
		 }
	}

}